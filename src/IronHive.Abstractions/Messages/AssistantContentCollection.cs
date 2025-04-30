﻿using System.Collections;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// IAssistantContent를 담는 컬렉션 구현입니다.
/// </summary>
public class AssistantContentCollection : ICollection<IAssistantContent>
{
    private readonly List<IAssistantContent> _items = new();

    /// <summary>
    /// TextContent를 추가합니다.
    /// 인덱스 번호는 자동으로 재할당됩니다.
    /// </summary>
    public void AddText(string? value)
    {
        Add(new AssistantTextContent
        {
            Value = value
        });
    }

    /// <summary>
    /// ToolContent를 추가합니다.
    /// 인덱스 번호는 자동으로 재할당됩니다.
    /// </summary>
    public void AddTool(string? id, string? name, string? arguments, string? result)
    {
        Add(new AssistantToolContent
        {
            Id = id,
            Name = name,
            Arguments = arguments,
            Result = result
        });
    }

    /// <summary>
    /// ThinkingContent를 추가합니다.
    /// 인덱스 번호는 자동으로 재할당됩니다.
    /// </summary>
    public void AddThinking(string? id, string? value)
    {
        Add(new AssistantThinkingContent
        {
            Id = id,
            Value = value
        });
    }

    /// <summary>
    /// 배열을 추가합니다. 
    /// 인덱스 번호는 자동으로 재할당됩니다.
    /// </summary>
    public void AddRange(IEnumerable<IAssistantContent> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }

    /// <summary>
    /// 아이템을 동일한 타입에 따라 분리합니다.
    /// 예) [ToolContent, ToolContent, TextContent, ToolContent]
    ///  => [ToolContent, ToolContent], [TextContent], [ToolContent]
    /// </summary>
    public IEnumerable<(Type, AssistantContentCollection)> Split()
    {
        var result = new List<(Type, AssistantContentCollection)>();
        var group = new AssistantContentCollection();
        Type? groupType = null;

        foreach (var item in _items)
        {
            var currentType = item.GetType();
            if (group.Count == 0)
            {
                // 그룹이 비어있으면 현재 아이템 타입을 기준으로 그룹 시작
                groupType = currentType;
                group.Add(item);
            }
            else if (currentType != groupType)
            {
                // 현재 아이템 타입과 이전 그룹의 타입이 다르면 그룹 분리 
                result.Add((groupType!, group));
                group = new AssistantContentCollection();
                groupType = currentType;
                group.Add(item);
            }
            else
            {
                // 현재 아이템 타입과 이전 그룹의 타입이 같으면 그룹에 추가
                group.Add(item);
            }
        }

        // 마지막 그룹이 있다면 추가합니다.
        if (group.Count > 0)
        {
            result.Add((groupType!, group));
        }

        return result;
    }

    #region ICollection Implementations

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public IAssistantContent this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    /// <summary>
    /// 아이템을 추가합니다.
    /// 아이템의 인덱스 번호는 자동으로 재할당됩니다.
    /// </summary>
    public void Add(IAssistantContent item)
    {
        item.Index = _items.Count;
        _items.Add(item);
    }

    public void Clear() => _items.Clear();

    public bool Contains(IAssistantContent item) => _items.Contains(item);

    public void CopyTo(IAssistantContent[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(IAssistantContent item) => _items.Remove(item);

    public IEnumerator<IAssistantContent> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
