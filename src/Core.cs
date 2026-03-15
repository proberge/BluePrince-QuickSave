using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Il2Cpp;
using System.Collections;
using QuickSave.DebugUtils;

[assembly: MelonInfo(typeof(QuickSave.Core), "QuickSave", "1.0.0", "YggBlueprint", null)]
[assembly: MelonGame("Dogubomb", "BLUE PRINCE")]

namespace QuickSave
{
    public class Core : MelonMod
    {
        public static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;
        private IEnumerator _quickLoadCoroutine;

        public override void OnInitializeMelon()
        {
            Logger.Msg("QuickSave Initialized.");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            Logger.Msg($"Scene Initialized: {sceneName}");
            FsmHookManager.OnSceneLoaded();
            
            if (sceneName == "Main Menu")
            {
                MainMenuManager.OnMainMenuReady();
            } else if (sceneName == "Mount Holly Estate")
            {
                InGameMenuManager.OnInGameSceneReady();
                if (MainMenuManager.ShouldLoadQuicksave)
                {
                    var dayFSM = GameObject.Find("/DAY").GetComponent<PlayMakerFSM>();
                    // This seems like one of the last events before the day is done loading.
                    FsmHookManager.AddCallbackToFsmState(dayFSM, "ZERO STAR", 0, (eventId) => {
                        // Reset the flag so we don't load again if we call it a day.
                        MainMenuManager.ShouldLoadQuicksave = false;
                        Logger.Msg("[InGameMenu] Performing QuickLoad...");
                        
                        // Executed as a coroutine since we need to do this in two steps (pickups and player after placing rooms).
                        _quickLoadCoroutine = SaveLoadManager.LoadFromSlot(MainMenuManager.CapturedProfileSlot);
                        MelonCoroutines.Start(_quickLoadCoroutine);  
                    });
                }
            }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            Logger.Msg($"Scene Unloaded: {sceneName}");
            FsmHookManager.OnSceneUnloaded();
            
            if (sceneName == "Main Menu")
            {
                MainMenuManager.OnMainMenuLeft();
            }
        }

        public override void OnUpdate()
        {
            FsmHookManager.OnUpdate();

            // Poll for profile changes while in the Main Menu.
            // Horribly inefficient - ideally we'd attach to the /CURRENT SAVE object
            // and listen for changes instead.
            if (SceneManager.GetActiveScene().name == "Main Menu")
                MainMenuManager.OnUpdate();

            // For debugging, enable save states with CTRL+1, 2, 3 and load states with CTRL+SHIFT+1, 2, 3
            DebugInputHandler.HandleInput();
        }
    }
}