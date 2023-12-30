using AmongUs.GameOptions;
using Hazel;
using System.Collections.Generic;
using TOHE.Roles.Neutral;
using TOHE.Roles.Double;
using TOHE.Roles.AddOns.Crewmate;
using UnityEngine;
using static TOHE.Options;
using static TOHE.Translator;

namespace TOHE.Roles.Neutral;

public static class Briber
{
    private static readonly int Id = 3950;

    //private static Dictionary<byte, int> RecruitLimit = new();
    public static List<byte> playerIdList = new();

    private static OptionItem RecruitCooldown;
    public static bool IsEnable = false;
    //private static OptionItem NeutralCanBeRecruited;
    //private static OptionItem RecruitLimitOption;
    public static OptionItem RecruitedKillCD;
    //private static OptionItem KillCooldown;
    public static OptionItem HasTasks;
    private static OptionItem HasImpostorVision;
    public static OptionItem RecruitedCanSabotage;
    //private static OptionItem CanKill;
    //private static bool CanKillBool = false;
    public static OptionItem CanSabotage;
    private static OptionItem CanRecruitNeutral;
    private static OptionItem CanRecruitCrewmate;
    private static OptionItem CanRecruitImpostors;
	private static OptionItem CanRecruitMadmate;

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Briber);
        /*CanKill = BooleanOptionItem.Create(Id + 10, "CanKill", false, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);
        KillCooldown = FloatOptionItem.Create(Id + 14, "KillCooldown", new(5f, 180f, 2.5f), 30f, TabGroup.NeutralRoles, false).SetParent(CanKill)
            .SetValueFormat(OptionFormat.Seconds);*/
        RecruitCooldown = FloatOptionItem.Create(Id + 13, "RecruitCooldown", new(0f, 180f, 2.5f), 20f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber])
            .SetValueFormat(OptionFormat.Seconds);
        RecruitedKillCD = FloatOptionItem.Create(Id + 11, "RecruitedKillCooldown", new(0f, 180f, 2.5f), 30f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber])
            .SetValueFormat(OptionFormat.Seconds);
        RecruitedCanSabotage = BooleanOptionItem.Create(Id + 12,  "RecruitedCanSabotage", true, TabGroup.NeutralRoles, false).SetParent(RecruitedKillCD);
        CanSabotage = BooleanOptionItem.Create(Id + 15, "CanUseSabotage", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);
     	HasTasks = BooleanOptionItem.Create(Id + 16, "HasTasks", false, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);  
      //RecruitLimitOption = IntegerOptionItem.Create(Id + 1, "RecruitLimit", new(1, 15, 1), 3).SetParent(CustomRoleSpawnChances[CustomRoles.Briber])
            //.SetValueFormat(OptionFormat.Times);
        HasImpostorVision = BooleanOptionItem.Create(Id + 17, "ImpostorVision", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);
    	CanRecruitNeutral = BooleanOptionItem.Create(Id + 18, "CanRecruitNeutral", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);
	    CanRecruitImpostors = BooleanOptionItem.Create(Id + 19, "CanRecruitImpostors", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);
	    CanRecruitCrewmate = BooleanOptionItem.Create(Id + 20, "CanRecruitCrewmate", false, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);
		CanRecruitMadmate = BooleanOptionItem.Create(Id + 21, "CanRecruitMadmate", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Briber]);
	}
    public static void Init()
    {
        playerIdList = new();
        IsEnable = false;
        //CanKillBool = CanKill.GetBool();
        //RecruitLimit = new();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        IsEnable = true;
        //RecruitLimit.TryAdd(playerId, RecruitLimitOpt.GetInt());
    }
    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = RecruitCooldown.GetFloat();
    public static void SetKillButtonText() => HudManager.Instance.KillButton.OverrideText($"{GetString("BriberButtonText")}");
    public static void ApplyGameOptions(IGameOptions opt) => opt.SetVision(HasImpostorVision.GetBool());
    public static bool OnCheckRecruit(PlayerControl killer, PlayerControl target)
    {
        if (Mini.Age < 18 && (target.Is(CustomRoles.NiceMini) || target.Is(CustomRoles.EvilMini)))
        {
            killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Briber), GetString("CantRecruit")));
            killer.RpcGuardAndKill();
            return false;
        }
        
        if (CanBeRecruited(target))
        {
            //if (!AttendantCantRoles.GetBool() && Mini.Age == 18 || !AttendantCantRoles.GetBool() &&  Mini.Age != 18 && !(target.Is(CustomRoles.NiceMini) || target.Is(CustomRoles.EvilMini))
            target.RpcSetCustomRole(CustomRoles.SidekickB);

            if (!Main.ResetCamPlayerList.Contains(target.PlayerId))
                Main.ResetCamPlayerList.Add(target.PlayerId);

	        killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Briber), GetString("GangsterSuccessfullyRecruited")));
	        target.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Briber), GetString("BeRecruitedByBriber")));
            
            //Utils.NotifyRoles();
            Utils.NotifyRoles(SpecifySeer: killer, SpecifyTarget: target, ForceLoop: true);
            Utils.NotifyRoles(SpecifySeer: target, SpecifyTarget: killer, ForceLoop: true);

            killer.ResetKillCooldown();
            killer.SetKillCooldown();
            if (!DisableShieldAnimations.GetBool()) 
            {
                killer.RpcGuardAndKill(target);
                target.RpcGuardAndKill(killer);
                target.RpcGuardAndKill(target);
            }

            Logger.Info("设置职业:" + target?.Data?.PlayerName + " = " + target.GetCustomRole().ToString() + " + " + CustomRoles.SidekickB.ToString(), "Assign " + CustomRoles.SidekickB.ToString());

            //Logger.Info($"{killer.GetNameWithRole()} : 剩余{RecruitLimit[killer.PlayerId]}次招募机会", "Briber");
            return true;
        }
        
        killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Briber), GetString("GangsterRecruitmentFailure")));
        //Logger.Info($"{killer.GetNameWithRole()} : 剩余{RecruitLimit[killer.PlayerId]}次招募机会", "Briber");
        //if (!DisableShieldAnimations.GetBool()) killer.RpcGuardAndKill();
        return false;
    }

    private static bool CanBeRecruited(PlayerControl pc)
    {
        return pc != null && (pc.GetCustomRole().IsCrewmateWithKillButton() && CanRecruitCrewmate.GetBool() || pc.GetCustomRole().IsImpostor() && CanRecruitImpostors.GetBool() || pc.GetCustomRole().HasImpKillButton(true) && (pc.GetCustomRole().IsMadmate() || pc.GetCustomSubRoles().Contains(CustomRoles.Madmate)) && CanRecruitMadmate.GetBool() || pc.GetCustomRole().IsAbleToBeSidekicked() && pc.GetCustomRole().IsNeutralTeamV2() && CanRecruitNeutral.GetBool())
            && !pc.Is(CustomRoles.Soulless) && !pc.Is(CustomRoles.Lovers) && !pc.Is(CustomRoles.Loyal)
	        && !pc.Is(CustomRoles.Admired)
            && !((pc.Is(CustomRoles.NiceMini) || pc.Is(CustomRoles.EvilMini)) && Mini.Age < 18)
            && !(pc.GetCustomSubRoles().Contains(CustomRoles.Hurried) && !Hurried.CanBeConverted.GetBool())
			&& !pc.Is(CustomRoles.God)
            && !pc.Is(CustomRoles.Briber)
            && !pc.Is(CustomRoles.SidekickB);
    }
}