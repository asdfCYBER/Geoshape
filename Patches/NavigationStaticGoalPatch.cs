using Artitas;
using HarmonyLib;
using Xenonauts.Strategy.Systems;

namespace Geoshape.Patches
{
    [HarmonyPatch(typeof(GeoscapeNavigationSystem), nameof(GeoscapeNavigationSystem.SteerTowardsStaticGoal))]
    internal class NavigationStaticGoalPatch
    {
        private static void Postfix(ref GeoscapeNavigationSystem.Steer __result, Entity self)
        {
            if (!GreatCircleArc.TryGetArc(self, out GreatCircleArc arc)) 
                return;

            // Calculate the bearing and make the game rotate the aircraft in that direction
            __result = new GeoscapeNavigationSystem.Steer() {
                CurrentVelocity = __result.CurrentVelocity,
                AccelerationChange = __result.AccelerationChange,
                NewDirection = arc.DirectionAt(self.Position())
        };
        }
    }
}
