using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Xenonauts.Strategy;
using Xenonauts.Strategy.Scripts;
using Xenonauts.Strategy.UI;

namespace Geoshape
{
    public class GreatCircleArc
    {
        public static Dictionary<int, GreatCircleArc> Arcs { get; } = new Dictionary<int, GreatCircleArc>();

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

        public Vector2 MoveAlongArc(Vector2 position_geoscape, float distance)
        {
            float angle = Geometry.AngleFromDistance(distance);

            Vector3 position = Geometry.GeoscapeToNormal(position_geoscape);
            Vector3 direction = Vector3.Cross(GreatCircle, position);
            Vector3 destination = position * Mathf.Cos(angle) + direction * Mathf.Sin(angle);

            return Geometry.NormalToGeoscape(destination);

            // update line
        }
    }
}
