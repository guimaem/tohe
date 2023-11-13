using System.Collections.Generic;
using TOHE.Roles.Neutral;
using UnityEngine;
using TOHE.Roles.Crewmate;
using TOHE.Roles.Impostor;
using static TOHE.Options;
using static TOHE.Translator;

namespace TOHE.Roles.Neutral;

public static class Briber
{
    private static readonly int Id = 3956;

    //private static Dictionary<byte, byte> RecruitLimit = new();
    private static List<byte> playerIdList = new();

    public static OptionItem RecruitCooldown;
    //private static OptionItem NeutralCanBeRecruited;
    //private static OptionItem RecruitLimitOption;
    public static OptionItem RecruitedKillCD;
    public static OptionItem KillCooldown;
    private static OptionItem CanKill;
    private static bool CanKillBool;
    private static OptionItem CanSabotage;

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.OtherRoles, CustomRoles.Briber);
        /*CanKill = BooleanOptionItem.Create(Id + 10, "CanKill", false, TabGroup.OtherRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);
        KillCooldown = FloatOptionItem.Create(Id + 14, "KillCooldown", new(5f, 180f, 2.5f), 30f, TabGroup.OtherRoles, false).SetParent(CanKill)
            .SetValueFormat(OptionFormat.Seconds);*/
        RecruitCooldown = FloatOptionItem.Create(Id + 13, "RecruitCooldown", new(5f, 180f, 2.5f), 20f, TabGroup.OtherRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber])
            .SetValueFormat(OptionFormat.Seconds);
        RecruitedKillCD = FloatOptionItem.Create(Id + 11, "RecruitedKillCooldown", new(5f, 180f, 2.5f), 25f, TabGroup.OtherRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber])
            .SetValueFormat(OptionFormat.Seconds);
        RecruitedCanSabotage = BooleanOptionItem.Create(Id + 12,  "CanUseSabotage", true, TabGroup.OtherRoles, false).SetParent(RecruitedKillCD);
        CanSabotage = BooleanOptionItem.Create(Id + 15, "CanUseSabotage", true, TabGroup.OtherRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);
        //RecruitLimitOption = IntegerOptionItem.Create(Id + 16, "RecruitLimit", new(1, 15, 1), 3).SetParent(CustomRoleSpawnChances[CustomRoles.Briber])
            //.SetValueFormat(OptionFormat.Times);
    }
    public static void Init()
    {
        playerIdList = new();
        CanKillBool = CanKill.GetBool();
        //RecruitLimit = new();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        //RecruitLimit.TryAdd(playerId, RecruitLimitOpt.GetInt());
    }
    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = RecruitCooldown.GetFloat();
    public static void SetKillButtonText() => HudManager.Instance.OverrideText(GetString("GangsterButtonText"));
    public static void ApplyGameOptions(IGameOptions opt) => opt.SetVision(true);
    public static bool OnCheckRecruit(PlayerControl killer, PlayerControl target)
    {
        if (target.Is(CustomRoles.NiceMini) && Mini.Age < 18)
        {
            killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Briber), GetString("CantRecruit")));
            return false;
        }

        if (CanBeRecruited(target))
        {
            target.RpcSetCustomRole(CustomRoles.SidekickB);

                if (!Main.ResetCamPlayerList.Contains(target.PlayerId))
                    Main.ResetCamPlayerList.Add(target.PlayerId);

                killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), GetString("GangsterSuccessfullyRecruited")));
                target.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), GetString("BeRecruitedByJackal")));
                Utils.NotifyRoles();

                if (!DisableShieldAnimations.GetBool()) killer.RpcGuardAndKill(target);
                target.RpcGuardAndKill(killer);
                target.RpcGuardAndKill(target);

                Logger.Info("设置职业:" + target?.Data?.PlayerName + " = " + target.GetCustomRole().ToString() + " + " + CustomRoles.SidekickB.ToString(), "Assign " + CustomRoles.SidekickB.ToString());

                Logger.Info($"{killer.GetNameWithRole()} : 剩余{RecruitLimit[killer.PlayerId]}次招募机会", "Jackal");
                return true;
        }
    }

    public static bool CanBeRecruited(PlayerControl pc)
    {
        return pc != null && (pc.GetCustomRole().IsCrewmate() || pc.GetCustomRole().IsImpostor() || pc.GetCustomRole().IsNeutral())
            && !pc.Is(CustomRoles.Soulless) && !pc.Is(CustomRoles.Lovers) && !pc.Is(CustomRoles.Loyal)
            && !((pc.Is(CustomRoles.NiceMini) || pc.Is(CustomRoles.EvilMini)) && Mini.Age < 18)
            && !(pc.GetCustomSubRoles().Contains(CustomRoles.Hurried) && !Hurried.CanBeConverted.GetBool());
    }
}