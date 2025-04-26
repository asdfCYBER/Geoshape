using System.Collections.Generic;
using System.Linq;
using Artitas;
using UnityEngine;
using Xenonauts.Strategy;
using Xenonauts.Strategy.Scripts;
using Xenonauts.Strategy.UI;
using Strategy.UI.Components.UI;
using Geoshape.Logging;
using Common.Code.Util;

namespace Geoshape
{
    public static class GreatCircleArc
    {
        //private static PulseLine _prefabLine = StrategyConstants.DEFAULT_PULSE_LINE.Get();

        /// <summary>
        /// Update the start- and endpoint of the great circle arc <paramref name="entity"/>
        /// is currently following, and redraw the lines accordingly. Set <paramref name="targetPosition"/>
        /// if the point the entity should move to is not the same as the entity's goal's position.
        /// </summary>
        public static void Update(Entity entity, Vector3 start, Vector3 end)
        {
            var plcs = FindPulseLineControllers(entity);
            Debug.Log($"[Geoshape] {plcs.Count} pulselinecontrollers found");
            foreach (PulseLineIconController plc in plcs)
                Draw(10, plc, start, end);
        }

        /// <summary>
        /// Calculate the start and end point for all <paramref name="steps"/> lines
        /// </summary>
        private static void Draw(ushort steps, PulseLineIconController plc, Vector3 start, Vector3 end)
        {
            //Debug.Log("PulseLineIconController gameobject dump:");
            //plc.gameObject.DumpToLog(4);

            /*
            gameobject with name Pulse Left line for entity 4190
                component of type UnityEngine.Transform
                component of type Xenonauts.Strategy.UI.PulseLineIconController
                component of type Xenonauts.Strategy.Scripts.IconSortController

                gameobject with name pulse_line(Clone)
                    component of type UnityEngine.RectTransform
                    component of type Xenonauts.Strategy.Scripts.PulseLine

                    gameobject with name Image
                        component of type UnityEngine.RectTransform
                        component of type UnityEngine.CanvasRenderer
                        component of type UnityEngine.UI.Image
            */

            //if (plc?.Visualizer?.gameObject != null)
            //Object.Destroy(plc.Visualizer.gameObject);

            Debug.Log($"[Geoshape] Checking for line obj for {plc.transform.name}");
            GameObject lineObj = plc.transform.Find("line")?.gameObject;
            if (lineObj == null)
            {
                Debug.Log($"[Geoshape] Creating new line");
                lineObj = new GameObject("line");
                Object.DontDestroyOnLoad(lineObj);
                //lineObj = Object.Instantiate(plc.Visualizer.gameObject, plc.transform, true);
                lineObj.name = "line";
                lineObj.transform.SetParent(plc.transform, true);
                //Object.DestroyImmediate(lineObj.GetComponent<PulseLine>());
                lineObj.AddComponent<LineRenderer>();
                lineObj.SetActive(true);
                //Object.Destroy(lineObj.transform.GetChild(0));

                Debug.Log($"[Geoshape] megadump");
                plc.transform.parent.gameObject.DumpToLog(3);
            }
            else
            {
                Debug.Log($"[Geoshape] Line already exists");
            }

            LineRenderer line = lineObj.GetComponent<LineRenderer>();
            line.positionCount = steps;
            line.startColor = Color.white;
            line.endColor = Color.magenta;
            line.startWidth = 200;
            line.endWidth = 2f;
            line.enabled = true;
            line.material = StrategyConstants.DEFAULT_PULSE_LINE.Get()._lineObj.GetComponent<UnityEngine.UI.Image>().material;// new Material(Shader.Find("Unlit/Color"));
            line.material.color = Color.green;
            line.useWorldSpace = true;
            lineObj.SetActive(true);
            //lineObj.transform.position = Vector3.forward;
            lineObj.transform.localPosition = plc.Visualizer.transform.localPosition;
            lineObj.transform.localScale = plc.Visualizer.transform.localScale;

            Debug.Log($"[Geoshape] Current line: start: {plc.Visualizer.Start}, end: {plc.Visualizer.End}\n" +
                $"Start screen position: {plc.Visualizer.RenderCamera.WorldToScreenPoint(plc.Visualizer.Start)}");

            //line.positionCount = 4;
            //line.SetPositions(new Vector3[]{new Vector3(-1920,1080), new Vector3(1920, 1080), new Vector3(1920, -1080)});

            
            // Calculate the start and end point for every line segment
            Vector3[] positions = new Vector3[steps];
            for (ushort i = 0; i < steps; i++)
            {
                float fraction_start = (float)i / (steps - 1);
                //float fraction_end = (float)(i + 1) / (steps - 1);

                Vector3 segment_start = (start * (1 - fraction_start) + end * fraction_start).normalized;
                //Vector3 segment_end   = (start * (1 - fraction_end)   + end * fraction_end).normalized;
                Vector2 position = Geometry.NormalToGeoscape(segment_start);
                if (plc.Screen == GeoscapeScreenComponent.Target.Left)
                    position.x -= StrategyConstants.GEOSCAPE_DIMENSIONS.x;
                else if (plc.Screen == GeoscapeScreenComponent.Target.Right)
                    position.x += StrategyConstants.GEOSCAPE_DIMENSIONS.x;

                //Debug.Log("4-1");
                positions[i] = plc.Visualizer.RenderCamera.WorldToScreenPoint(position);
                positions[i].z = 5;
                Debug.Log($"[Geoshape] point {i}: Geoshape position {position}, screen point {positions[i]}");
                //Debug.Log("4-2");
                // Draw a line between segment_start and segment_end (in geoscape coordinates)
                //PulseLine line = UnityEngine.Object.Instantiate(_prefabLine, parent.transform);
                //line.Start = Geometry.NormalToGeoscape(segment_start);
                //line.End = Geometry.NormalToGeoscape(segment_end);
                //line.RenderCamera = parent.Visualizer.RenderCamera;
                //line.gameObject.SetActive(true);
                //Lines[i] = line;

                /*PulseLine line = UnityEngine.Object.Instantiate(_prefabLine, parent.transform);
                line.Start = Geometry.NormalToGeoscape(segment_start);
                line.End = Geometry.NormalToGeoscape(segment_end);
                line._lineColor = Color.magenta;
                line.RenderCamera = parent.Visualizer.RenderCamera;
                line.gameObject.SetActive(true);*/
            }
            
            line.SetPositions(positions);

            plc.gameObject.DumpToLog(4);
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
