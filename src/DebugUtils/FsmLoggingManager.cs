using UnityEngine;
using MelonLoader;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using Il2Cpp;
using System;
using System.Collections.Generic;


namespace QuickSave.DebugUtils
{
    public static class FsmLoggingManager
    {
        private static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        private static HashSet<PlayMakerFSM> _loggedFsms = new HashSet<PlayMakerFSM>();

        public static void enableFullFsmLogging(this PlayMakerFSM fsmToLog)
        {
            if (fsmToLog == null || fsmToLog.Fsm == null)
            {
                Logger.Warning("[FsmLoggingManager] fsmToLog or fsmToLog.Fsm is null.");
                return;
            }

            if (_loggedFsms.Contains(fsmToLog))
            {
                return;
            }

            _loggedFsms.Add(fsmToLog);

            foreach (var state in fsmToLog.Fsm.States)
            {
                var originalActions = state.Actions;
                int originalActionCount = originalActions.Length;
                
                for (int i = originalActionCount - 1; i >= 0; i--)
                {
                    var nextAction = originalActions[i];
                    string nextActionName = nextAction != null ? nextAction.GetIl2CppType().Name : "null";
                    string stateName = state.Name;

                    FsmHookManager.AddCallbackToFsmState(fsmToLog, stateName, i, (eventId) => {
                        string details = ActionLoggingUtilities.GetActionDetails(nextAction);
                        Logger.Msg($"[FsmLogging] '{fsmToLog.gameObject.name}' State '{stateName}' Action '{nextActionName}': {details}");
                    });
                }
            }
            
            Logger.Msg($"[FsmLoggingManager] Enabled full logging for FSM '{fsmToLog.FsmName}' on GameObject '{fsmToLog.gameObject.name}'.");
        }

        public static string disableFullFsmLogging(this PlayMakerFSM fsmToLog)
        {
            if (fsmToLog == null || fsmToLog.Fsm == null)
            {
                Logger.Warning("[FsmLoggingManager] fsmToLog or fsmToLog.Fsm is null.");
                return "Failed";
            }

            if (!_loggedFsms.Contains(fsmToLog))
            {
                return "Not logged";
            }

            _loggedFsms.Remove(fsmToLog);

            foreach (var state in fsmToLog.Fsm.States)
            {
                FsmHookManager.RemoveCallbacksFromFsmState(fsmToLog, state.Name);
            }
            
            Logger.Msg($"[FsmLoggingManager] Disabled full logging for FSM '{fsmToLog.FsmName}' on GameObject '{fsmToLog.gameObject.name}'.");
            
            return "Success";
        }

        public static void disableAllFullFsmLogging(int count)
        {
            var copy = new List<PlayMakerFSM>(_loggedFsms);
            int disabledCount = 0;
            foreach (var fsm in copy)
            {
                if (disabledCount >= count)
                {
                    break;
                }
                disableFullFsmLogging(fsm);
                disabledCount++;
            }
        }
    }
}
