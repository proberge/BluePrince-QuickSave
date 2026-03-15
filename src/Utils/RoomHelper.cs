using UnityEngine;
using MelonLoader;
using Il2Cpp;
using BluePrince.Modding.QuickSave;
using Vector3 = UnityEngine.Vector3;
using Column = BluePrince.Modding.QuickSave.Column;
using Rank = BluePrince.Modding.QuickSave.Rank;

namespace QuickSave.Utils
{
    // Internal cardinal direction constants: 0=North, 1=East, 2=South, 3=West
    public static class RoomHelper
    {
        public static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        /// <summary>
        /// Returns the world-space cardinal direction (0=N, 1=E, 2=S, 3=W) a door faces
        /// after applying the room's Y rotation, or null if the door name is unrecognised.
        /// </summary>
        public static int? GetDoorWorldDir(string doorName, float rotationY)
        {
            int baseDir = -1;
            if (doorName.Contains("N Door", System.StringComparison.OrdinalIgnoreCase)) baseDir = 0;
            else if (doorName.Contains("E Door", System.StringComparison.OrdinalIgnoreCase)) baseDir = 1;
            else if (doorName.Contains("S Door", System.StringComparison.OrdinalIgnoreCase)) baseDir = 2;
            else if (doorName.Contains("W Door", System.StringComparison.OrdinalIgnoreCase)) baseDir = 3;
            if (baseDir < 0) return null;

            int rotationSteps = Mathf.RoundToInt(rotationY / 90f);
            int finalDir = ((baseDir + rotationSteps) % 4 + 4) % 4;
            return finalDir;
        }

        /// <summary>Returns the (dx, dz) grid delta for a cardinal direction (0=N,1=E,2=S,3=W).</summary>
        public static (int dx, int dz) DirectionToDelta(int dir)
        {
            return dir switch
            {
                0 => (0, 1),   // North
                1 => (1, 0),   // East
                2 => (0, -1),  // South
                3 => (-1, 0),  // West
                _ => (0, 0)
            };
        }

        public static void PlaceDeadEnd(Vector3 worldPos, int direction)
        {
            GameObject spawnPoolGo = GameObject.Find("__SYSTEM/OtherPrefab Spawn Pools");
            if (spawnPoolGo == null)
            {
                Logger.Warning("  Could not find OtherPrefab Spawn Pools for DeadEnd placement.");
                return;
            }

            var components = spawnPoolGo.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp != null && comp.GetIl2CppType().Name == "SpawnPool")
                {
                    float rotY = direction * 90f + 180f;
                    var rot = Quaternion.Euler(0, rotY, 0);

                    try
                    {
                        var pool = comp.TryCast<Il2CppPathologicalGames.SpawnPool>();
                        if (pool != null)
                        {
                            pool.Spawn("DeadEnd1", worldPos, rot);
                            Logger.Msg($"  Spawned DeadEnd at {worldPos} rot={rotY}");
                            return;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Warning($"  DeadEnd spawn failed: {ex.Message}");
                    }
                }
            }
            Logger.Warning($"  DeadEnd spawn failed: SpawnPool component not found");
        }

        public static void TriggerRoomBegin(GameObject roomGo)
        {
            var fsm = roomGo.GetComponent<PlayMakerFSM>();
            if (fsm != null)
            {
                fsm.SendEvent("Begin");
                Logger.Msg($"  Triggered 'Begin' event for room: {roomGo.name}");
            }
        }

        public static bool GetOccupancy(bool[,] occupancy, int x, int z)
        {
            if (x < 0 || x >= 5 || z < 0 || z >= 9) return false;
            return occupancy[x, z];
        }

        public static bool GetOccupancy(bool[,] occupancy, Column col, Rank rank)
        {
            return GetOccupancy(occupancy, col.ToX(), rank.ToZ());
        }
    }
}
