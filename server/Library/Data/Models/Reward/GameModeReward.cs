using Library.DTO;

namespace Library.Data.Models.Reward;

public interface IGameModeReward
{
    int ItemSn { get; set; }
    long Amount { get; set; }
}

public class NoneReward : IGameModeReward
{
    public InGameItemType RewardType { get; } = InGameItemType.None;
    public int ItemSn { get; set; }
    public long Amount { get; set; }
}


public class CurrencyReward : IGameModeReward
{    
    public int ItemSn { get; set; }
    public long Amount { get; set; }
}
public class ShelterReward : IGameModeReward
{
    public InGameItemType RewardType { get; } = InGameItemType.Shelter;
    public int ItemSn { get; set; }
    public long Amount { get; set; }
    public int SlotId { get; set; } = (int)InGameItemType.Shelter;
}



public class SnackOrToyReward : IGameModeReward
{
    public InGameItemType RewardType { get; } = InGameItemType.SnackOrToy;
    public int ItemSn { get; set; }
    public long Amount { get; set; }
    public int SlotId { get; set; } = (int)InGameItemType.SnackOrToy;
}

public class TransportReward : IGameModeReward
{
    public InGameItemType RewardType { get; } = InGameItemType.Transport;
    public int ItemSn { get; set; }
    public long Amount { get; set; }
    public int SlotId { get; set; } = (int)InGameItemType.Transport;
}

public class BuffReward : IGameModeReward
{
    public InGameItemType RewardType { get; } = InGameItemType.SpeedBuff;
    public int ItemSn { get; set; }
    public long Amount { get; set; }
}

public class BadgeReward : IGameModeReward
{
    public InGameItemType RewardType { get; } = InGameItemType.Badge;
    public int ItemSn { get; set; }
    public long Amount { get; set; }
    public int SlotId { get; set; } = (int)InGameItemType.Badge;
}
