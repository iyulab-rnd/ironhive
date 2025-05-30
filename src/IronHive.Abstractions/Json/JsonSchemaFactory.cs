﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using Json.Schema;
using Json.Schema.Generation;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using JsonSchemaNet = Json.Schema.JsonSchema;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace IronHive.Abstractions.Json;

/// <summary>
/// Factory class for creating JSON schemas from .NET types.
/// </summary>
public static class JsonSchemaFactory
{
    /// <summary>
    /// Creates a new JSON schema from a .NET type using the Json.Schema library.
    /// </summary>
    [Obsolete("Use CreateFrom<T>() instead. This method will be removed in a future version.")]
    public static JsonSchemaNet CreateNew<T>()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<T>()
            .Build();
        return schema;
    }

    /// <summary>
    /// Creates a JSON schema from a .NET type.
    /// </summary>
    public static JsonSchema CreateFrom<T>(string? description = null)
    {
        return CreateFrom(typeof(T), description);
    }

    /// <summary>
    /// Creates a JSON schema from a .NET type.
    /// </summary>
    public static JsonSchema CreateFrom(Type type, string? description = null)
    {
        // boolean type
        if (type == typeof(bool))
            return new BooleanJsonSchema(description);

        // integer type
        if (type == typeof(byte))
            return new IntegerJsonSchema(description) { Format = "byte" };
        if (type == typeof(sbyte))
            return new IntegerJsonSchema(description) { Format = "sbyte" };
        if (type == typeof(short))
            return new IntegerJsonSchema(description) { Format = "short" };
        if (type == typeof(ushort))
            return new IntegerJsonSchema(description) { Format = "ushort" };
        if (type == typeof(int))
            return new IntegerJsonSchema(description) { Format = "int32" };
        if (type == typeof(uint))
            return new IntegerJsonSchema(description) { Format = "uint32" };
        if (type == typeof(long))
            return new IntegerJsonSchema(description) { Format = "int64" };
        if (type == typeof(ulong))
            return new IntegerJsonSchema(description) { Format = "uint64" };

        // number type
        if (type == typeof(float))
            return new NumberJsonSchema(description) { Format = "float" };
        if (type == typeof(double))
            return new NumberJsonSchema(description) { Format = "double" };
        if (type == typeof(decimal))
            return new NumberJsonSchema(description) { Format = "decimal" };

        // string type
        if (type == typeof(char))
            return new StringJsonSchema(description) { MaxLength = 1, MinLength = 1 };
        if (type == typeof(string))
            return new StringJsonSchema(description);
        if (type == typeof(TimeSpan))
            return new StringJsonSchema(description) { Format = "duration" };
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            return new StringJsonSchema(description) { Format = "date-time" };
        if (type == typeof(Uri))
            return new StringJsonSchema(description) { Format = "uri" };
        if (type == typeof(Guid))
            return new StringJsonSchema(description) { Format = "guid" };
        if (type.IsEnum)
            return new StringJsonSchema(description) { Enum = Enum.GetNames(type) };

        // array type (Tuple, IEnumerable, ICollection, IList, List, Array)
        if (type.IsArray)
        {
            var genericType = type.GetElementType()
                ?? throw new ArgumentException("Array type must have an element type.", nameof(type));
            var items = CreateFrom(genericType);
            return new ArrayJsonSchema(description) { Items = items };
        }
        if (IsGenericArray(type))
        {
            var genericType = type.GetGenericArguments()[0];
            var items = CreateFrom(genericType);
            return new ArrayJsonSchema(description) { Items = items };
        }
        if (IsTuple(type))
        {
            var genericTypes = type.GetGenericArguments();
            var items = genericTypes.Select(t => CreateFrom(t)).ToArray();
            return new ArrayJsonSchema(description) { Items = items };
        }

        // object type (IDictionary, class, struct, record)
        if (IsDictionary(type))
        {
            var valueType = type.GetGenericArguments()[1];
            var additionalProperties = CreateFrom(valueType);
            return new ObjectJsonSchema
            {
                Description = description,
                AdditionalProperties = additionalProperties
            };
        }
        if (IsComplexObject(type))
        {
            var properties = new Dictionary<string, JsonSchema>();
            var required = new List<string>();

            foreach (var prop in type.GetProperties())
            {
                var propType = prop.PropertyType;
                var propDescription = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;
                var propSchema = CreateFrom(propType, propDescription);
                properties.Add(prop.Name, propSchema);

                if (IsRequiredProperty(prop))
                    required.Add(prop.Name);
            }

            return new ObjectJsonSchema
            {
                Description = description,
                Properties = properties,
                Required = required.Count > 0 ? required.ToArray() : null
            };
        }

        throw new ArgumentException("Type not supported.", nameof(type));
    }

    #region Private Methods

    private static bool IsGenericArray(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var gType = type.GetGenericTypeDefinition();
        return gType == typeof(IEnumerable<>) || gType == typeof(ICollection<>) || gType == typeof(IList<>)
            || gType == typeof(List<>) || gType == typeof(IReadOnlyList<>) || gType == typeof(IReadOnlyCollection<>);
    }

    private static bool IsTuple(Type type)
    {
        var interfaces = type.GetInterfaces();
        return interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITuple));
    }

    private static bool IsDictionary(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    private static bool IsComplexObject(Type type)
    {
        // Class
        if (type.IsClass && !type.IsAbstract && !type.IsInterface)
            return true;

        // Struct, Record
        if (type.IsValueType && !type.IsPrimitive && !type.IsEnum)
            return true;

        return false;
    }

    private static bool IsRequiredProperty(PropertyInfo prop)
    {
        if (prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null)
            return true;

        if (prop.GetCustomAttribute<RequiredAttribute>() != null)
            return true;

        if (prop.GetCustomAttribute<RequiredMemberAttribute>() != null)
            return true;

        return false;
    }

    #endregion
}
