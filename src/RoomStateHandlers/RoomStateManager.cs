using System.Collections.Generic;
using BluePrince.Modding.QuickSave;
using UnityEngine;
using MelonLoader;

namespace QuickSave.RoomStateHandlers
{
    public static class RoomStateManager
    {
        private static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;
        private static readonly Dictionary<string, IRoomStateHandler> Handlers = new Dictionary<string, IRoomStateHandler>();

        static RoomStateManager()
        {
            RegisterHandler(new ParlorStateHandler());
            RegisterHandler(new BilliardStateHandler());
        }

        private static void RegisterHandler(IRoomStateHandler handler)
        {
            Handlers[handler.RoomName] = handler;
        }

        public static void SaveAllRoomStates(SaveState saveState)
        {
            // Find all active rooms in the scene
            GameObject roomContainer = GameObject.Find("__SYSTEM/Room Spawn Pools/");
            if (roomContainer == null) return;

            for (int i = 0; i < roomContainer.transform.childCount; i++)
            {
                Transform child = roomContainer.transform.GetChild(i);
                string cleanName = GetCleanRoomName(child.name);

                if (Handlers.TryGetValue(cleanName, out var handler))
                {
                    var roomPuzzleState = new RoomPuzzleState { RoomName = cleanName };
                    handler.SaveState(roomPuzzleState);
                    saveState.PuzzleStates.Add(roomPuzzleState);
                    Logger.Msg($"Saved puzzle state for room: {cleanName}");
                }
            }
        }

        public static void LoadAllRoomStates(SaveState saveState)
        {
            foreach (var puzzleState in saveState.PuzzleStates)
            {
                if (Handlers.TryGetValue(puzzleState.RoomName, out var handler))
                {
                    handler.LoadState(puzzleState);
                    Logger.Msg($"Loaded puzzle state for room: {puzzleState.RoomName}");
                }
                else
                {
                    Logger.Warning($"No handler found for saved room state: {puzzleState.RoomName}");
                }
            }
        }

        private static string GetCleanRoomName(string name)
        {
            int cloneIndex = name.IndexOf("(Clone)");
            if (cloneIndex >= 0) return name.Substring(0, cloneIndex);
            return name;
        }
    }
}
