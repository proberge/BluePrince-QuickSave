using System;
using UnityEngine;
using MelonLoader;
using BluePrince.Modding.QuickSave;
using Il2Cpp;
using Vector3 = UnityEngine.Vector3;

namespace QuickSave
{
    public static class PickupItemManager
    {
        public static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        public static void SavePickupItems(SaveState saveState)
        {
            GameObject itemsContainer = GameObject.Find("__SYSTEM/Pickup Spawn Pools");
            if (itemsContainer == null)
            {
                Logger.Warning("Could not find Pickup Spawn Pools for saving.");
                return;
            }

            for (int i = 0; i < itemsContainer.transform.childCount; i++)
            {
                Transform childTransform = itemsContainer.transform.GetChild(i);
                if (!childTransform.gameObject.activeInHierarchy) continue;

                string name = childTransform.name;
                int cloneIndex = name.IndexOf("(Clone)");
                if (cloneIndex >= 0) name = name.Substring(0, cloneIndex);

                var itemState = new PickupItemState
                {
                    Name = name,
                    Position = new BluePrince.Modding.QuickSave.Vector3
                    {
                        X = childTransform.position.x,
                        Y = childTransform.position.y,
                        Z = childTransform.position.z
                    },
                    Rotation = new BluePrince.Modding.QuickSave.Vector3
                    {
                        X = childTransform.eulerAngles.x,
                        Y = childTransform.eulerAngles.y,
                        Z = childTransform.eulerAngles.z
                    }
                };
                saveState.PickupItems.Add(itemState);
            }
            Logger.Msg($"Saved {saveState.PickupItems.Count} pickup items.");
        }

        public static void RestorePickupItems(SaveState saveState)
        {
            GameObject itemsContainer = GameObject.Find("__SYSTEM/Pickup Spawn Pools");
            if (itemsContainer == null)
            {
                Logger.Warning("Could not find Pickup Spawn Pools for loading.");
                return;
            }

            deactivateAllPickupItems();

            var pool = itemsContainer.GetComponent<Il2CppPathologicalGames.SpawnPool>();
            if (pool == null)
            {
                var components = itemsContainer.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp != null && comp.GetIl2CppType().Name == "SpawnPool")
                    {
                        pool = comp.TryCast<Il2CppPathologicalGames.SpawnPool>();
                        break;
                    }
                }
            }

            if (pool == null)
            {
                Logger.Error("Could not find SpawnPool component on Pickup Spawn Pools.");
                return;
            }

            foreach (var item in saveState.PickupItems)
            {
                try
                {
                    Vector3 pos = new Vector3(item.Position.X, item.Position.Y, item.Position.Z);
                    Quaternion rot = Quaternion.Euler(item.Rotation.X, item.Rotation.Y, item.Rotation.Z);
                    pool.Spawn(item.Name, pos, rot);
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to spawn item {item.Name}: {ex.Message}");
                }
            }
            Logger.Msg($"Restored {saveState.PickupItems.Count} pickup items.");
        }

        private static void deactivateAllPickupItems()
        {
            GameObject itemsContainer = GameObject.Find("__SYSTEM/Pickup Spawn Pools");
            if (itemsContainer == null) return;

            for (int i = 0; i < itemsContainer.transform.childCount; i++)
            {
                var child = itemsContainer.transform.GetChild(i).gameObject;
                if (child.activeInHierarchy)
                {
                    Logger.Msg($"Deactivated pickup item: {child.name}");
                    child.SetActive(false);
                }
            }
            Logger.Msg("Deactivated all active pickup items.");
        }
    }
}
