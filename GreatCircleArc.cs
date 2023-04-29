using System.Collections.Generic;
using Artitas;
using UnityEngine;
using Xenonauts.Strategy;
using Xenonauts.Strategy.Scripts;
using Xenonauts.Strategy.UI;

namespace Geoshape
{
    public class GreatCircleArc
    {
        public static Dictionary<ulong, GreatCircleArc> Arcs { get; }
            = new Dictionary<ulong, GreatCircleArc>();
        public static bool TryGetArc(Entity entity, out GreatCircleArc arc)
        {
            if (!entity.HasGoal())
            {
                arc = null;
                return false;
            }
            
            ulong combinedID = ((ulong)entity.ID << 32) + (uint)entity.Goal().Value.ID;
            return Arcs.TryGetValue(combinedID, out arc);
        }

        public Vector3 Start { get; }
        public Vector3 End { get; }
        public Vector3 GreatCircle => Vector3.Cross(Start, End);
        private PulseLine[] Lines { get; }

        private readonly PulseLine _prefabLine = StrategyConstants.DEFAULT_PULSE_LINE.Get();

        /// <summary>
        /// Create a great circle arc between two normal vectors with <paramref name="steps"/>
        /// line segments parented to <paramref name="parent"/>. If <paramref name="steps"/> is
        /// equal to 0, no lines are drawn.
        /// </summary>
        public GreatCircleArc(Vector2 start_geoscape, Vector2 end_geoscape,
            ushort steps, PulseLineIconController parent)
        {
            Start = Geometry.GeoscapeToNormal(start_geoscape);
            End = Geometry.GeoscapeToNormal(end_geoscape);
            Lines = new PulseLine[steps];
            
            if (steps > 0 && parent != null)
                Draw(steps, parent);
        }

        /// <summary>
        /// Calculate the start and end point for all <paramref name="steps"/> lines
        /// </summary>
        private void Draw(int steps, PulseLineIconController parent)
        {
            // Calculate the start and end point for every line, can be optimized
            // because each point's end is the next one's start
            for (int i = 0; i < steps - 1; i++)
            {
                float fraction_start = (float)i / (steps - 1);
                float fraction_end = (float)(i + 1) / (steps - 1);

                Vector3 segment_start = (Start * (1 - fraction_start) + End * fraction_start).normalized;
                Vector3 segment_end   = (Start * (1 - fraction_end)   + End * fraction_end).normalized;

                // Draw a line between segment_start and segment_end (in geoscape coordinates)
                PulseLine line = UnityEngine.Object.Instantiate(_prefabLine, parent.transform);
                line.Start = Geometry.NormalToGeoscape(segment_start);
                line.End = Geometry.NormalToGeoscape(segment_end);
                line.RenderCamera = parent.Visualizer.RenderCamera;
                line.gameObject.SetActive(true);
                Lines[i] = line;
            }
        }

        /// <summary>
        /// Return the position <paramref name="distance_km"/> km away when following this 
        /// great circle from the current location <paramref name="position_geoscape"/>
        /// </summary>
        public Vector2 MoveDistanceFrom(Vector2 position_geoscape, float distance_km)
        {
            float angle = Geometry.AngleFromDistance(distance_km);

            Vector3 position = Geometry.GeoscapeToNormal(position_geoscape);
            Vector3 direction = Vector3.Cross(GreatCircle, position);
            Vector3 destination = position * Mathf.Cos(angle) + direction * Mathf.Sin(angle);

            return Geometry.NormalToGeoscape(destination);

            // TODO: update line to remove passed segments
        }

        /// <summary>
        /// Approximate the direction an object at <paramref name="position_geoscape"/> would move in
        /// </summary>
        public Vector2 DirectionAt(Vector2 position_geoscape)
        {
            Vector2 position_close = MoveDistanceFrom(position_geoscape, 1);
            Vector2 difference = position_close - position_geoscape;
            return difference.normalized;
        }
    }
}
