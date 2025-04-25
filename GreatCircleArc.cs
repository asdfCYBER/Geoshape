using System.Collections.Generic;
using System.Linq;
using Artitas;
using Common.UI.DataStructures;
using UnityEngine;
using Xenonauts.Strategy;
using Xenonauts.Strategy.Scripts;
using Xenonauts.Strategy.UI;

namespace Geoshape
{
    public static class GreatCircleArc
    {
        /// <summary>
        /// Update the start- and endpoint of the great circle arc <paramref name="entity"/>
        /// is currently following, and redraw the lines accordingly. Set <paramref name="targetPosition"/>
        /// if the point the entity should move to is not the same as the entity's goal's position.
        /// </summary>
        public static void Update(Entity entity, Vector3 start, Vector3 end)
        {
            // Calculate the updated positions for all lines
            //if (Lines == null || Lines.Length == 0)
            //return;

            //Debug.Log($"[Geoshape] Components in entity {entity.Name()}:");
            //foreach (var component in entity.GetComponents())
            //{
            //    Debug.Log(component.ToString());
            //}

            var plcs = FindPulseLineControllers(entity);
            Debug.Log($"[Geoshape] {plcs.Count} pulselinecontrollers found");
            foreach (PulseLineIconController plc in plcs)
                Draw(20, plc, start, end);

            /*Vector3 start = Geometry.GeoscapeToNormal(Start);
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
            }*/
        }

        /// <summary>
        /// Calculate the start and end point for all <paramref name="steps"/> lines
        /// </summary>
        private static void Draw(ushort steps, PulseLineIconController plc, Vector3 start, Vector3 end)
        {
            Debug.Log("[Geoshape] Components in plc:");

            if (!plc.Visualizer._lineObj.TryGetComponent(out LineRenderer line))
                line = plc.Visualizer._lineObj.gameObject.AddComponent<LineRenderer>();

            line.positionCount = steps;
            line.startColor = Color.white;
            line.endColor = Color.white;
            line.enabled = true;
            //line.useWorldSpace = true;

            // Calculate the start and end point for every line segment
            Vector3[] positions = new Vector3[steps];
            for (ushort i = 0; i < steps - 1; i++)
            {
                float fraction_start = (float)i / (steps - 1);
                //float fraction_end = (float)(i + 1) / (steps - 1);

                Vector3 segment_start = (start * (1 - fraction_start) + end * fraction_start).normalized;
                //Vector3 segment_end   = (start * (1 - fraction_end)   + end * fraction_end).normalized;
                positions[i] = Geometry.NormalToGeoscape(segment_start);
                positions[i] = plc.Visualizer.RenderCamera.WorldToScreenPoint(positions[i]);

                // Draw a line between segment_start and segment_end (in geoscape coordinates)
                //PulseLine line = UnityEngne.Object.Instantiate(_prefabLine, parent.transform);
                //line.Start = Geometry.NormalToGeoscape(segment_start);
                //line.End = Geometry.NormalToGeoscape(segment_end);
                //line.RenderCamera = parent.Visualizer.RenderCamera;
                //line.gameObject.SetActive(true);
                //Lines[i] = line;
            }
            
            line.SetPositions(positions);
            //parent.Visualizer.gameObject.SetActive(false);  // BUG: dos not work. Problem lies in FindPulseLineController probably
        }

        /// <summary>
        /// Get the <see cref="PulseLineIconController"/>s of <paramref name="entity"/>, if they exist
        /// </summary>
        private static List<PulseLineIconController> FindPulseLineControllers(Entity entity)
        {
            List<PulseLineIconController> plcs = new List<PulseLineIconController>();
            if (!entity.HasGeoscapeIcons())
                return plcs;

            foreach (Entity icon in entity.GeoscapeIcons())
            {
                var plc = icon.UIControllers()
                    .FirstOrDefault(c => c is PulseLineIconController) as PulseLineIconController;
                if (plc != null)
                    plcs.Add(plc);
            }

            return plcs;
        }
    }
}
