using System.Collections.Generic;
using Hazel;
using TOHE.Modules;
using UnityEngine;
using static TOHE.Translator;
using static TOHE.Options;

namespace TOHE.Roles.Impostor;

public static class LimitedKiller
{
    private static readonly int Id = 13550;
    private static bool IsEnable = false;

    public static OptionItem KillLimit;
    private static OptionItem KillCooldown;
    public static Dictionary<byte, int> AbilityLimit = new();

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.LimitedKiller);
        KillLimit = IntegerOptionItem.Create(Id + 2, "KillLimit", new(1, 15, 1), 8, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.LimitedKiller])
        KillCooldown = FloatOptionItem.Create(Id + 3, "KillCooldown", new(0f, 180f, 2.5f), 30f, TabGroup.ImpostorRoles,  false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.LimitedKiller])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public static void Init()
    {
        AbilityLimit = new();
        IsEnable = false;
    }
    public static void Add(byte playerId)
    {
        IsEnable = true;
        AbilityLimit.Add(playerId, KillLimit.GetInt());
    }

    public static void SetKillCooldown(PlayerControl player) => Main.AllPlayerKillCooldown[player.PlayerId] = CanKill(player.PlayerId) ? KillCooldown.GetFloat() : 300f;
    private static bool CanKill(byte playerId) => AbilityLimit[playerId] > 0;
    public static bool CanUseKillButton(this PlayerControl player) => !player.Data.IsDead && CanKill(player.PlayerId);

    public static string GetKillLimit(byte playerId) => Utils.ColorString(AbilityLimit.ContainsKey(playerId) && CanKill(playerId) ? Utils.GetRoleColor(CustomRoles.Impostor).ShadeColor(0.25f) : Color.gray, AbilityLimit.TryGetValue(playerId, out var kLimit) ? $"({KillLimit.GetInt() - kLimit}/{KillLimit.GetInt()})" : "Invalid");

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

    public static void UpdateLimit(byte killerId) // on check murder
    {
        if (!IsEnable) return;
        AbilityLimit[killerId]--;
        var player = Utils.GetPlayerById(killerId);
        Logger.Info($"{player.GetNameWithRole()} : Number of kills left: {AbilityLimit[killerId]}", "Limited Reaper");
        SendRPC(killerId);
    }
}