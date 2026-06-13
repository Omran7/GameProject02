using Microsoft.Maui.Controls.Xaml;
using System;
using System.Reflection;

namespace GameProject02.Helpers;

[ContentProperty(nameof(Member))]
public class StaticExtension : IMarkupExtension
{
    public string Member { get; set; } = string.Empty;

    public object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Member))
            return null;

        var dotIndex = Member.LastIndexOf('.');
        if (dotIndex < 0)
            throw new ArgumentException("Member must be in format 'ClassName.PropertyName'");

        var className = Member.Substring(0, dotIndex);
        var propertyName = Member.Substring(dotIndex + 1);

        Type? type = null;
        var namespaces = new[] { "GameProject02.Helpers", "GameProject02.Models", "GameProject02.Services" };
        foreach (var ns in namespaces)
        {
            type = Type.GetType($"{ns}.{className}");
            if (type != null) break;
        }

        if (type == null)
            throw new ArgumentException($"Type '{className}' not found.");

        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
        if (property == null)
            throw new ArgumentException($"Static property '{propertyName}' not found in type '{className}'.");

        return property.GetValue(null);
    }
}