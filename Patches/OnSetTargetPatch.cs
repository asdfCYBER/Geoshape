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

            ulong combinedID = ((ulong)__instance.Target.ID << 32) + (uint)__instance.Target.Goal().Value.ID;
            if (!GreatCircleArc.Arcs.ContainsKey(combinedID))
            {
                // Clamp the maximum height to just slightly below the north pole
                if (originalLine.End.y >= _northpoleYCoord - 0.1f)
                    originalLine.End = new Vector2(originalLine.End.x, _northpoleYCoord - 0.1f);

                GreatCircleArc.Arcs[combinedID] = new GreatCircleArc(
                    originalLine.Start,
                    originalLine.End,
                    steps: 100,
                    __instance
                );
            }
            
            originalLine.gameObject.SetActive(false);
        }
    }
}
