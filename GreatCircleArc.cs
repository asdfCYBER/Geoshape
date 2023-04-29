using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Artitas;
using Xenonauts.Strategy;
using Xenonauts.Strategy.Scripts;
using Xenonauts.Strategy.UI;

namespace Geoshape
{
    public class GreatCircleArc
    {
        public static Dictionary<long, GreatCircleArc> Arcs { get; } = new Dictionary<long, GreatCircleArc>();
        public static bool TryGetArc(Entity entity, out GreatCircleArc arc)
        {
            if (!entity.HasGoal())
            {
                arc = null;
                return false;
            }
            
            long combinedID = entity.ID << 16 + entity.Goal().Value.ID;
            return Arcs.TryGetValue(combinedID, out arc);
        }

        public Vector3 Start { get; }
        public Vector3 End { get; }
        public Vector3 GreatCircle => Vector3.Cross(Start, End);
        private PulseLine[] Lines { get; }

        private readonly PulseLine _prefabLine = StrategyConstants.DEFAULT_PULSE_LINE.Get();

        /// <summary>
        /// Create a great circle arc between two normal vectors with <paramref name="steps"/>
        /// line segments parented to <paramref name="parent"/>.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="steps"></param>
        public GreatCircleArc(Vector3 start, Vector3 end, int steps, PulseLineIconController parent)
        {
            Start = start;
            End = end;
            Lines = new PulseLine[steps];

            for (int i = 0; i < steps - 1; i++)
            {
                float fraction_start = (float)i / (steps - 1);
                float fraction_end = (float)(i + 1) / (steps - 1);

                Vector3 segment_start = (start * (1 - fraction_start) + end * fraction_start).normalized;
                Vector3 segment_end = (start * (1 - fraction_end) + end * fraction_end).normalized;

                PulseLine line = UnityEngine.Object.Instantiate(_prefabLine, parent.transform);
                line.Start = Geometry.NormalToGeoscape(segment_start);
                line.End = Geometry.NormalToGeoscape(segment_end);
                line.RenderCamera = parent.Visualizer.RenderCamera;
                line.gameObject.SetActive(true);
                Lines[i] = line;
            }
        }

        /// <summary>
        /// Return the position <paramref name="distance"/> km away when following this 
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
        /// Calculate the angle between the direction vector and the vector pointing north
        /// </summary>
        public Vector2 BearingAt(Vector2 position_geoscape)
        {
            Vector3 north = new Vector3(0, 0, 1);
            Vector3 position = Geometry.GeoscapeToNormal(position_geoscape);
            Vector3 greatCircleThroughNorth = Vector3.Cross(position, north);
            Vector3 cross = Vector3.Cross(GreatCircle, greatCircleThroughNorth);
            float sin_theta = cross.Norm() * Mathf.Sign(Vector3.Dot(cross, position));
            float cos_theta = Vector3.Dot(GreatCircle, greatCircleThroughNorth);
            return new Vector2(sin_theta, cos_theta);
        }
    }
}
