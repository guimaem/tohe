using AmongUs.GameOptions;
using System.Collections.Generic;

namespace TOHE.Roles.Neutral;

public static class SidekickB
{
    public static List<byte> playerIdList = new();
    public static bool IsEnable = false;
    public static void Init()
    {
        playerIdList = new();
        IsEnable = false;
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        IsEnable = true;

        if (!AmongUsClient.Instance.AmHost) return;
        if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }
    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = Briber.RecruitedKillCD.GetFloat();
    public static void ApplyGameOptions(IGameOptions opt) => opt.SetVision(true);
    public static void SetHudActive(HudManager __instance, bool isActive)
    {
        __instance.SabotageButton.ToggleVisible(isActive && Briber.RecruitedCanSabotage.GetBool());
    }
}
