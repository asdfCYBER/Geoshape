using HarmonyLib;
using UnityEngine;
using Xenonauts.Strategy.Systems;

namespace Geoshape.Patches
{
    //[HarmonyPatch(typeof(GeoscapeNavigationSystem), nameof(GeoscapeNavigationSystem.DistanceToTarget))]
    internal class DistanceToTargetPatch
    {
        private static bool Prefix(Vector3 selfPosition, Vector3 targetPosition, ref Vector3 __result)
        {
            Vector3 selfNormal = Geometry.GeoscapeToNormal(selfPosition);
            Vector3 targetNormal = Geometry.GeoscapeToNormal(targetPosition);
            __result = Vector3.forward * Geometry.DistanceBetweenPoints(selfNormal, targetNormal);
            return false;  // skip original method
        }
    }
}
