using HarmonyLib;
using UnityEngine;
using LevelImposter.Shop;
using Hazel;

namespace LevelImposter.Core
{
    /*
     *      Normally, ladders are handled by
     *      AirshipStatus. This bypasses that
     *      requirement by supplying it's own
     *      ladder listings.
     */
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
    public static class LadderPatch
    {
        public static bool Prefix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader, PlayerPhysics __instance)
        {
            if (MapLoader.CurrentMap == null)
                return true;

            if (callId == 31)
            {
                byte ladderId = reader.ReadByte();
                byte climbLadderSid = reader.ReadByte();
                bool isFound = LadderBuilder.TryGetLadder(ladderId, out Ladder? ladder);

                if (isFound)
                {
                    __instance.ClimbLadder(ladder, climbLadderSid);
                    return false;
                }
                LILogger.Warn($"[RPC] Could not find a ladder of id: {ladderId}");
            }
            return true;
        }
    }
}
