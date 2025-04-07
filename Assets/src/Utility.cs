using UnityEngine;

namespace Assets.src
{
    internal static class Utility
    {
        public static float AngleTo(Vector2 p1, Vector2 p2) =>
            Mathf.Rad2Deg * Mathf.Atan2(p2.y - p1.y, p2.x - p1.x);
    }
}
