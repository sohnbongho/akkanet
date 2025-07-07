using Library.DTO;
using Library.MessageHandling;
using Library.messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchMark.Test.Attributes;

public interface IItemPolicy
{
    MainItemType MainItemType { get; }
    int GetTest();
}

[MainItemTypeHandler(MainItemType.Character)]
public class CharacterItemPolicy : IItemPolicy
{
    public MainItemType MainItemType { get; private set; } = MainItemType.Character;
    public int GetTest() { return (int)MainItemType; }
}

[MainItemTypeHandler(MainItemType.Clothing)]
public class ClothItemPolicy : IItemPolicy
{
    public MainItemType MainItemType { get; private set; } = MainItemType.Clothing;
    public int GetTest() { return (int)MainItemType; }
}
