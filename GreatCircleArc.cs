using System.Collections.Generic;
using System.Linq;
using Artitas;
using Strategy.UI.Components.UI;
using UnityEngine;
using Xenonauts.Strategy;
using Xenonauts.Strategy.Scripts;
using Xenonauts.Strategy.UI;
using Geoshape.Logging;

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
            var plcs = FindPulseLineControllers(entity);
            foreach (PulseLineIconController plc in plcs)
                Draw(100, plc, start, end);
        }

        /// <summary>
        /// Calculate the start and end point for all <paramref name="steps"/> lines
        /// </summary>
        private static void Draw(ushort steps, PulseLineIconController plc, Vector3 start, Vector3 end)
        {
            GameObject lineObj = plc.transform.Find("line")?.gameObject;
            LineRenderer line;
            if (lineObj == null)
            {
                Debug.Log($"[Geoshape] Creating new line for {plc.transform.name}");
                lineObj = new GameObject("line");
                lineObj.name = "line";
                lineObj.transform.SetParent(plc.transform, true);
                lineObj.transform.localPosition = plc.Visualizer.transform.localPosition;
                lineObj.transform.localScale = plc.Visualizer.transform.localScale;
                lineObj.SetActive(true);
                line = lineObj.AddComponent<LineRenderer>();
                line.startWidth = 2f;
                line.endWidth = 2f;
                line.material = new Material(Shader.Find("Unlit/Color"));
                line.material.color = Color.yellow;
                line.useWorldSpace = true;
                plc.Visualizer._lineObj.gameObject.SetActive(false);
            }
            else
            {
                line = lineObj.GetComponent<LineRenderer>();
            }
            
            // Calculate the start and end point for every line segment
            Vector3[] positions = new Vector3[steps];
            line.positionCount = steps;
            for (ushort i = 0; i < steps; i++)
            {
                float fraction = (float)i / (steps - 1);

                Vector3 position_normal = (start * (1 - fraction) + end * fraction).normalized;
                Vector3 position = Geometry.NormalToGeoscape(position_normal);
                if (plc.Screen == GeoscapeScreenComponent.Target.Left)
                    position.x -= StrategyConstants.GEOSCAPE_DIMENSIONS.x;
                else if (plc.Screen == GeoscapeScreenComponent.Target.Right)
                    position.x += StrategyConstants.GEOSCAPE_DIMENSIONS.x;
                position.z = -4;
                positions[i] = position;
            }

            line.SetPositions(positions);
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
