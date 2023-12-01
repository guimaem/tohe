using AmongUs.GameOptions;
using TOHE.Modules;
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
    private static List<byte> playerIdList = new();
    private static bool IsEnable = false;

    private static OptionItem VentCooldown;
    private static OptionItem CanKillImpostors;

    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Venter);
        VentCooldown = FloatOptionItem.Create(Id + 10, "VentCooldown", new(0f, 180f, 2.5f), 25f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Venter])
            .SetValueFormat(OptionFormat.Seconds);
        CanKillImpostors = BooleanOptionItem.Create(Id + 11, "CanKillAllies", false, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Venter]);
    }
    public static void SetGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown = VentCooldown.GetFloat();
        AURoleOptions.EngineerInVentMaxTime = 1;
    }

    public static void Init()
    {
        playerIdList = new();
        IsEnable = false;
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        IsEnable = true;
    }

    public static void OnEnterVent(PlayerControl pc)
    {
        if (!IsEnable) return;
        if (!pc.Is(CustomRoles.Venter)) return;
        List<PlayerControl> list = Main.AllAlivePlayerControls.Where(x => x.PlayerId != pc.PlayerId && (CanKillImpostors.GetBool() || !x.GetCustomSubRoles().Contains(CustomRoles.Madmate) && !x.GetCustomRole().IsMadmate() || !x.GetCustomRole().IsImpostorTeam())).ToList();
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
                pc.RpcGuardAndKill();
                pc.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), GetString("VampireTargetDead")));
                Logger.Info($"Venter vented to kill：{pc.GetNameWithRole()} => {target.GetNameWithRole()}", "Venter");
            }
            else
            {
                pc.SetRealKiller(target);
                target.RpcMurderPlayerV3(pc);
                pc.RpcGuardAndKill();
                Logger.Info($"Venter tried to kill pestilence (reflected back)：{target.GetNameWithRole()} => {pc.GetNameWithRole()}", "Pestilence Reflect");
            }
        }
    }
}