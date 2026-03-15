using UnityEngine;
using MelonLoader;
using Il2CppTMPro;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using Il2Cpp;

namespace QuickSave
{
    public static class InGameMenuManager
    {
        public static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        private static PlayMakerFSM _buttonFsm = null;

        public static void OnInGameSceneReady()
        {
            var saveAndQuitButton = GameObject.Find("UI OVERLAY CAM/PAUSE/SYSTEM MENU/SETTINGS/SAVE & QUIT");
            if (saveAndQuitButton == null)
            {
                Logger.Warning("[InGameMenu] Could not find 'SAVE & QUIT' button.");
                return;
            }

            // Find the FSM that handles the click/quit logic
            var fsm = saveAndQuitButton.GetComponent<PlayMakerFSM>();
            _buttonFsm = fsm;

            var tmp = saveAndQuitButton.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
            {
                // Yes, there's 4 line breaks before the SAVE & QUIT string...
                tmp.text = """




QUICKSAVE & QUIT
""";
                Logger.Msg("[InGameMenu] Updated 'SAVE & QUIT' text to 'QuickSave & Quit'");
            }
            else
            {
                Logger.Warning("[InGameMenu] No TextMeshProUGUI found on 'SAVE & QUIT' button.");
            }

            // Block the event which opens the "Call it a Day?" sub-menu.
            var activateCallItADayAction = fsm.Fsm.GetState("State 1").Actions[2];
            activateCallItADayAction.Cast<ActivateGameObject>().activate = false;

            // Add callback to the click action state. Before the "Call it a day menu" is opened.
            FsmHookManager.AddCallbackToFsmState(fsm, "State 1", 2, (eventId) => {
                Logger.Msg("[InGameMenu] Performing QuickSave before quit...");
                SaveLoadManager.SaveToSlot(MainMenuManager.CapturedProfileSlot);

                Logger.Msg("[InGameMenu] Quitting to main menu...");
                var saveSystemFsm = GameObject.Find("/DAY/SAVE SYSTEM").GetComponent<PlayMakerFSM>();
                saveSystemFsm.Fsm.SetState("Quit to main menu");
            });
        }
    }
}
