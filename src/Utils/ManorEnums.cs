using UnityEngine;
using BluePrince.Modding.QuickSave;
using Vector3 = UnityEngine.Vector3;

namespace QuickSave.Utils
{
    public struct ManorPosition
    {
        public Rank Rank;
        public Column Column;

        public ManorPosition(Rank rank, Column column)
        {
            Rank = rank;
            Column = column;
        }

        public override string ToString() => $"{(char)('A' + (int)Column - 1)}{(int)Rank}";
    }

    public static class ManorExtensions
    {
        // --------------------------------------------------------------------
        // Column / X Conversions (A-E <-> 0-4)
        // --------------------------------------------------------------------
        public static int ToX(this Column column) => (int)column - 1;
        public static Column ToColumn(int x) => (Column)(x + 1);

        // --------------------------------------------------------------------
        // Rank / Z Conversions (1-9 <-> 0-8)
        // --------------------------------------------------------------------
        public static int ToZ(this Rank rank) => (int)rank - 1;
        public static Rank ToRank(int z) => (Rank)(z + 1);

        // --------------------------------------------------------------------
        // Rotation / RotationY Conversions
        // --------------------------------------------------------------------
        public static float ToRotationY(this Rotation rotation) => (int)rotation * 90f;
        public static Rotation ToRotation(float rotationY)
        {
            int steps = Mathf.RoundToInt(rotationY / 90f) % 4;
            if (steps < 0) steps += 4;
            return (Rotation)steps;
        }

        // --------------------------------------------------------------------
        // World Position Helpers
        // --------------------------------------------------------------------
        public static Vector3 GetWorldPosition(Rank rank, Column column, float y = 0f)
        {
            float x = 15f + column.ToX() * 10f;
            float z = 15f + rank.ToZ() * 10f;
            return new Vector3(x, y, z);
        }

        public static ManorPosition FromWorldPosition(Vector3 position)
        {
            int xIndex = Mathf.RoundToInt((position.x - 15f) / 10f);
            int zIndex = Mathf.RoundToInt((position.z - 15f) / 10f);
            return new ManorPosition(ToRank(zIndex), ToColumn(xIndex));
        }

        public static Vector3 GetWorldPosition(this ManorPosition pos, float y = 0f)
        {
            return GetWorldPosition(pos.Rank, pos.Column, y);
        }
    }
}
