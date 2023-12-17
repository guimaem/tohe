using AmongUs.GameOptions;
using System.Collections.Generic;
using TOHE.Modules;
using static TOHE.Translator;
using static TOHE.Options;

namespace TOHE.Roles.Impostor;

public static class Bomber
{
    private static readonly int Id = 700;

    private static OptionItem BomberRadius;
    public static OptionItem CanKill;
    private static OptionItem BombCooldown;
    private static OptionItem KillCooldown;
    public static OptionItem ImpostorsSurviveBombs;
    public static OptionItem BomberDiesInExplosion;
    public static OptionItem NukerChance;
    public static OptionItem NukeCooldown;
    public static OptionItem NukeRadius;

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Bomber);
        BomberRadius = FloatOptionItem.Create(Id + 10, "BomberRadius", new(0.5f, 5f, 0.5f), 2f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bomber])
            .SetValueFormat(OptionFormat.Multiplier);
        CanKill = BooleanOptionItem.Create(Id + 11, "CanKill", false, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bomber]);
        KillCooldown = FloatOptionItem.Create(Id + 12, "KillCooldown", new(0f, 180f, 2.5f), 30f, TabGroup.ImpostorRoles, false)
            .SetParent(BomberCanKill)
            .SetValueFormat(OptionFormat.Seconds);
        BombCooldown = FloatOptionItem.Create(Id + 13, "BombCooldown", new(5f, 180f, 2.5f), 60f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bomber])
            .SetValueFormat(OptionFormat.Seconds);
        ImpostorsSurviveBombs = BooleanOptionItem.Create(Id + 14, "ImpostorsSurviveBombs", true, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bomber]);
        BomberDiesInExplosion = BooleanOptionItem.Create(Id + 15, "BomberDiesInExplosion", true, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bomber]);
        NukerChance = IntegerOptionItem.Create(Id + 16, "NukerChance", new(0, 100, 5), 0, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bomber])
            .SetValueFormat(OptionFormat.Percent);
        NukeCooldown = FloatOptionItem.Create(Id + 17, "NukeCooldown", new(5f, 180f, 2.5f), 60f, TabGroup.ImpostorRoles, false)
            .SetParent(NukerChance)
            .SetValueFormat(OptionFormat.Seconds);
        NukeRadius = FloatOptionItem.Create(Id + 18, "NukeRadius", new(1f, 100f, 1f), 25f, TabGroup.ImpostorRoles, false)
            .SetParent(NukerChance)
            .SetValueFormat(OptionFormat.Multiplier);
    }

    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = CanKill.GetBool() ? KillCooldown.GetFloat() : 300f;
    public static bool CanUseKillButton(this PlayerControl pc) => CanKill.GetBool() && pc.IsAlive();

    public static void ApplyGameOptions()
    {
        AURoleOptions.ShapeshifterCooldown = BombCooldown.GetFloat();
        AURoleOptions.ShapeshifterDuration = 2f;
    }

    public static void OnShapeshift(PlayerControl pc, bool shapeshifting)
    {
        if (shapeshifting)
        {
            Logger.Info("炸弹爆炸了", "Boom");
            CustomSoundsManager.RPCPlayCustomSoundAll("Boom");
            foreach (var tg in Main.AllPlayerControls)
            {
                if (!tg.IsModClient()) tg.KillFlash();
                var pos = pc.transform.position;
                var dis = Vector2.Distance(pos, tg.transform.position);

                if (!tg.IsAlive() || Pelican.IsEaten(tg.PlayerId) || Medic.ProtectList.Contains(tg.PlayerId) || (tg.Is(CustomRoleTypes.Impostor) && ImpostorsSurviveBombs.GetBool()) || tg.inVent || tg.Is(CustomRoles.Pestilence)) continue;
                if (dis > BomberRadius.GetFloat()) continue;
                if (tg.PlayerId == pc.PlayerId) continue;

                Main.PlayerStates[tg.PlayerId].deathReason = PlayerState.DeathReason.Bombed;
                tg.SetRealKiller(pc);
                tg.RpcMurderPlayerV3(tg);
                Medic.IsDead(tg);
            }
            _ = new LateTask(() =>
            {
                var totalAlive = Main.AllAlivePlayerControls.Length;
                //自分が最後の生き残りの場合は勝利のために死なない
                if (BomberDiesInExplosion.GetBool())
                {
                    if (totalAlive > 0 && !GameStates.IsEnded)
                    {
                        Main.PlayerStates[pc.PlayerId].deathReason = PlayerState.DeathReason.Bombed;
                        pc.RpcMurderPlayerV3(pc);
                    }
                }
                Utils.NotifyRoles();
            }, 1.5f, "Bomber Suiscide");
        }
    }
}