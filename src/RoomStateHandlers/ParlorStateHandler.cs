using BluePrince.Modding.QuickSave;
using UnityEngine;
using MelonLoader;
using Il2Cpp;
using QuickSave.RoomStateHandlers;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;

namespace QuickSave.RoomStateHandlers
{
    public class ParlorStateHandler : IRoomStateHandler
    {
        private static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        public string RoomName => "Parlor";

        private const string RoomRootPath = "__SYSTEM/Room Spawn Pools/Parlor(Clone)";
        private const string ParlorGamePath = "_GAMEPLAY/PARLOR GAME";
        
        public void SaveState(RoomPuzzleState state)
        {
            GameObject parlor = GetParlorRoom();
            if (parlor == null)
            {
                Logger.Error("Failed to find Parlor room for saving state.");
                return;
            }

            var parlorState = new ParlorState();
            var parlorGameGo = parlor.transform.Find(ParlorGamePath)?.gameObject;
            if (parlorGameGo == null)
            {
                Logger.Error($"Failed to find Parlor game object at {ParlorGamePath} for saving state.");
                return;
            }

            var parlorGameFsm = parlorGameGo.GetComponent<PlayMakerFSM>();
            if (parlorGameFsm != null)
            {
                parlorState.Solution = parlorGameFsm.FsmVariables.GetFsmInt("Solution").Value;
                parlorState.Correct = parlorGameFsm.FsmVariables.GetFsmBool("Correct").Value;
            }

            // Box Open Status & Gems
            parlorState.BlueBoxUnlocked = IsBoxUnlocked(parlor, "Blue");
            parlorState.BlackBoxUnlocked = IsBoxUnlocked(parlor, "Black");
            parlorState.WhiteBoxUnlocked = IsBoxUnlocked(parlor, "White");

            state.Parlor = parlorState;
        }

        public void LoadState(RoomPuzzleState state)
        {
            if (state.Parlor == null)
            {
                Logger.Warning("No Parlor state found in save state.");
                return;
            }
            GameObject parlor = GetParlorRoom();
            if (parlor == null)
            {
                Logger.Error("Failed to find Parlor room for loading state.");
                return;
            }

            var parlorState = state.Parlor;
            var parlorGameGo = parlor.transform.Find(ParlorGamePath)?.gameObject;
            if (parlorGameGo == null)
            {
                Logger.Error($"Failed to find Parlor game object at {ParlorGamePath} for loading state.");
                return;
            }

            var parlorGameFsm = parlorGameGo.GetComponent<PlayMakerFSM>();
            if (parlorGameFsm != null)
            {
                parlorGameFsm.FsmVariables.GetFsmInt("Solution").Value = parlorState.Solution;
                parlorGameFsm.FsmVariables.GetFsmBool("Correct").Value = parlorState.Correct;
            } else {
                Logger.Error($"Failed to find Parlor game FSM at {ParlorGamePath} for loading state.");
                return;
            }

            // Restore Box/Gem states
            SetBoxUnlocked(parlor, "Blue", parlorState.BlueBoxUnlocked);
            SetBoxUnlocked(parlor, "Black", parlorState.BlackBoxUnlocked);
            SetBoxUnlocked(parlor, "White", parlorState.WhiteBoxUnlocked);
        }

        private GameObject GetParlorRoom()
        {
            GameObject roomContainer = GameObject.Find("__SYSTEM/Room Spawn Pools/");
            if (roomContainer == null) return null;

            for (int i = 0; i < roomContainer.transform.childCount; i++)
            {
                GameObject child = roomContainer.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Parlor")) return child;
            }
            Logger.Error("Failed to find Parlor room for saving state.");
            return null;
        }

        private bool IsBoxUnlocked(GameObject parlor, string color)
        {
            var keyInSlot = parlor.transform.Find($"{ParlorGamePath}/ParlorBox {color}/Parlor Box/Keyhole/Wind Up Key");
            return keyInSlot != null && keyInSlot.gameObject.activeSelf;
        }

        private void SetBoxUnlocked(GameObject parlor, string color, bool open)
        {
            if (!open) return;

            var keyInSlot = parlor.transform.Find($"{ParlorGamePath}/ParlorBox {color}/Parlor Box/Keyhole/Wind Up Key");
            if (keyInSlot != null)
            {
                keyInSlot.gameObject.SetActive(true);
            }

            var keyHoleFsm = parlor.transform.Find($"{ParlorGamePath}/ParlorBox {color}/Parlor Box/Keyhole").GetComponent<PlayMakerFSM>();
            if (keyHoleFsm != null)
            {
                var keyTweenFsm = keyHoleFsm.Fsm.GetState("Click").Actions[3];
                var keyTween1 = keyTweenFsm.TryCast<iTweenMoveTo>();
                if (keyTween1 != null) keyTween1.DoiTween();

                var keyTweenFsm2 = keyHoleFsm.Fsm.GetState("State 2").Actions[2];
                var keyTween2 = keyTweenFsm2.TryCast<iTweenRotateBy>();
                if (keyTween2 != null) keyTween2.DoiTween();
            }
            
            string lidPath = $"{ParlorGamePath}/ParlorBox {color}/Parlor Box {color} Lid/OUT_Parlor Box Lid/Lid Collider - Click blocker";
            var lid = parlor.transform.Find(lidPath);
            if (lid != null)
            {
                var fsm = lid.gameObject.GetComponent<PlayMakerFSM>();
                if (fsm != null)
                {
                    fsm.SendEvent("activate");
                }
            }
        } 

    }
}
