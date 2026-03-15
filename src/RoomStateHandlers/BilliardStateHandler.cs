using BluePrince.Modding.QuickSave;
using MelonLoader;

namespace QuickSave.RoomStateHandlers
{
    public class BilliardStateHandler : IRoomStateHandler
    {
        private static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        public string RoomName => "Billiard Room";

        public void SaveState(RoomPuzzleState state)
        {
            state.Billiard = new BilliardState();
            // TODO: Research Billiard room puzzle FSMs and GameObjects
            Logger.Msg("Billiard room save state not yet implemented.");
        }

        public void LoadState(RoomPuzzleState state)
        {
            if (state.Billiard == null) return;
            // TODO: Implement Billiard room loading logic
            Logger.Msg("Billiard room load state not yet implemented.");
        }
    }
}
