using System.Collections.Generic;
using System.Linq;
using static TOHE.Options;

namespace TOHE.Roles.Impostor;

public static class Mare
{
    private static readonly int Id = 23000;
    public static List<byte> playerIdList = new();

    public static OptionItem KillCooldownInLightsOut;
    //private static OptionItem KillCooldownNormally;
    private static OptionItem SpeedInLightsOut;
    private static bool idAccelerated = false;


    public static void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Mare, canSetNum: true, tab: TabGroup.Addons);
        SpeedInLightsOut = FloatOptionItem.Create(Id + 10, "MareAddSpeedInLightsOut", new(0.1f, 0.5f, 0.1f), 0.3f, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Mare])
            .SetValueFormat(OptionFormat.Multiplier);
        KillCooldownInLightsOut = FloatOptionItem.Create(Id + 11, "MareKillCooldownInLightsOut", new(0f, 180f, 2.5f), 7.5f, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Mare]);
    }
    public static void Init()
    {
        playerIdList = new();
    }
    public static void Add(byte mare)
    {
        playerIdList.Add(mare);
    }
    public static bool IsEnable => playerIdList.Count > 0;
    public static float GetKillCooldown => Utils.IsActive(SystemTypes.Electrical) ? KillCooldownInLightsOut.GetFloat() : DefaultKillCooldown;
    public static void ApplyGameOptions(byte playerId)
    {
        if (Utils.IsActive(SystemTypes.Electrical) && !idAccelerated)
        {
            idAccelerated = true;
            Main.AllPlayerSpeed[playerId] += SpeedInLightsOut.GetFloat();
        }
        else if (!Utils.IsActive(SystemTypes.Electrical) && idAccelerated)
        {
            idAccelerated = false;
            Main.AllPlayerSpeed[playerId] -= SpeedInLightsOut.GetFloat();
        }
    }

    public static bool KnowTargetRoleColor(PlayerControl target, bool isMeeting)
        => !isMeeting && playerIdList.Contains(target.PlayerId) && Utils.IsActive(SystemTypes.Electrical);
}