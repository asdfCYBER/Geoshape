using System.Collections.Generic;
using Artitas;
using Common.UI.DataStructures;
using UnityEngine;
using Xenonauts.Strategy;
using Xenonauts.Strategy.Scripts;
using Xenonauts.Strategy.UI;

namespace Geoshape
{
    public class GreatCircleArc
    {
        public static Dictionary<int, GreatCircleArc> Arcs { get; }
            = new Dictionary<int, GreatCircleArc>();

        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }
        private PulseLine[] Lines { get; }

        private static readonly PulseLine _prefabLine = StrategyConstants.DEFAULT_PULSE_LINE.Get();
        private static readonly float _northpoleYCoord = Geometry.GCSToGeoscape(new Vector2(90, 0)).y;

        /// <summary>
        /// Create a great circle arc between two normal vectors with <paramref name="steps"/>
        /// line segments parented to <paramref name="parent"/>. If <paramref name="steps"/> is
        /// equal to 0, no lines are drawn. Disable the original line.
        /// </summary>
        public GreatCircleArc(Vector2 start_geoscape, Vector2 end_geoscape,
            ushort steps, PulseLineIconController parent)
        {
            // Ensure the coordinates are valid
            if (start_geoscape.y > _northpoleYCoord)
                start_geoscape.y = _northpoleYCoord;
            if (end_geoscape.y > _northpoleYCoord)
                end_geoscape.y = _northpoleYCoord;

            Start = start_geoscape;
            End = end_geoscape;

            if (steps > 0 && parent != null)
            {
                Lines = new PulseLine[steps];
                Draw(steps, parent);
                parent.Visualizer?.gameObject?.SetActive(false);
            }
        }

        /// <summary>
        /// Return the great circle arc that <paramref name="entity"/> is following
        /// </summary>
        public static GreatCircleArc GetArc(Entity entity)
        {
            Vector2 targetPosition = entity.Goal().Value.Position();
            return GetArc(entity, targetPosition);
        }

        /// <summary>
        /// Return the great circle arc that <paramref name="entity"/> is following
        /// </summary>
        public static GreatCircleArc GetArc(Entity entity, Vector2 targetPosition)
        {
            if (Arcs.TryGetValue(entity.ID, out GreatCircleArc arc))
            {
                // Update the arc if the target position has changed
                if (arc.End != targetPosition)
                    arc.Update(entity, targetPosition);

                return arc;
            }
            else
            {
                // No arc exists yet, so create a new one
                PulseLineIconController parent = FindPulseLineController(entity);
                Arcs[entity.ID] = new GreatCircleArc(entity.Position(), targetPosition, steps: 100, parent);
                return Arcs[entity.ID];
            }
        }

        /// <summary>
        /// Update the start- and endpoint of the great circle arc <paramref name="entity"/>
        /// is currently following, and redraw the lines accordingly. Set <paramref name="targetPosition"/>
        /// if the point the entity should move to is not the same as the entity's goal's position.
        /// </summary>
        public void Update(Entity entity, Vector2? targetPosition = null)
        {
            Start = entity.Position();
            End = targetPosition ?? entity.Goal().Value.Position();

            // Calculate the updated positions for all lines
            if (Lines == null || Lines.Length == 0)
                return;

            Vector3 start = Geometry.GeoscapeToNormal(Start);
            Vector3 end = Geometry.GeoscapeToNormal(End);

            for (ushort i = 0; i < Lines.Length; i++)
            {
                float fraction_start = (float)i / (Lines.Length - 1);
                float fraction_end = (float)(i + 1) / (Lines.Length - 1);
                
                Vector3 segment_start = (start * (1 - fraction_start) + end * fraction_start).normalized;
                Vector3 segment_end   = (start * (1 - fraction_end)   + end * fraction_end).normalized;

                // Update the lines
                if (Lines[i] != null)
                {
                    Lines[i].Start = Geometry.NormalToGeoscape(segment_start);
                    Lines[i].End = Geometry.NormalToGeoscape(segment_end);
                }
            }
        }

        /// <summary>
        /// Calculate the start and end point for all <paramref name="steps"/> lines
        /// </summary>
        private void Draw(ushort steps, PulseLineIconController parent)
        {
            Vector3 start = Geometry.GeoscapeToNormal(Start);
            Vector3 end = Geometry.GeoscapeToNormal(End);

            // Calculate the start and end point for every line, can be optimized
            // because each point's end is the next one's start
            for (ushort i = 0; i < steps - 1; i++)
            {
                float fraction_start = (float)i / (steps - 1);
                float fraction_end = (float)(i + 1) / (steps - 1);

                Vector3 segment_start = (start * (1 - fraction_start) + end * fraction_start).normalized;
                Vector3 segment_end   = (start * (1 - fraction_end)   + end * fraction_end).normalized;

                // Draw a line between segment_start and segment_end (in geoscape coordinates)
                PulseLine line = UnityEngine.Object.Instantiate(_prefabLine, parent.transform);
                line.Start = Geometry.NormalToGeoscape(segment_start);
                line.End = Geometry.NormalToGeoscape(segment_end);
                line.RenderCamera = parent.Visualizer.RenderCamera;
                line.gameObject.SetActive(true);
                Lines[i] = line;
            }

            parent.Visualizer.gameObject.SetActive(false);  // BUG: dos not work. Problem lies in FindPulseLineController probably
        }

        /// <summary>
        /// Return the position <paramref name="distance_km"/> km away when following this 
        /// great circle from the current location <paramref name="position_geoscape"/>
        /// </summary>
        public Vector2 MoveDistanceFrom(Vector2 position_geoscape, float distance_km)
        {
            Vector3 position = Geometry.GeoscapeToNormal(position_geoscape);
            Vector3 destination = MoveDistanceFrom(position, distance_km);
            return Geometry.NormalToGeoscape(destination);

            // TODO: update line to remove passed segments
        }

        /// <summary>
        /// Return the position <paramref name="distance_km"/> km away when following this 
        /// great circle from the location with normal vector <paramref name="position"/> 
        /// </summary>
        public Vector3 MoveDistanceFrom(Vector3 position, float distance_km)
        {
            float angle = Geometry.AngleFromDistance(distance_km);
            Vector3 start = Geometry.GeoscapeToNormal(Start);
            Vector3 end   = Geometry.GeoscapeToNormal(End);
            Vector3 greatCircle = Vector3.Cross(start, end);
            Vector3 direction = Vector3.Cross(greatCircle, position);
            return position * Mathf.Cos(angle) + direction * Mathf.Sin(angle);
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

        /// <summary>
        /// Get the <see cref="PulseLineIconController"/> of <paramref name="entity"/>, if it exists
        /// </summary>
        private static PulseLineIconController FindPulseLineController(Entity entity)
        {
            if (!entity.HasGeoscapeIcons())
                return null;

            foreach (Entity icon in entity.GeoscapeIcons())
            {
                if (!icon.HasUIControllers())
                    continue;

                foreach (IUIController controller in icon.UIControllers())
                    if (controller is PulseLineIconController pulselineController)
                        return pulselineController;
            }

            return null;
        }
    }
}
