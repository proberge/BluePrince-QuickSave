using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using MelonLoader;

namespace QuickSave.DebugUtils
{
    public static class ActionLoggingUtilities
    {
        private static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        private static string GetTargetState(FsmStateAction action, FsmEvent fsmEvent)
        {
            if (fsmEvent == null || action == null || action.State == null || action.State.Transitions == null) return "None";
            foreach (var transition in action.State.Transitions)
            {
                if (transition.EventName == fsmEvent.Name)
                {
                    return transition.ToState;
                }
            }
            return "None";
        }

        private static string FormatStateTransition(FsmStateAction action, FsmEvent fsmEvent)
        {
            string target = GetTargetState(action, fsmEvent);
            return target == "None" ? "do nothing" : $"change state to {target}";
        }

        public static string GetActionDetails(FsmStateAction action)
        {
            if (action == null) return "null";

            // State changes
            var setFsmInt = action.TryCast<SetFsmInt>();
            if (setFsmInt != null) return $"[{setFsmInt.gameObject?.GameObject?.Value?.name}] '{setFsmInt.variableName?.Value}' -> {setFsmInt.setValue?.Value}";

            var setFsmBool = action.TryCast<SetFsmBool>();
            if (setFsmBool != null) return $"[{setFsmBool.gameObject?.GameObject?.Value?.name}] '{setFsmBool.variableName?.Value}' -> {setFsmBool.setValue?.Value}";

            var setFsmFloat = action.TryCast<SetFsmFloat>();
            if (setFsmFloat != null) return $"[{setFsmFloat.gameObject?.GameObject?.Value?.name}] '{setFsmFloat.variableName?.Value}' -> {setFsmFloat.setValue?.Value}";

            var setFsmString = action.TryCast<SetFsmString>();
            if (setFsmString != null) return $"[{setFsmString.gameObject?.GameObject?.Value?.name}] '{setFsmString.variableName?.Value}' -> '{setFsmString.setValue?.Value}'";

            var setFsmGameObject = action.TryCast<SetFsmGameObject>();
            if (setFsmGameObject != null) return $"[{setFsmGameObject.gameObject?.GameObject?.Value?.name}] '{setFsmGameObject.variableName?.Value}' -> {setFsmGameObject.setValue?.Value?.name}";

            var getFsmInt = action.TryCast<GetFsmInt>();
            if (getFsmInt != null) return $"[{getFsmInt.gameObject?.GameObject?.Value?.name}] Get '{getFsmInt.variableName?.Value}' -> Store in '{getFsmInt.storeValue?.Name}'";

            var getFsmBool = action.TryCast<GetFsmBool>();
            if (getFsmBool != null) return $"[{getFsmBool.gameObject?.GameObject?.Value?.name}] Get '{getFsmBool.variableName?.Value}' -> Store in '{getFsmBool.storeValue?.Name}'";

            var getFsmFloat = action.TryCast<GetFsmFloat>();
            if (getFsmFloat != null) return $"[{getFsmFloat.gameObject?.GameObject?.Value?.name}] Get '{getFsmFloat.variableName?.Value}' -> Store in '{getFsmFloat.storeValue?.Name}'";

            var getFsmString = action.TryCast<GetFsmString>();
            if (getFsmString != null) return $"[{getFsmString.gameObject?.GameObject?.Value?.name}] Get '{getFsmString.variableName?.Value}' -> Store in '{getFsmString.storeValue?.Name}'";

            var getFsmGameObject = action.TryCast<GetFsmGameObject>();
            if (getFsmGameObject != null) return $"[{getFsmGameObject.gameObject?.GameObject?.Value?.name}] Get '{getFsmGameObject.variableName?.Value}' -> Store in '{getFsmGameObject.storeValue?.Name}'";

            var getOwner = action.TryCast<GetOwner>();
            if (getOwner != null) return $"Store in '{getOwner.storeGameObject?.Name}'";

            var setBoolValue = action.TryCast<SetBoolValue>();
            if (setBoolValue != null) return $"'{setBoolValue.boolVariable?.Name}' -> {setBoolValue.boolValue?.Value}";

            var intAdd = action.TryCast<IntAdd>();
            if (intAdd != null) return $"'{intAdd.intVariable?.Name}' += {intAdd.add?.Value}";

            var intCompare = action.TryCast<IntCompare>();
            if (intCompare != null)
            {
                string stateTransitions = $"(If Equal: {FormatStateTransition(action, intCompare.equal)}, if Less: {FormatStateTransition(action, intCompare.lessThan)}, if Greater: {FormatStateTransition(action, intCompare.greaterThan)})";
                return $"Test '{intCompare.integer1?.Name}' ({intCompare.integer1?.Value}) == '{intCompare.integer2?.Name}' ({intCompare.integer2?.Value}) {stateTransitions}";
            }

            // Logic and Control Flow
            var boolTest = action.TryCast<BoolTest>();
            if (boolTest != null) return $"Test '{boolTest.boolVariable?.Name}' == {boolTest.boolVariable?.Value} (If True: {FormatStateTransition(action, boolTest.isTrue)}, if False: {FormatStateTransition(action, boolTest.isFalse)})";

            var wait = action.TryCast<Wait>();
            if (wait != null) return $"Wait {wait.time?.Value}s -> {GetTargetState(action, wait.finishEvent)}";

            var nextFrameEvent = action.TryCast<NextFrameEvent>();
            if (nextFrameEvent != null) return $"-> {GetTargetState(action, nextFrameEvent.sendEvent)}";

            // Collections
            var arrayListContains = action.TryCast<ArrayListContains>();
            if (arrayListContains != null)
            {
                string value = !string.IsNullOrEmpty(arrayListContains.variable?.variableName) ? $"'{arrayListContains.variable.variableName}'" : $"({arrayListContains.variable?.ToString() ?? "null"})";
                return $"Check '{arrayListContains.reference?.Value}' for {value} (If True: {FormatStateTransition(action, arrayListContains.isContainedEvent)}, if False: {FormatStateTransition(action, arrayListContains.isNotContainedEvent)})";
            }

            var arrayListAdd = action.TryCast<ArrayListAdd>();
            if (arrayListAdd != null)
            {
                string value = !string.IsNullOrEmpty(arrayListAdd.variable?.variableName) ? $"'{arrayListAdd.variable.variableName}'" : $"({arrayListAdd.variable?.ToString() ?? "null"})";
                return $"Add {value} to '{arrayListAdd.reference?.Value}'";
            }

            var arrayListRemove = action.TryCast<ArrayListRemove>();
            if (arrayListRemove != null)
            {
                string value = !string.IsNullOrEmpty(arrayListRemove.variable?.variableName) ? $"'{arrayListRemove.variable.variableName}'" : $"({arrayListRemove.variable?.ToString() ?? "null"})";
                return $"Remove {value} from '{arrayListRemove.reference?.Value}'";
            }

            // Other FSM interactions
            var sendEvent = action.TryCast<SendEvent>();
            if (sendEvent != null)
            {
                string target = sendEvent.eventTarget?.gameObject?.GameObject?.Value?.name ?? "Self/Unknown";
                return $"Event '{sendEvent.sendEvent?.Name}' to '{target}'";
            }

            var sendEventByName = action.TryCast<SendEventByName>();
            if (sendEventByName != null)
            {
                string target = sendEventByName.eventTarget?.gameObject?.GameObject?.Value?.name ?? "Self/Unknown";
                return $"Event '{sendEventByName.sendEvent?.Name}' to '{target}'";
            }

            var activateGov = action.TryCast<ActivateGameObject>();
            if (activateGov != null) return $"Activate '{activateGov.gameObject?.GameObject?.Value?.name}': {activateGov.activate?.Value}";

            var mousePickEvent = action.TryCast<MousePickEvent>();
            if (mousePickEvent != null) return $"Target: {mousePickEvent.GameObject?.GameObject?.Value?.name}";

            var pmtSpawn = action.TryCast<PmtSpawn>();
            if (pmtSpawn != null)
            {
                string goName = pmtSpawn.gameObject?.Value?.name ?? "null";
                string poolName = pmtSpawn.poolName?.Value ?? "null";
                string spawnTransformName = pmtSpawn.spawnTransform?.Value?.name ?? "null";
                return $"Spawning GameObject [{goName}] in [{poolName}] at position [{spawnTransformName}]";
            }

            var pmtDeSpawn = action.TryCast<PmtDeSpawn>();
            if (pmtDeSpawn != null)
            {
                string goName = pmtDeSpawn.gameObject?.Value?.name ?? "null";
                string poolName = pmtDeSpawn.poolName?.Value ?? "null";
                string delayOutput = pmtDeSpawn.delay != null ? $"{pmtDeSpawn.delay.Value}s" : "no delay";
                return $"Despawning GameObject [{goName}] in [{poolName}] in {delayOutput}";
            }

            var iTweenMoveTo = action.TryCast<iTweenMoveTo>();
            if (iTweenMoveTo != null) return "<iTweenMoveTo>";

            var iTweenRotateBy = action.TryCast<iTweenRotateBy>();
            if (iTweenRotateBy != null) return "<iTweenRotateBy>";

            var iTweenScaleTo = action.TryCast<iTweenScaleTo>();
            if (iTweenScaleTo != null) return "<iTweenScaleTo>";

            var playSound = action.TryCast<PlaySound>();
            if (playSound != null) return "<PlaySound>";

            string typeName = action.GetIl2CppType() != null ? action.GetIl2CppType().Name : action.GetType().Name;
            Logger.Warning($"[ActionLoggingUtilities] Unhandled Action Type: {typeName}");
            return "Unhandled";
        }
    }
}
