using System.Collections.Generic;
using static TOHE.Options;

namespace TOHE.Roles.Neutral;

public static class Workaholic
{
    private static readonly int Id = 15700;

    public static List<byte> WorkaholicAlive = new();

    public static OptionItem WorkaholicVisibleToEveryone;
    public static OptionItem WorkaholicGiveAdviceAlive;
    public static OptionItem WorkaholicCannotWinAtDeath;
    public static OptionItem WorkaholicCanGuess;
    private static OptionItem WorkaholicVentCooldown;
    public static OverrideTasksData WorkaholicTasks;

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Workaholic); // TOH_Y
        WorkaholicCannotWinAtDeath = BooleanOptionItem.Create(Id + 2, "WorkaholicCannotWinAtDeath", false, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Workaholic]);
        WorkaholicVentCooldown = FloatOptionItem.Create(Id + 3, "VentCooldown", new(0f, 180f, 2.5f), 0f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Workaholic])
            .SetValueFormat(OptionFormat.Seconds);
        WorkaholicVisibleToEveryone = BooleanOptionItem.Create(Id + 4, "WorkaholicVisibleToEveryone", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Workaholic]);
        WorkaholicGiveAdviceAlive = BooleanOptionItem.Create(Id + 5, "WorkaholicGiveAdviceAlive", true, TabGroup.NeutralRoles, false)
            .SetParent(WorkaholicVisibleToEveryone);
        WorkaholicCanGuess = BooleanOptionItem.Create(Id + 6, "CanGuess", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Workaholic]);
        WorkaholicTasks = OverrideTasksData.Create(Id + 7, TabGroup.NeutralRoles, CustomRoles.Workaholic);
    }

    public static void ApplyGameOptions()
    {
        AURoleOptions.EngineerCooldown = WorkaholicVentCooldown.GetFloat();
        AURoleOptions.EngineerInVentMaxTime = 0f;
    }

    public static void Init()
    {
        WorkaholicAlive = new();
    }
    public static void Add(byte playerId)
    {
        WorkaholicAlive.Add(playerId);
    }

    public static void OnCompleteTasks(PlayerControl player)
    {
        Logger.Info("工作狂任务做完了", "Workaholic");
        RPC.PlaySoundRPC(player.PlayerId, Sounds.KillSound);
        foreach (var target in Main.AllAlivePlayerControls)
        {
            if (target.PlayerId != player.PlayerId)
            {
                Main.PlayerStates[target.PlayerId].deathReason = target.PlayerId == player.PlayerId ?
                PlayerState.DeathReason.Overtired : PlayerState.DeathReason.Ashamed;
                target.RpcMurderPlayerV3(target);
                Main.PlayerStates[target.PlayerId].SetDead();
                target.SetRealKiller(player);
            }
        }
        if (!CustomWinnerHolder.CheckForConvertedWinner(player.PlayerId))
        {
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Workaholic); //Workaholic
            CustomWinnerHolder.WinnerIds.Add(player.PlayerId);
        }
    }
}