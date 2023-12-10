using System.Collections.Generic;
using Hazel;
using TOHE.Modules;
using static TOHE.Translator;
using static TOHE.Options;

namespace TOHE.Roles.Impostor;

public static class LimitedKiller
{
    private static readonly int Id = 13550;

    public static OptionItem KillLimit;
    private static OptionItem KillCooldown;
    public static Dictionary<byte, int> AbilityLimit = new();

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.LimitedKiller);
        KillLimit = IntegerOptionItem.Create(Id + 1, "KillLimit", new(1, 15, 1), 8, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.LimitedKiller])
            .SetValueFormat(OptionFormat.Multiplier);
        KillCooldown = FloatOptionItem.Create(Id + 2, "KillCooldown", new(0f, 180f, 2.5f), 30f, TabGroup.ImpostorRoles,  false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.LimitedKiller])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public static void Init()
    {
        AbilityLimit = new();
    }
    public static void Add(byte playerId)
    {
        AbilityLimit.TryAdd(playerId, KillLimit.GetInt());
    }

    public static void SetKillCooldown(this PlayerControl player) => Main.AllPlayerKillCooldown[player.PlayerId] = CanUseKillButton(player) ? KillCooldown.GetFloat() : 300f;
    private static bool CanKill(byte playerId) => AbilityLimit[playerId] >= 1;
    public static bool CanUseKillButton(PlayerControl player) => !player.Data.IsDead && CanKill(player.PlayerId);

    private static void SendRPC(byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLimitedKillLimit, SendOption.Reliable, -1);
        writer.Write(playerId);
        writer.Write(AbilityLimit[playerId]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC(MessageReader reader)
    {
        byte LimitedId = reader.ReadByte();
        int Limit = reader.ReadInt32();
        if (AbilityLimit.ContainsKey(LimitedId))
            AbilityLimit[LimitedId] = Limit;
        else
            AbilityLimit.Add(LimitedId, KillLimit.GetInt());
    }

    public static void UpdateLimit(byte killerId) => AbilityLimit[killerId]--; // on check murder
}