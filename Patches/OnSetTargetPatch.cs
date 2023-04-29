using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;
using Xenonauts.Strategy.Scripts;
using Xenonauts.Strategy.UI;

namespace Geoshape.Patches
{
    [HarmonyPatch(typeof(PulseLineIconController), "OnSetTarget")]
    internal static class OnSetTargetPatch
    {
        private static readonly float _northpoleYCoord = Geometry.GCSToGeoscape(new Vector2(90, 0)).y;

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Harmony")]
        private static void Postfix(PulseLineIconController __instance)
        {
            PulseLine originalLine = __instance.Visualizer;

            long combinedID = __instance.Target.ID << 16 + __instance.Target.Goal().Value.ID;
            if (!GreatCircleArc.Arcs.ContainsKey(combinedID))
            {
                if (originalLine.End.y >= _northpoleYCoord - 0.1f)
                    originalLine.End = new Vector2(originalLine.End.x, _northpoleYCoord - 0.1f);

                GreatCircleArc.Arcs[combinedID] = new GreatCircleArc(
                    Geometry.GeoscapeToNormal(originalLine.Start),
                    Geometry.GeoscapeToNormal(originalLine.End),
                    steps: 100,
                    __instance
                );
            }
            
            originalLine.gameObject.SetActive(false);
        }
    }
}
