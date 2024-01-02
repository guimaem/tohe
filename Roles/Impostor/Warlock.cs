using static TOHE.Translator;
using System.Collections.Generic;
using System.Linq;
using TOHE.Roles.Crewmate;
using TOHE.Modules;
using TOHE.Roles.Neutral;
using UnityEngine;

namespace TOHE.Roles.Impostor;

public static class Warlock
{
    private static readonly int Id = 5100;

    //private static bool IsEnable = false;

    public static Dictionary<byte, PlayerControl> CursedPlayers = new();
    public static Dictionary<byte, bool> isCurseAndKill = new();
    private static Dictionary<byte, float> Timer = new();
    private static bool isCursed;
    private static OptionItem ShapeshiftDur;
    private static OptionItem CanKillAllies;
    private static OptionItem CanKillSelf;

    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Warlock);
        CanKillAllies = BooleanOptionItem.Create(Id + 2, "CanKillAllies", true, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Warlock]);
        CanKillSelf = BooleanOptionItem.Create(Id + 3, "CanKillSelf", true, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Warlock]);
        ShapeshiftDur = FloatOptionItem.Create(Id + 4, "ShapeshiftDuration", new(1f, 180f, 1f), 1f, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Warlock])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public static void ApplyGameOptions()
    {
        AURoleOptions.ShapeshifterCooldown = isCursed ? 1f : Options.DefaultKillCooldown;
        AURoleOptions.ShapeshifterDuration = isCursed ? ShapeshiftDur.GetFloat() : Options.DefaultKillCooldown;
    }

    public static void Init()
    {
        CursedPlayers = new();
        isCurseAndKill = new();
        Timer = new();
        //IsEnable = false;

        isCursed = false;
    }

    public static void Add(byte playerId)
    {
        CursedPlayers.Add(playerId, null);
        isCurseAndKill.Add(playerId, false);

        //IsEnable = true;
    }

    public static bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (!Main.CheckShapeshift[killer.PlayerId] && !isCurseAndKill[killer.PlayerId])
        { //Warlockが変身時以外にキルしたら、呪われる処理
            if (target.Is(CustomRoles.Needy) || target.Is(CustomRoles.Lazy)) return false;
            isCursed = true;
            killer.SetKillCooldown();
            //RPC.PlaySoundRPC(killer.PlayerId, Sounds.KillSound);
            killer.RPCPlayCustomSound("Line");
            CursedPlayers[killer.PlayerId] = target;
            Timer.Add(killer.PlayerId, 0f);
            isCurseAndKill[killer.PlayerId] = true;
            //RPC.RpcSyncCurseAndKill();
            return false;
        }
        if (Main.CheckShapeshift[killer.PlayerId])
        {//呪われてる人がいないくて変身してるときに通常キルになる
            killer.RpcCheckAndMurder(target);
            return false;
        }
        if (isCurseAndKill[killer.PlayerId]) killer.RpcGuardAndKill(target);
        return false;
    }

    public static void OnShapeshift(PlayerControl pc, bool shapeshifting)
    {
        //if (!IsEnable) return;
        if (CursedPlayers[pc.PlayerId] != null)
        {
            if (shapeshifting && !CursedPlayers[pc.PlayerId].Data.IsDead)
            {
                var cp = CursedPlayers[pc.PlayerId];
                Vector2 cppos = cp.transform.position;
                Dictionary<PlayerControl, float> cpdistance = new();
                float dis;
                foreach (PlayerControl p in Main.AllAlivePlayerControls)
                {
                    if (p.PlayerId == cp.PlayerId) continue;
                    if (!CanKillSelf.GetBool() && p.PlayerId == pc.PlayerId) continue;
                    if (!CanKillAllies.GetBool() && p.GetCustomRole().IsImpostor()) continue;
                    if (p.Is(CustomRoles.Glitch)) continue;
                    if (p.Is(CustomRoles.Pestilence)) continue;
                    if (Pelican.IsEaten(p.PlayerId) || Medic.ProtectList.Contains(p.PlayerId)) continue;
                    dis = Vector2.Distance(cppos, p.transform.position);
                    cpdistance.Add(p, dis);
                    Logger.Info($"{p?.Data?.PlayerName}の位置{dis}", "Warlock");
                }
                if (cpdistance.Count >= 1)
                {
                    var min = cpdistance.OrderBy(c => c.Value).FirstOrDefault();//一番小さい値を取り出す
                    PlayerControl targetw = min.Key;
                    if (cp.RpcCheckAndMurder(targetw, true))
                    {
                        targetw.SetRealKiller(pc);
                        Logger.Info($"{targetw.GetNameWithRole()}was killed", "Warlock");
                        cp.RpcMurderPlayerV3(targetw);//殺す
                        pc.RpcGuardAndKill(pc);
                        pc.Notify(GetString("WarlockControlKill"));
                    }
                }
                else
                {
                    pc.Notify(GetString("WarlockNoTarget"));
                }
                isCurseAndKill[pc.PlayerId] = false;
            }
            CursedPlayers[pc.PlayerId] = null;
        }
    }

    public static void OnFixedUpdate(PlayerControl player)
    {
        if (Timer.TryGetValue(player.PlayerId, out var warlockTimer))
        {
            var playerId = player.PlayerId;
            if (player.IsAlive())
            {
                if (warlockTimer >= 1f)
                {
                    player.RpcResetAbilityCooldown();
                    isCursed = false;
                    player.SyncSettings();
                    Timer.Remove(playerId);
                }
                else
                {
                    warlockTimer += Time.fixedDeltaTime;
                    Timer[playerId] = warlockTimer;
                }
            }
            else
            {
                Timer.Remove(playerId);
            }
        }
    }

    public static void GetButtonText(HudManager __instance, PlayerControl pc, bool shapeshifting)
    {
        __instance.ReportButton.OverrideText(GetString("ReportButtonText"));
        bool curse = isCurseAndKill.TryGetValue(pc.PlayerId, out bool wcs) && wcs;
        if (!shapeshifting && !curse)
            __instance.KillButton.OverrideText(GetString("WarlockCurseButtonText"));
        else
            __instance.KillButton.OverrideText(GetString("KillButtonText"));
        if (!shapeshifting && curse)
            __instance.AbilityButton.OverrideText(GetString("WarlockShapeshiftButtonText"));
    }
}