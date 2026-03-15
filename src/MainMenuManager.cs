using System.IO;
using UnityEngine;
using MelonLoader;
using Il2Cpp;
using Il2CppTMPro;
using Il2CppHutongGames.PlayMaker;
using QuickSave.Utils;

namespace QuickSave
{
    public static class MainMenuManager
    {
        public static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        /// <summary>
        /// Set to true when the main menu detects a valid slot-0 quicksave that matches
        /// the active profile and day. The game loading logic should check this flag and
        /// load from |CapturedProfileSlot| instead of triggering a normal Continue.
        /// </summary>
        public static bool ShouldLoadQuicksave { get; set; } = false;

        /// <summary>
        /// The profile slot that was active in the most recent Main Menu session.
        /// Captured from the '/CURRENT SAVE' FSM before it is unloaded with the
        /// Main Menu scene, so that SaveLoadManager can use it during gameplay.
        /// Updated live whenever the player switches profiles on the Main Menu.
        /// </summary>
        public static int CapturedProfileSlot { get; private set; } = 0;

        // Cached reference to the FsmInt variable — obtained once in OnMainMenuReady,
        // then read cheaply each frame in OnUpdate without any GameObject.Find calls.
        private static FsmInt _currentSaveFsmInt = null;

        // Set to true when we need to re-evaluate the quicksave offer on the next frame.
        // This handles cases where certain FSM variables (like the Day counter) update
        // a frame later than others (like the active profile slot).
        private static bool _needsRefreshNextFrame = false;

        // --------------------------------------------------------------------
        // Called by Core.OnSceneWasInitialized when the Main Menu scene is ready
        // --------------------------------------------------------------------

