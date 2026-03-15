using System;
using System.IO;
using UnityEngine;
using MelonLoader;
using BluePrince.Modding.QuickSave;
using Google.Protobuf;
using System.Collections;
using QuickSave.Utils;

namespace QuickSave
{
    public static class SaveLoadManager
    {
        public static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        public static void SaveToSlot(int slot, string saveSuffix = "")
        {
            var saveState = new SaveState();
            saveState.Layout = LayoutManager.GetLayout();
            saveState.Inventory = InventoryManager.GetInventory();
            saveState.Player = PlayerManager.GetPlayer();
            saveState.Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            
            PickupItemManager.SavePickupItems(saveState);
            // Save room-specific puzzle states
            QuickSave.RoomStateHandlers.RoomStateManager.SaveAllRoomStates(saveState);

            // /CURRENT SAVE only exists in the Main Menu scene; use the value captured there.
            saveState.ProfileSlot = MainMenuManager.CapturedProfileSlot;
            var currentDay = FsmHelper.GetFsmInt("DAY", "Day");
            if (currentDay != null)
            {
                saveState.CurrentDay = (int)currentDay;
            } else {
                Logger.Error("Failed to get current day from FSM, not attempting to save.");
                return;
            }

            string savePath = $"UserData/SaveState{slot}{saveSuffix}.textproto";
            try
            {
                File.WriteAllText(savePath, saveState.ToString());  
                Logger.Msg($"Saved state to slot {slot} ({savePath})");
                Logger.Msg(saveState.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save state to slot {slot}: {ex.Message}");
            }
        }

        public static IEnumerator LoadFromSlot(int slot, string saveSuffix = "")
        {
            string savePath = $"UserData/SaveState{slot}{saveSuffix}.textproto";
            if (!File.Exists(savePath))
            {
                Logger.Warning($"No save file found at {savePath}");
                yield break;
            }

            string text = File.ReadAllText(savePath);
            var saveState = Google.Protobuf.JsonParser.Default.Parse<SaveState>(text);

            Logger.Msg($"Loading state from slot {slot} ({savePath})...");

            LayoutManager.RestoreLayout(saveState.Layout);
            LayoutManager.RestoreMap(saveState.Layout);
            InventoryManager.RestoreInventory(saveState.Inventory);

            yield return null; 
            
            PlayerManager.RestorePlayer(saveState.Player);
            PickupItemManager.RestorePickupItems(saveState);

            // Restore room-specific puzzle states
            QuickSave.RoomStateHandlers.RoomStateManager.LoadAllRoomStates(saveState);

            Logger.Msg($"State loaded from slot {slot}.");
        }
    }
}
