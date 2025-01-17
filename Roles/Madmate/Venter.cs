using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static TOHE.Options;
using static TOHE.Translator;

namespace TOHE.Roles.Madmate;

public static class Venter
{
    private static readonly int Id = 17130;
    //private static List<byte> playerIdList = new();
    private static bool IsEnable = false;
    public static Dictionary<byte, int> KillLimit = new();

    private static OptionItem VentCooldown;
    private static OptionItem CanKillImpostors;
    public static OptionItem HasSkillLimit;    
    public static OptionItem SkillLimit;
    private static OptionItem HasImpostorVision;
   
    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Venter);
        VentCooldown = FloatOptionItem.Create(Id + 10, "VentCooldown", new(0f, 180f, 2.5f), 30f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Venter])
            .SetValueFormat(OptionFormat.Seconds);
        CanKillImpostors = BooleanOptionItem.Create(Id + 11, "CanKillAllies", false, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Venter]);
        HasSkillLimit = BooleanOptionItem.Create(Id + 12, "HasSkillLimit", true, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Venter]);
        SkillLimit = IntegerOptionItem.Create(Id + 13, "SkillLimit", new(1, 20, 1), 10, TabGroup.ImpostorRoles, false).SetParent(HasSkillLimit);
        HasImpostorVision = BooleanOptionItem.Create(Id + 14, "ImpostorVision", false, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Venter]);
    }
    public static void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        if (HasSkillLimit.GetBool())
        {
            AURoleOptions.EngineerCooldown = CanUseSkill(playerId) ? VentCooldown.GetFloat() : 0f;
            AURoleOptions.EngineerInVentMaxTime = CanUseSkill(playerId) ? 1 : 0f;
        }
        else
        {
            AURoleOptions.EngineerCooldown = VentCooldown.GetFloat();
            AURoleOptions.EngineerInVentMaxTime = 1;
        }
        opt.SetVision(HasImpostorVision.GetBool());
    }

    public static void Init()
    {
        //playerIdList = new();
        IsEnable = false;
        if (HasSkillLimit.GetBool())
	        KillLimit = new();
    }
    public static void Add(byte playerId)
    {
        //playerIdList.Add(playerId);
        IsEnable = true;
        if (HasSkillLimit.GetBool())
            KillLimit.Add(playerId, SkillLimit.GetInt());
    }
    private static void SendRPC(byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetVenterKillLimit, SendOption.Reliable, -1);
        writer.Write(playerId);
        writer.Write(KillLimit[playerId]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC(MessageReader reader)
    {
        byte VenterId = reader.ReadByte();
        int Limit = reader.ReadInt32();
        if (KillLimit.ContainsKey(VenterId))
            KillLimit[VenterId] = Limit;
        else
            KillLimit.Add(VenterId, SkillLimit.GetInt());
    }
    private static bool CanUseSkill(byte id) => KillLimit[id] > 0;

    public static string GetSkillLimit(byte playerId)
    {
        return Utils.ColorString(KillLimit.ContainsKey(playerId) && CanUseSkill(playerId) ? Utils.GetRoleColor(CustomRoles.Madmate).ShadeColor(0.25f) : Color.gray,
        KillLimit.TryGetValue(playerId, out var killLimit) ? $"({SkillLimit.GetInt() - killLimit}/{SkillLimit.GetInt()})" : "Invalid");
    }

    private static bool CanBeKilled(this PlayerControl pc)
    {
        return pc.IsAlive() && !pc.Is(CustomRoleTypes.Madmate) && !pc.GetCustomSubRoles().Contains(CustomRoles.Madmate) && !(pc.GetCustomRole().IsImpostorTeam() && !CanKillImpostors.GetBool());
    } 

    public static void OnEnterVent(PlayerControl pc)
    {
        if (!IsEnable) return;
        if (!pc.Is(CustomRoles.Venter)) return;
        if (HasSkillLimit.GetBool())
        {
            if (!CanUseSkill(pc.PlayerId)) return;
            KillLimit[pc.PlayerId]--;
            Logger.Info($"{pc.GetNameWithRole()} : Number of kills left: {KillLimit[pc.PlayerId]}", "Venter");
            SendRPC(pc.PlayerId);
        }
        
        List<PlayerControl> list = Main.AllAlivePlayerControls.Where(x => x.PlayerId != pc.PlayerId && x.CanBeKilled()).ToList();
        if (list.Count < 1)
        {
            Logger.Info($"No target to kill", "Venter");
        }
        else
        {
            list = list.OrderBy(x => Vector2.Distance(pc.transform.position, x.transform.position)).ToList();
            var target = list[0];
            if (!target.Is(CustomRoles.Pestilence))
            {
                
                target.SetRealKiller(pc);
                target.RpcCheckAndMurder(target);
                if (!DisableShieldAnimations.GetBool()) pc.RpcGuardAndKill();
                //pc.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), GetString("VampireTargetDead")));
                Logger.Info($"Venter vented to kill：{pc.GetNameWithRole()} => {target.GetNameWithRole()}", "Venter");
            }
            else
            {
                pc.SetRealKiller(target);
                target.RpcMurderPlayerV3(pc);
                if (!DisableShieldAnimations.GetBool()) pc.RpcGuardAndKill();
                Logger.Info($"Venter tried to kill pestilence (reflected back)：{target.GetNameWithRole()} => {pc.GetNameWithRole()}", "Pestilence Reflect");
            }
        }
    }
}