using Library.DTO;
using System;

namespace BenchMark.Test.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MainItemTypeHandlerAttribute : Attribute
{
    public MainItemType MessageType { get; }

    public MainItemTypeHandlerAttribute(MainItemType messageType)
    {
        MessageType = messageType;
    }
}
