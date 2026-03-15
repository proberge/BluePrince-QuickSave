using UnityEngine;
using MelonLoader;
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;

namespace QuickSave.Utils
{
    /// <summary>
    /// Centralised helpers for reading and writing PlayMaker FSM variables.
    /// All Get methods return a nullable value type: <c>null</c> means "not found",
    /// and a detailed error is always logged so callers don't need to.
    /// </summary>
    public static class FsmHelper
    {
        private static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        // -------------------------------------------------------------------------
        // GetFsmInt
        // -------------------------------------------------------------------------

        /// <summary>
        /// Reads an <c>FsmInt</c> variable from any FSM on the GameObject at
        /// <paramref name="goPath"/>. Returns <c>null</c> and logs an error on failure.
        /// </summary>
        public static int? GetFsmInt(string goPath, string varName)
        {
            try
            {
                GameObject go = GameObject.Find(goPath);
                if (go == null)
                {
                    Logger.Error($"[FsmHelper] GetFsmInt: GameObject not found: '{goPath}'");
                    return null;
                }

                foreach (var fsm in go.GetComponents<PlayMakerFSM>())
                {
                    var v = fsm.FsmVariables.GetFsmInt(varName);
                    if (v != null) return v.Value;
                }

                Logger.Error($"[FsmHelper] GetFsmInt: FsmInt '{varName}' not found on '{goPath}'");
                return null;
            }
            catch (System.Exception ex)
            {
                Logger.Error($"[FsmHelper] GetFsmInt('{goPath}', '{varName}'): {ex.Message}");
                return null;
            }
        }

        // -------------------------------------------------------------------------
        // SetFsmInt
        // -------------------------------------------------------------------------

        /// <summary>
        /// Writes <paramref name="value"/> to an <c>FsmInt</c> variable on the first
        /// matching FSM on the GameObject at <paramref name="goPath"/>, then sends each
        /// of <paramref name="sendEvents"/> to that FSM. Logs an error on failure.
        /// </summary>
        public static void SetFsmInt(string goPath, string varName, int value, string[] sendEvents = null)
        {
            try
            {
                GameObject go = GameObject.Find(goPath);
                if (go == null)
                {
                    Logger.Error($"[FsmHelper] SetFsmInt: GameObject not found: '{goPath}'");
                    return;
                }

                PlayMakerFSM targetFsm = null;
                foreach (var fsm in go.GetComponents<PlayMakerFSM>())
                {
                    var v = fsm.FsmVariables.GetFsmInt(varName);
                    if (v != null)
                    {
                        v.Value = value;
                        targetFsm = fsm;
                        break;
                    }
                }

                if (targetFsm == null)
                {
                    Logger.Error($"[FsmHelper] SetFsmInt: FsmInt '{varName}' not found on '{goPath}'");
                    return;
                }

                var events = sendEvents ?? new[] { "Update" };
                foreach (var ev in events)
                    if (!string.IsNullOrEmpty(ev)) targetFsm.SendEvent(ev);

                Logger.Msg($"[FsmHelper] Set '{varName}' = {value} on '{goPath}'");
            }
            catch (System.Exception ex)
            {
                Logger.Error($"[FsmHelper] SetFsmInt('{goPath}', '{varName}', {value}): {ex.Message}");
            }
        }
    }
}
