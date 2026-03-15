using UnityEngine;

namespace QuickSave.DebugUtils
{
    public static class DebugInputHandler
    {
        private static readonly KeyCode[] SlotKeys = {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        };

        public static void HandleInput()
        {
            bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            for (int i = 0; i < SlotKeys.Length; i++)
            {
                if (Input.GetKeyDown(SlotKeys[i]))
                {
                    int slot = i + 1;
                    if (ctrlHeld)
                        SaveLoadManager.SaveToSlot(slot, "_Manual");
                    else if (shiftHeld)
                        SaveLoadManager.LoadFromSlot(slot, "_Manual");
                    break;
                }
            }
        }
    }
}
