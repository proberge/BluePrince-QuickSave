using UnityEngine;
using MelonLoader;
using BluePrince.Modding.QuickSave;

namespace QuickSave
{
    public static class PlayerManager
    {
        public static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        public static Player GetPlayer()
        {
            var playerMsg = new Player();
            GameObject playerGo = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince");
            if (playerGo != null)
            {
                playerMsg.Position = new BluePrince.Modding.QuickSave.Vector3
                {
                    X = playerGo.transform.position.x,
                    Y = playerGo.transform.position.y,
                    Z = playerGo.transform.position.z
                };
                playerMsg.RotationY = playerGo.transform.eulerAngles.y;
            }
            return playerMsg;
        }

        public static void RestorePlayer(Player player)
        {
            if (player == null || player.Position == null) return;

            GameObject playerGo = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince");
            if (playerGo != null)
            {
                playerGo.transform.position = new UnityEngine.Vector3(player.Position.X, player.Position.Y, player.Position.Z);
                playerGo.transform.rotation = Quaternion.Euler(0, player.RotationY, 0);
                Logger.Msg($"  Restored player position to {playerGo.transform.position}");
            }
        }
    }
}
