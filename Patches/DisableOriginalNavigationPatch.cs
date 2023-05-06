using HarmonyLib;
using Xenonauts.Strategy.Systems;

namespace Geoshape.Patches
{
    [HarmonyPatch(typeof(GeoscapeNavigationSystem), nameof(GeoscapeNavigationSystem.MoveEntitiesOnGeoscape))]
    internal class DisableOriginalNavigationPatch
    {
        // Skip the method where positions/velocities/rotations are calculated for every movable entity
        private static bool Prefix() => false;
    }
}
