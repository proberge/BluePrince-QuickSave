using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using MelonLoader;
using BluePrince.Modding.QuickSave;
using Il2Cpp;
using QuickSave.Utils;

namespace QuickSave
{
    public static class InventoryManager
    {
        public static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        public static Inventory GetInventory()
        {
            var inventory = new Inventory();

            inventory.Steps = FsmHelper.GetFsmInt("/__SYSTEM/HUD/Steps", "STEPS") ?? 0;
            inventory.Keys = FsmHelper.GetFsmInt("/__SYSTEM/HUD/Keys", "KEYS") ?? 0;
            inventory.Gems = FsmHelper.GetFsmInt("/__SYSTEM/HUD/Gems", "GEMS") ?? 0;
            inventory.Gold = FsmHelper.GetFsmInt("/__SYSTEM/HUD/Gold", "GOLD") ?? 0;
            inventory.Dice = FsmHelper.GetFsmInt("/__SYSTEM/HUD/Bones", "BONES") ?? 0;
            inventory.Stars = FsmHelper.GetFsmInt("/__SYSTEM/HUD/Stars", "TotalStars") ?? 0;

            void GetItemsFromProxy(string categoryName, Google.Protobuf.Collections.RepeatedField<string> targetList)
            {
                string goPath = $"/__SYSTEM/Inventory/Inventory ({categoryName})";
                GameObject go = GameObject.Find(goPath);
                if (go == null) return;
                var components = go.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp != null && comp.GetIl2CppType().Name == "PlayMakerArrayListProxy")
                    {
                        var property = comp.GetIl2CppType().GetProperty("arrayList");
                        if (property != null)
                        {
                            var listValue = property.GetValue(comp);
                            if (listValue != null)
                            {
                                var list = listValue.Cast<Il2CppSystem.Collections.ArrayList>();
                                if (list != null)
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        var entry = list[i];
                                        if (entry == null) continue;
                                        
                                        string itemName = (entry is GameObject entryGo) ? entryGo.name : entry.ToString();
                                        int suffixIndex = itemName.IndexOf(" (");
                                        if (suffixIndex >= 0) itemName = itemName.Substring(0, suffixIndex).Trim();
                                        targetList.Add(itemName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            GetItemsFromProxy("PickedUp", inventory.PickedUpItems);
            GetItemsFromProxy("PreSpawn", inventory.PreSpawnItems);
            GetItemsFromProxy("EstateItems", inventory.EstateItems);
            GetItemsFromProxy("CoatCheck", inventory.CoatCheckItems);
            GetItemsFromProxy("UsedItems", inventory.UsedItems);
            GetItemsFromProxy("Combined", inventory.CombinedItems);
            GetItemsFromProxy("TempArray", inventory.TempArrayItems);

            return inventory;
        }

        public static void RestoreInventory(Inventory inventory)
        {
            if (inventory == null)
            {
                Logger.Warning("No inventory data in save state.");
                return;
            }

            // Probably need to set steps to 1 more since moving the player position into a room seems to automatically
            // lower it.
            FsmHelper.SetFsmInt("/__SYSTEM/HUD/Steps", "STEPS", inventory.Steps + 1, new[] { "Update", "Update Step" });
            FsmHelper.SetFsmInt("/__SYSTEM/HUD/Keys", "KEYS", inventory.Keys);
            FsmHelper.SetFsmInt("/__SYSTEM/HUD/Gems", "GEMS", inventory.Gems);
            // not working:
            // FsmHelper.SetFsmInt("/__SYSTEM/HUD/Gems", "Gem Adjustment Amount", 0);
            FsmHelper.SetFsmInt("/__SYSTEM/HUD/Gold", "GOLD", inventory.Gold);
            FsmHelper.SetFsmInt("/__SYSTEM/HUD/Bones", "BONES", inventory.Dice);
            FsmHelper.SetFsmInt("/__SYSTEM/HUD/Stars", "TotalStars", inventory.Stars);

            string[] categories = { "PreSpawn", "EstateItems", "PickedUp", "CoatCheck", "UsedItems", "Combined", "TempArray" };
            var itemPool = new Dictionary<string, GameObject>();
            var proxies = new Dictionary<string, Il2CppSystem.Collections.ArrayList>();

            foreach (var cat in categories)
            {
                GameObject go = GameObject.Find($"/__SYSTEM/Inventory/Inventory ({cat})");
                if (go == null) continue;
                var components = go.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp != null && comp.GetIl2CppType().Name == "PlayMakerArrayListProxy")
                    {
                        var property = comp.GetIl2CppType().GetProperty("arrayList");
                        if (property != null)
                        {
                            var listValue = property.GetValue(comp);
                            if (listValue != null)
                            {
                                var list = listValue.Cast<Il2CppSystem.Collections.ArrayList>();
                                if (list != null)
                                {
                                    proxies[cat] = list;
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        var entry = list[i];
                                        if (entry is GameObject itemGo)
                                        {
                                            if (!itemPool.ContainsKey(itemGo.name))
                                                itemPool[itemGo.name] = itemGo;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var kvp in proxies)
            {
                if (kvp.Key == "PickedUp") continue;
                kvp.Value.Clear();
            }

            void PopulateCategory(string cat, Google.Protobuf.Collections.RepeatedField<string> names)
            {
                if (!proxies.ContainsKey(cat)) return;
                var list = proxies[cat];
                foreach (var name in names)
                {
                    if (itemPool.TryGetValue(name, out var itemGo)) list.Add(itemGo);
                }
            }

            PopulateCategory("PreSpawn", inventory.PreSpawnItems);
            PopulateCategory("EstateItems", inventory.EstateItems);
            PopulateCategory("CoatCheck", inventory.CoatCheckItems);
            PopulateCategory("UsedItems", inventory.UsedItems);
            PopulateCategory("Combined", inventory.CombinedItems);
            PopulateCategory("TempArray", inventory.TempArrayItems);

            GameObject globalManager = GameObject.Find("/Global Manager");
            if (globalManager != null)
            {
                var fsm = globalManager.GetComponent<PlayMakerFSM>();
                if (fsm != null)
                {
                    foreach (var rawName in inventory.PickedUpItems)
                    {
                        string itemName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(rawName.ToLower());
                        
                        bool alreadyOwned = false;
                        if (proxies.TryGetValue("PickedUp", out var pickedUpList))
                        {
                            for (int i = 0; i < pickedUpList.Count; i++)
                            {
                                var entry = pickedUpList[i];
                                if (entry == null) continue;
                                string entryName = (entry is GameObject entryGo) ? entryGo.name : entry.ToString();
                                int suffixIndex = entryName.IndexOf(" (");
                                if (suffixIndex >= 0) entryName = entryName.Substring(0, suffixIndex).Trim();

                                if (string.Equals(entryName, rawName))
                                {
                                    alreadyOwned = true;
                                    break;
                                }
                            }
                        }

                        if (alreadyOwned)
                        {
                            Logger.Msg($"  Item already in internal 'PickedUp' list, skipping event: {rawName}");
                            continue;
                        }

                        string eventName = itemName + " Pickup";
                        if (itemName == "Lunch Box") eventName = "Luch Box Pickup";
                        else if (itemName == "Lucky Rabbit's Foot") eventName = "Lucky Rabbbit's Foot Pickup";
                        else if (itemName == "Wind-Up Key") eventName = "Wind-Up Key pickup";
                        else if (itemName.StartsWith("Safety Deposit Key")) eventName = itemName.Replace("Safety", "Saftey") + " Pickup";
                        
                        fsm.SendEvent(eventName);
                        Logger.Msg($"  Sent pickup event for: {rawName} (Event: {eventName})");
                    }
                }
            }
        }
    }
}
