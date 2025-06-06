﻿using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <inheritdoc />
public class ToolPluginManager : IToolPluginManager
{
    private const char Delimiter = '_';
    private readonly Dictionary<string, IToolPlugin> _plugins;

    public ToolPluginManager(IEnumerable<IToolPlugin> plugins)
    {
        _plugins = plugins.ToDictionary(plugin => plugin.PluginName, plugin => plugin);
    }

    /// <inheritdoc />
    public bool ContainsPlugin(string name)
    {
        return _plugins.ContainsKey(name);
    }

    /// <inheritdoc />
    public bool TryAddPlugin(IToolPlugin plugin)
    {
        return _plugins.TryAdd(plugin.PluginName, plugin);
    }

    /// <inheritdoc />
    public bool TryRemovePlugin(string name)
    {
        return _plugins.Remove(name);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ToolDescriptor>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var tasks = _plugins.Select(async kvp =>
        {
            var (pluginName, plugin) = (kvp.Key, kvp.Value);
            var tools = (await plugin.ListAsync(cancellationToken))
                .Select(tool =>
                {
                    tool.Name = $"{pluginName}{Delimiter}{tool.Name}";
                    return tool;
                }).ToList();

            return tools;
        });

        var toolLists = await Task.WhenAll(tasks);
        return toolLists.SelectMany(t => t);
    }

    /// <inheritdoc />
    public Task<ToolOutput> InvokeAsync(
        string name,
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        var parts = name.Split(Delimiter, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            throw new ArgumentException($"Invalid tool name '{name}'.");
        }

        var (pluginName, toolName) = (parts[0], parts[1]);
        if (!_plugins.TryGetValue(pluginName, out var plugin))
        {
            throw new KeyNotFoundException($"Tool '{name}' not found.");
        }

        return plugin.InvokeAsync(toolName, input, cancellationToken);
    }
}
