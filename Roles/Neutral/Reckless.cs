using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using static TOHE.Options;

namespace TOHE.Roles.Neutral;

// TOHE+
// https://github.com/Gurge44/TOHE_PLUS/blob/main/Roles/Neutral/Reckless.cs
public static class Reckless
{
    private static readonly int Id = 7830;
    private static List<byte> playerIdList = new();

    private static OptionItem DefaultKillCD;
    private static OptionItem ReduceKillCD;
    private static OptionItem MinKillCD;
    
    private static OptionItem HasImpostorVision;
    public static OptionItem CanVent;

    private static Dictionary<byte, float> NowCooldown;

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Reckless);
        DefaultKillCD = FloatOptionItem.Create(Id + 10, "SansDefaultKillCooldown", new(0f, 180f, 2.5f), 35f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Reckless])
            .SetValueFormat(OptionFormat.Seconds);
        ReduceKillCD = FloatOptionItem.Create(Id + 11, "SansReduceKillCooldown", new(0f, 30f, 0.5f), 5f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Reckless])
            .SetValueFormat(OptionFormat.Seconds);
        MinKillCD = FloatOptionItem.Create(Id + 12, "SansMinKillCooldown", new(0f, 30f, 2.5f), 10f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Reckless])
            .SetValueFormat(OptionFormat.Seconds);
        HasImpostorVision = BooleanOptionItem.Create(Id + 13, "ImpostorVision", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Reckless]);
        CanVent = BooleanOptionItem.Create(Id + 14, "CanVent", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Reckless]);
    }

    public static void Init()
    {
        playerIdList = new();
        NowCooldown = new();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        NowCooldown.TryAdd(playerId, DefaultKillCD.GetFloat());

        if (!AmongUsClient.Instance.AmHost) return;
        if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }
    public static bool IsEnable => playerIdList.Count > 0;
    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = NowCooldown[id];
    public static void ApplyGameOptions(IGameOptions opt) => opt.SetVision(HasImpostorVision.GetBool());
    public static void OnCheckMurder(PlayerControl killer)
    {
        NowCooldown[killer.PlayerId] = Math.Clamp(NowCooldown[killer.PlayerId] - ReduceKillCD.GetFloat(), MinKillCD.GetFloat(), DefaultKillCD.GetFloat());
        killer.ResetKillCooldown();
        killer.SyncSettings();
    }
    public static void OnReportDeadBody()
    {
        foreach (byte id in playerIdList.ToArray())
        {
            NowCooldown[Utils.GetPlayerById(id).PlayerId] = DefaultKillCD.GetFloat();
        }
    }
}