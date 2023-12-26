using AmongUs.GameOptions;
using System.Collections.Generic;
using TOHE.Modules;
using static TOHE.Options;

namespace TOHE.Roles.Neutral;

public static class JesterKiller
{
    private static readonly int Id = 14430;

    public static OptionItem CanVent;
    public static OptionItem HideVote;
    private static OptionItem ImpVision;
    public static OptionItem CanUseButton;
    public static OptionItem MeetingsNeededForWin;
    public static OptionItem CanSabotage;
    private static OptionItem KillCooldown;

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.JesterKiller);
        CanUseButton = BooleanOptionItem.Create(Id + 2, "JesterCanUseButton", false, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.JesterKiller]);
        CanVent = BooleanOptionItem.Create(Id + 3, "CanVent", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.JesterKiller]);
        ImpVision = BooleanOptionItem.Create(Id + 4, "ImpostorVision", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.JesterKiller]);
        HideVote = BooleanOptionItem.Create(Id + 5, "HideJesterVote", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.JesterKiller]);
        MeetingsNeededForWin = IntegerOptionItem.Create(Id + 6, "MeetingsNeededForWin", new(0, 10, 1), 2, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.JesterKiller])
            .SetValueFormat(OptionFormat.Times);
        KillCooldown = FloatOptionItem.Create(Id + 7, "KillCooldown", new(0f, 180f, 2.5f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.JesterKiller])
            .SetValueFormat(OptionFormat.Seconds);
        CanSabotage = BooleanOptionItem.Create(Id + 8, "CanUseSabotage", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.JesterKiller]);
    }

    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public static void ApplyGameOptions(IGameOptions opt) => opt.SetVision(ImpVision.GetBool());
}