﻿using System.Collections;

namespace IronHive.Abstractions.Messages;

public class UserContentCollection : ICollection<IUserContent>
{
    private readonly List<IUserContent> _items = new();

    /// <summary>
    /// TextContent를 추가합니다.
    /// </summary>
    public void AddText(string? value)
    {
        Add(new UserTextContent
        {
            Value = value
        });
    }

    /// <summary>
    /// ImageContent를 추가합니다.
    /// </summary>
    public void AddImage(string? data)
    {
        Add(new UserImageContent
        {
            Data = data
        });
    }

    /// <summary>
    /// 배열을 추가합니다.
    /// </summary>
    public void AddRange(IEnumerable<IUserContent> array)
    {
        foreach (var item in array)
        {
            Add(item);
        }
    }

    #region ICollection Implementations

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public IUserContent this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    /// <summary>
    /// 아이템을 추가합니다.
    /// 아이템의 인덱스 번호는 자동으로 재할당됩니다.
    /// </summary>
    public void Add(IUserContent item)
    {
        item.Index = _items.Count;
        _items.Add(item);
    }

    public void Clear() => _items.Clear();

    public bool Contains(IUserContent item) => _items.Contains(item);

    public void CopyTo(IUserContent[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(IUserContent item) => _items.Remove(item);

    public IEnumerator<IUserContent> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
