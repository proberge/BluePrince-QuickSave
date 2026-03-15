using BluePrince.Modding.QuickSave;

namespace QuickSave.RoomStateHandlers
{
    public interface IRoomStateHandler
    {
        string RoomName { get; }
        void SaveState(RoomPuzzleState state);
        void LoadState(RoomPuzzleState state);
    }
}
