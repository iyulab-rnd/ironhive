﻿using Raggle.Abstractions;

namespace Raggle.Core;

public class RaggleService : IRaggleService
{
    //private const string DEFAULT_INDEX = "default";
    //private readonly IChatCompletionService _chat;
    //private readonly IKernelMemory _memory;
    //private readonly IPromptProvider? _prompt;

    //public ChatHistory? History { get; private set; }

    //public RaggleService(
    //    IChatCompletionService chatService, 
    //    IKernelMemory kernelMemory,
    //    IPromptProvider? promptProvider = null)
    //{
    //    _memory = kernelMemory;
    //    _chat = chatService;
    //    _prompt = promptProvider;
    //}

    //public async Task<string> MemorizeTextAsync(string documentId, string text, string? index = null)
    //{
    //    index ??= DEFAULT_INDEX;
    //    return await _memory.ImportTextAsync(text: text, documentId: documentId, index: index);
    //}

    //public async Task<string> MemorizeDocumentAsync(string documentId, string filePath, string? index = null)
    //{
    //    index ??= DEFAULT_INDEX;
    //    var isExist = await _memory.IsDocumentReadyAsync(documentId: documentId, index: index);
    //    return isExist
    //        ? documentId
    //        : await _memory.ImportDocumentAsync(filePath: filePath, documentId: documentId, index: index);
    //}

    //public async Task<string> MemorizeWebPageAsync(string documentId, string url, string? index = null)
    //{
    //    index ??= DEFAULT_INDEX;
    //    var isExist = await _memory.IsDocumentReadyAsync(documentId: documentId, index: index);
    //    return isExist
    //        ? documentId
    //        : await _memory.ImportWebPageAsync(url: url, documentId: documentId, index: index);
    //}

    //public async Task UnMemorizeAsync(string documentId, string? index = null)
    //{
    //    index ??= DEFAULT_INDEX;
    //    await _memory.DeleteDocumentAsync(documentId: documentId, index: index);
    //}

    //public async Task<DataPipelineStatus?> GetMemorizeStatusAsync(string documentId, string? index = null)
    //{
    //    index ??= DEFAULT_INDEX;
    //    var status = await _memory.GetDocumentStatusAsync(documentId: documentId, index: index);
    //    return status;
    //}

    //public async Task<string> GetInformationAsync(
    //    string query, 
    //    int? limit = null, 
    //    double? minRelevance = null, 
    //    string? index = null,
    //    ICollection<MemoryFilter>? filters = null)
    //{
    //    index ??= DEFAULT_INDEX;
    //    var memories = await _memory.SearchAsync(
    //            query: query,
    //            index: index,
    //            limit: limit ?? 10,
    //            minRelevance: minRelevance ?? 0,
    //            filters: filters
    //        );
    //    return memories.Results.SelectMany(m => m.Partitions)
    //        .Aggregate("", (sum, chunk) => sum + chunk.Text + "\n")
    //        .Trim();
    //}

    //public async IAsyncEnumerable<string> AskStreamingAsync(
    //    string query, 
    //    ICollection<MemoryFilter>? filters = null)
    //{
    //    var information = await GetInformationAsync(query: query, filters: filters);
    //    if (History is null)
    //    {
    //        History = new ChatHistory();
    //        History.AddSystemMessage(_prompt?.GetPromptWithInfo(information) ?? $"[information]\n{information}");
    //    }
    //    else
    //    {
    //        History[0].Content = _prompt?.GetPromptWithInfo(information) ?? $"[information]\n{information}";
    //    }
        
    //    History.AddUserMessage(query);
    //    var reply = new StringBuilder();
    //    await foreach (var stream in _chat.GetStreamingChatMessageContentsAsync(History))
    //    {
    //        var content = stream.Content;
    //        if (content is not null)
    //        {
    //            reply.Append(content);
    //            yield return content;
    //        }
    //    }
    //    History.AddAssistantMessage(reply.ToString());
    //}

    //public void ClearHistory()
    //{
    //    History?.Clear();
    //}
}
