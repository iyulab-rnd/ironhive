﻿using System.Text.Json;
using System.Runtime.CompilerServices;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Messages;
using IronHive.Connectors.Anthropic.ChatCompletion;
using IronHive.Connectors.Anthropic.Clients;
using TokenUsage = IronHive.Abstractions.ChatCompletion.TokenUsage;

namespace IronHive.Connectors.Anthropic;

public class AnthropicChatCompletionConnector : IChatCompletionConnector
{
    private readonly AnthropicChatCompletionClient _client;

    public AnthropicChatCompletionConnector(AnthropicConfig config)
    {
        _client = new AnthropicChatCompletionClient(config);
    }

    public AnthropicChatCompletionConnector(string apiKey)
    {
        _client = new AnthropicChatCompletionClient(apiKey);
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse<AssistantMessage>> GenerateMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = ConvertRequest(request);
        var res = await _client.PostMessagesAsync(req, cancellationToken);
        var message = new AssistantMessage();

        foreach (var item in res.Content ?? [])
        {
            if (item is TextMessageContent text)
            {
                // 텍스트 생성
                message.Content.AddText(text.Text);
            }
            else if (item is ToolUseMessageContent tool)
            {
                // 툴 사용
                message.Content.AddTool(tool.Id, tool.Name, JsonSerializer.Serialize(tool.Input), null);
            }
            else
            {
                throw new NotSupportedException($"Unexpected content type [{item.GetType()}] not supported yet");
            }
        }

        return new ChatCompletionResponse<AssistantMessage>
        {
            EndReason = res.StopReason switch
            {
                StopReason.ToolUse => EndReason.ToolCall,
                StopReason.EndTurn => EndReason.EndTurn,
                StopReason.MaxTokens => EndReason.MaxTokens,
                StopReason.StopSequence => EndReason.StopSequence,
                _ => null
            },
            TokenUsage = new TokenUsage
            {
                InputTokens = res.Usage.InputTokens,
                OutputTokens = res.Usage.OutputTokens
            },
            Data = message
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResponse<IAssistantContent>> GenerateStreamingMessageAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = ConvertRequest(request);

        EndReason? reason = null;
        TokenUsage usage = new TokenUsage();
        await foreach (var res in _client.PostStreamingMessagesAsync(req, cancellationToken))
        {
            if (res is MessageStartEvent mse)
            {
                // 1. 메시지 시작 이벤트
                usage.InputTokens = mse.Message?.Usage.InputTokens;
            }
            else if (res is ContentStartEvent cse)
            {
                // 2-1. 컨텐츠 블록 시작 이벤트

                if (cse.ContentBlock is TextMessageContent text)
                {
                    // 텍스트 생성
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantTextContent
                        {
                            Index = cse.Index,
                            Value = text.Text
                        }
                    };
                }
                else if (cse.ContentBlock is ToolUseMessageContent tool)
                {
                    // 툴 사용
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantToolContent
                        {
                            Index = cse.Index,
                            Id = tool.Id,
                            Name = tool.Name
                        }
                    };
                }
            }
            else if (res is ContentDeltaEvent cde)
            {
                // 2-2. 컨텐츠 블록 생성 진행 이벤트

                if (cde.Delta is TextDeltaMessageContent text)
                {
                    // 텍스트 생성
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantTextContent
                        {
                            Index = cde.Index,
                            Value = text.Text
                        }
                    };
                }
                else if (cde.Delta is ToolUseDeltaMessageContent tool)
                {
                    // 툴 사용
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantToolContent
                        {
                            Index = cde.Index,
                            Arguments = tool.PartialJson,
                        }
                    };
                }
            }
            else if (res is ContentStopEvent)
            {
                // 2-3. 컨텐츠 블록 생성 종료 이벤트
            }
            else if (res is MessageDeltaEvent mde)
            {
                // 3. 메시지 정보 이벤트
                reason = mde.Delta?.StopReason switch
                {
                    StopReason.ToolUse => EndReason.ToolCall,
                    StopReason.EndTurn => EndReason.EndTurn,
                    StopReason.StopSequence => EndReason.StopSequence,
                    StopReason.MaxTokens => EndReason.MaxTokens,
                    _ => null
                };
                usage.OutputTokens = mde.Usage?.OutputTokens;
            }
            else if (res is MessageStopEvent)
            {
                // 4. 메시지 종료 이벤트
            }
            else if (res is PingEvent)
            {
                // 핑 이벤트
            }
            else if (res is ErrorEvent error)
            {
                // 에러 이벤트
                throw new Exception(JsonSerializer.Serialize(error.Error));
            }
            else
            {
                // Unknown
                throw new NotSupportedException($"Unexpected event type: {res.GetType()}");
            }
        }

        yield return new ChatCompletionResponse<IAssistantContent>
        {
            EndReason = reason,
            TokenUsage = usage
        };
    }

    private static MessagesRequest ConvertRequest(ChatCompletionRequest request)
    {
        var _req = new MessagesRequest
        {
            Model = request.Model,
            System = request.System,
            Messages = request.Messages.ToAnthropic(),
            MaxTokens = request.MaxTokens ?? 8192, // MaxToken이 필수요청사항으로 "8192"값을 기본으로 함
            Temperature = request.Temperature,
            TopK = request.TopK,
            TopP = request.TopP,
            StopSequences = request.StopSequences,
        };

        _req.Tools = request.Tools.Select(t => new Tool
        {
            Name = t.Name,
            Description = t.Description,
            InputSchema = t.InputSchema ?? new ObjectJsonSchema()
        });

        return _req;
    }
}
