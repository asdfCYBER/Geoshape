using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Geoshape.Tests
{
    internal static class GeometryTest
    {
        public static void Test()
        {
            Vector2 start = new Vector2(0, 0);
            Vector3 start_n = Geometry.GCSToNormal(start);
            Vector2 end = new Vector2(0, 90);
            Vector3 end_n = Geometry.GCSToNormal(end);

            Vector3 newpos_n = Navigation.TowardsTargetDistance(start_n, end_n, Geometry.Radius * Mathf.PI / 4f); // move halfway
            Vector2 newpos = Geometry.NormalToGCS(newpos_n);

            float distance = Geometry.DistanceBetweenPoints(start_n, newpos_n);

            // Note: wanted distance only correct for 0 latitude
            Debug.Log($"[Geoshape] test: arc from {start} to {end} (normal: from {start_n:F6} to {end_n:F6}). " +
                $"Newpos is {newpos:F6} (normal: {newpos_n:F6}). Wanted distance: {Geometry.Radius*Mathf.PI/4f}, actual distance: {distance}");
        }
    }
}