        public static void OnMainMenuReady()
        {
            ShouldLoadQuicksave = false;
            _currentSaveFsmInt = null;

            // ----------------------------------------------------------------
            // 0. Cache a direct reference to the FsmInt variable on /CURRENT SAVE.
            //    We read it once here and then poll .Value cheaply each frame.
            // ----------------------------------------------------------------
            try
            {
                var go = GameObject.Find("/CURRENT SAVE");
                if (go != null)
                {
                    foreach (var fsm in go.GetComponents<PlayMakerFSM>())
                    {
                        var v = fsm.FsmVariables.GetFsmInt("current save");
                        if (v != null)
                        {
                            _currentSaveFsmInt = v;
                            break;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Warning($"[MainMenu] Could not cache /CURRENT SAVE FsmInt: {ex.Message}");
            }

            if (_currentSaveFsmInt != null)
            {
                CapturedProfileSlot = _currentSaveFsmInt.Value;
                Logger.Msg($"[MainMenu] Captured profile slot: {CapturedProfileSlot}");
            }
            else
            {
                Logger.Error("[MainMenu] Could not find 'current save' FsmInt on /CURRENT SAVE.");
            }

            _needsRefreshNextFrame = true;
        }

        // --------------------------------------------------------------------
        // Called every frame by Core.OnUpdate while the Main Menu scene is active
        // --------------------------------------------------------------------

        public static void OnUpdate()
        {
            if (_needsRefreshNextFrame)
            {
                var go = GameObject.Find("MAIN MENU/MAINMENU LAYOUT/Continue Click");
                if (go != null && go.activeInHierarchy)
                {
                    _needsRefreshNextFrame = false;
                    RefreshQuicksaveOffer();
                }
            }

            if (_currentSaveFsmInt == null) return;

            int current = _currentSaveFsmInt.Value;
            if (current == CapturedProfileSlot) return;

            // Profile has changed — update the captured slot and re-evaluate the
            // quicksave offer (which may need to show or hide the "(Quicksave)" label).
            Logger.Msg($"[MainMenu] Profile switched: slot {CapturedProfileSlot} → {current}");
            CapturedProfileSlot = current;
            _needsRefreshNextFrame = true;
        }

        // --------------------------------------------------------------------
        // Clears state when leaving the Main Menu (FSM objects are about to be
        // destroyed). Core.OnSceneWasInitialized calls this for any non-menu scene.
        // --------------------------------------------------------------------

        public static void OnMainMenuLeft()
        {
            _currentSaveFsmInt = null;
            _needsRefreshNextFrame = false;
            // Leave ShouldLoadQuicksave and CapturedProfileSlot intact — that's the whole point.
        }

        // --------------------------------------------------------------------
        // Core quicksave validation logic — runs on entry and on every profile switch
        // --------------------------------------------------------------------

        private static void RefreshQuicksaveOffer()
        {
            ShouldLoadQuicksave = false;

            // Reset the button text first so a stale "(Quicksave)" label is never shown
            // after switching to a profile with no matching quicksave.
            UpdateContinueButtonText("Continue");

            // ----------------------------------------------------------------
            // 1. Does slot-0 exist?
            // ----------------------------------------------------------------
            if (!File.Exists($"UserData/SaveState{CapturedProfileSlot}.textproto"))
            {
                Logger.Msg($"[MainMenu] No slot-{CapturedProfileSlot} quicksave found.");
                return;
            }

            BluePrince.Modding.QuickSave.SaveState saved;
            try
            {
                string text = File.ReadAllText($"UserData/SaveState{CapturedProfileSlot}.textproto");
                saved = Google.Protobuf.JsonParser.Default.Parse<BluePrince.Modding.QuickSave.SaveState>(text);
            }
            catch (System.Exception ex)
            {
                Logger.Warning($"[MainMenu] Error reading slot-{CapturedProfileSlot}: {ex.Message}");
                return;
            }

            // ----------------------------------------------------------------
            // 2. Active profile slot — already in CapturedProfileSlot
            // ----------------------------------------------------------------
            int activeProfile = CapturedProfileSlot;

            // ----------------------------------------------------------------
            // 3. Read active day from "/CURRENT SAVE/Stat Load" FSM
            // ----------------------------------------------------------------
            int? activeDay = FsmHelper.GetFsmInt("/CURRENT SAVE/Stat Load", "DAY");
            if (activeDay == null)
            {
                // FsmHelper already logged the error.
                return;
            }

            Logger.Msg($"[MainMenu] Slot-{CapturedProfileSlot}: profile={saved.ProfileSlot} day={saved.CurrentDay} | Active: profile={activeProfile} day={activeDay}");

            // ----------------------------------------------------------------
            // 4. Compare — slot must match both profile and day
            // ----------------------------------------------------------------
            if (saved.ProfileSlot != activeProfile || saved.CurrentDay != activeDay)
            {
                Logger.Msg($"[MainMenu] Slot-{CapturedProfileSlot} does not match current profile/day — not offering quicksave.");
                return;
            }

            // ----------------------------------------------------------------
            // 5. Update the Continue button text
            // ----------------------------------------------------------------
            bool textUpdated = UpdateContinueButtonText("Continue (Quicksave)");
            if (textUpdated)
            {
                ShouldLoadQuicksave = true;
                Logger.Msg("[MainMenu] Quicksave matches — Continue button updated.");
            }
        }

        // --------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------


        /// <summary>
        /// Finds the TextMeshPro component in the Continue button hierarchy and sets its text.
        /// Returns true if successful.
        /// </summary>
        private static bool UpdateContinueButtonText(string newText)
        {
            try
            {
                // Try with and without trailing slash
                GameObject continueGo = GameObject.Find("MAIN MENU/MAINMENU LAYOUT/Continue Click");

                if (continueGo == null)
                {
                    Logger.Warning("[MainMenu] Could not find 'Continue Click' GameObject.");
                    return false;
                }

                // TMP may live on the GO itself or a child
                var tmp = continueGo.GetComponentInChildren<TextMeshPro>();
                if (tmp == null)
                {
                    Logger.Warning("[MainMenu] No TextMeshPro found on/under 'Continue Click'.");
                    return false;
                }

                tmp.text = newText;
                Logger.Msg($"[MainMenu] Continue button text set to: '{newText}'");
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Warning($"[MainMenu] UpdateContinueButtonText error: {ex.Message}");
                return false;
            }
        }
    }
}
