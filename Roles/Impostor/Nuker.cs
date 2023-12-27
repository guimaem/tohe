using System.Collections.Generic;
using TOHE.Roles.Impostor;
using TOHE.Roles.Neutral;
using TOHE.Roles.Crewmate;
using TOHE.Modules;
using UnityEngine;
using static TOHE.Translator;

namespace TOHE.Roles.Impostor;

public static class Nuker
{
    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = Bomber.CanKill.GetBool() ? Bomber.NukeCooldown.GetFloat() : 300f;
    public static bool CanUseKillButton(this PlayerControl pc) => Bomber.CanKill.GetBool() && pc.IsAlive();

    public static void ApplyGameOptions()
    {
        AURoleOptions.ShapeshifterCooldown = Bomber.NukeCooldown.GetFloat();
        AURoleOptions.ShapeshifterDuration = 2f;
    }

    public static void OnShapeshift(PlayerControl pc, bool shapeshifting)
    {
        if (shapeshifting)
        {
            Logger.Info("The bomb went off", "Nuker");
            CustomSoundsManager.RPCPlayCustomSoundAll("Boom");
            foreach (var tg in Main.AllPlayerControls)
            {
                if (!tg.IsModClient()) tg.KillFlash();
                var pos = pc.transform.position;
                var dis = Vector2.Distance(pos, tg.transform.position);

                if (!tg.IsAlive() || Pelican.IsEaten(tg.PlayerId) || Medic.ProtectList.Contains(tg.PlayerId) || tg.inVent || tg.Is(CustomRoles.Pestilence)) continue;
                if (dis > Bomber.NukeRadius.GetFloat()) continue;
                if (tg.PlayerId == pc.PlayerId) continue;

                Main.PlayerStates[tg.PlayerId].deathReason = PlayerState.DeathReason.Bombed;
                tg.SetRealKiller(pc);
                tg.RpcMurderPlayerV3(tg);
                Medic.IsDead(tg);
            }
            _ = new LateTask(() =>
            {
                var totalAlive = Main.AllAlivePlayerControls.Length;
                
                if (totalAlive > 0 && !GameStates.IsEnded)
                {
                    if (totalAlive > 0 && !GameStates.IsEnded)
                    {
                        Main.PlayerStates[pc.PlayerId].deathReason = PlayerState.DeathReason.Bombed;
                        pc.RpcMurderPlayerV3(pc);
                    }
                }
                Utils.NotifyRoles();
            }, 1.5f, "Nuker");
        }
    }
}