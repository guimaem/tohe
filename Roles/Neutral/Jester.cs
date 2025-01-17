using AmongUs.GameOptions;

namespace TOHE.Roles.Neutral;

public static class Jester
{
    private static readonly int Id = 14400;

    public static OptionItem CanUseButton;
    public static OptionItem CanVent;
    private static OptionItem ImpostorVision;
    public static OptionItem HideVote;
    public static OptionItem MeetingsNeededForWin;
    public static OptionItem SunnyboyChance;

    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Jester);
        CanUseButton = BooleanOptionItem.Create(Id + 2, "JesterCanUseButton", false, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Jester]);
        CanVent = BooleanOptionItem.Create(Id + 3, "CanVent", true, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Jester]);
        ImpostorVision = BooleanOptionItem.Create(Id + 4, "ImpostorVision", true, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Jester]);
        HideVote = BooleanOptionItem.Create(Id + 5, "HideJesterVote", true, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Jester]);
        MeetingsNeededForWin = IntegerOptionItem.Create(Id + 6, "MeetingsNeededForWin", new(0, 10, 1), 2, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Jester])
            .SetValueFormat(OptionFormat.Times);
        SunnyboyChance = IntegerOptionItem.Create(Id + 7, "SunnyboyChance", new(0, 100, 5), 0, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Jester])
            .SetValueFormat(OptionFormat.Percent);
    }

    public static void ApplyGameOptions(IGameOptions opt) // can vent
    {
        AURoleOptions.EngineerCooldown = 0f;
        AURoleOptions.EngineerInVentMaxTime = 0f;
        opt.SetVision(ImpostorVision.GetBool());
    }

}