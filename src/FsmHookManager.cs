using UnityEngine;
using MelonLoader;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using Il2Cpp;
using System;
using System.Collections.Generic;
using QuickSave.DebugUtils;

namespace QuickSave
{
    public static class FsmHookManager
    {
        private static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        private static GameObject _eventManagerGo;
        private static PlayMakerFSM _eventManagerFsm;
        private static Dictionary<string, Action<string>> _callbacks = new Dictionary<string, Action<string>>();

        public static void OnSceneLoaded()
        {
            // Clear existing callbacks when a new scene loads since GameObjects (and their FSMs) are destroyed.
            _callbacks.Clear();
            
            _eventManagerGo = new GameObject("QuickSaveModEventManager");
            _eventManagerFsm = _eventManagerGo.AddComponent<PlayMakerFSM>();
            _eventManagerFsm.FsmName = "FsmHookEventManager";
        }

        public static void OnSceneUnloaded()
        {
            if (_eventManagerGo != null)
            {
                GameObject.Destroy(_eventManagerGo);
                _eventManagerGo = null;
                _eventManagerFsm = null;
            }
            _callbacks.Clear();
        }

        public static void AddCallbackToFsmState(PlayMakerFSM fsm, string stateName, int targetActionIndex, Action<string> callback)
        {
            if (fsm == null)
            {
                Logger.Error($"[FsmHookManager] AddCallbackToFsmState: Cannot add callback, fsm is null.");
                return;
            }

            var state = fsm.Fsm.GetState(stateName);
            if (state == null)
            {
                Logger.Error($"[FsmHookManager] AddCallbackToFsmState: State '{stateName}' not found on FSM '{fsm.FsmName}' (GameObject: {fsm.gameObject.name}).");
                return;
            }

            if (_eventManagerFsm == null)
            {
                Logger.Error($"[FsmHookManager] AddCallbackToFsmState: EventManager FSM is null. Was OnSceneLoaded called?");
                return;
            }

            string eventId = $"{fsm.gameObject.name}_{fsm.FsmName}_{stateName}_{Guid.NewGuid().ToString("N")}";
            _callbacks[eventId] = callback;

            // Create FsmBool variable in our EventManager
            var fsmBool = new FsmBool(eventId);
            fsmBool.Value = false;
            
            var vars = new List<FsmBool>(_eventManagerFsm.FsmVariables.BoolVariables);
            vars.Add(fsmBool);
            _eventManagerFsm.FsmVariables.BoolVariables = vars.ToArray();
            _eventManagerFsm.FsmVariables.AddVariableLookup(fsmBool);

            // Construct the action to set our event bool to true
            var setBoolAction = new SetFsmBool();
            setBoolAction.fsmName = "FsmHookEventManager";
            setBoolAction.gameObject = new FsmOwnerDefault();
            setBoolAction.gameObject.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
            setBoolAction.gameObject.GameObject = new FsmGameObject { Value = _eventManagerGo };
            setBoolAction.variableName = new FsmString { Value = eventId };
            setBoolAction.setValue = new FsmBool { Value = true };
            setBoolAction.everyFrame = false;

            // Insert into target action list
            var actions = new List<FsmStateAction>(state.Actions);
            if (targetActionIndex < 0 || targetActionIndex > actions.Count)
            {
                targetActionIndex = actions.Count; // Append if out of bounds
            }
            actions.Insert(targetActionIndex, setBoolAction);
            state.Actions = actions.ToArray();

            Logger.Msg($"[FsmHookManager] Added callback for state '{stateName}' on '{fsm.gameObject.name}' with eventId '{eventId}'.");
        }

        public static void RemoveCallbacksFromFsmState(PlayMakerFSM fsm, string stateName)
        {
            if (fsm == null)
            {
                Logger.Error($"[FsmHookManager] RemoveCallbacksFromFsmState: Cannot remove callback, fsm is null.");
                return;
            }

            var state = fsm.Fsm.GetState(stateName);
            if (state == null)
            {
                Logger.Error($"[FsmHookManager] RemoveCallbacksFromFsmState: State '{stateName}' not found on FSM '{fsm.FsmName}' (GameObject: {fsm.gameObject.name}).");
                return;
            }

            if (_eventManagerFsm == null)
            {
                Logger.Error($"[FsmHookManager] RemoveCallbacksFromFsmState: EventManager FSM is null. Was OnSceneLoaded called?");
                return;
            }

            var newActions = new List<FsmStateAction>();
            var vars = new List<FsmBool>(_eventManagerFsm.FsmVariables.BoolVariables);
            bool actionsChanged = false;

            foreach (var action in state.Actions)
            {
                var setBool = action.TryCast<SetFsmBool>();
                if (setBool != null && setBool.fsmName != null && setBool.fsmName.Value == "FsmHookEventManager")
                {
                    string eventId = setBool.variableName.Value;
                    
                    // Remove from callbacks dictionary
                    _callbacks.Remove(eventId);

                    // Remove from EventManager FSM BoolVariables
                    vars.RemoveAll(v => v.Name == eventId);

                    actionsChanged = true;
                    continue; // Skip adding to newActions list to effectively remove it
                }
                newActions.Add(action);
            }

            if (actionsChanged)
            {
                state.Actions = newActions.ToArray();
                _eventManagerFsm.FsmVariables.BoolVariables = vars.ToArray();
                Logger.Msg($"[FsmHookManager] Removed callback(s) for state '{stateName}' on '{fsm.gameObject.name}'.");
            }
        }

        public static void OnUpdate()
        {
            if (_eventManagerFsm == null) {
                Logger.Error("[FsmHookManager] EventManager FSM is null. Was OnSceneLoaded called?");
                return;
            }

            foreach (var kvp in _callbacks)
            {
                string eventId = kvp.Key;
                Action<string> callback = kvp.Value;

                var fsmBool = _eventManagerFsm.FsmVariables.GetFsmBool(eventId);
                if (fsmBool != null && fsmBool.Value)
                {
                    // Reset to false before triggering to prevent infinite loops if callback does something weird
                    fsmBool.Value = false;
                    try
                    {
                        callback?.Invoke(eventId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[FsmHookManager] Exception in callback for event '{eventId}': {ex}");
                    }
                } else if (fsmBool == null) {
                    Logger.Error($"[FsmHookManager] FSM variable '{eventId}' not found.");
                }
            }
        }
    }
}
