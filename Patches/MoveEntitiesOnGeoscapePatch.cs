using Artitas;
using HarmonyLib;
using UnityEngine;
using Xenonauts.Strategy.Systems;

namespace Geoshape.Patches
{
    [HarmonyPatch(typeof(GeoscapeMovementSystem), nameof(GeoscapeMovementSystem.MoveEntitiesOnGeoscape))]
    internal class MoveEntitiesOnGeoscapePatch
    {
        private static bool Prefix(GeoscapeMovementSystem __instance, PostGeoscapeTickReport report)
        {
            // Get all movables from the private field
            // TODO: cache this
            Family movables = (Family)AccessTools.Field(typeof(GeoscapeMovementSystem), "_movables").GetValue(__instance);
            foreach (Entity movable in movables)
            {
                // Do not perform calculations for entities without goal or registered path
                if (!movable.HasGoal())
                    continue;

                Navigation.MoveEntity(movable, report.after - report.before);
            }

            __instance.World.HandleEvent(new GeoscapeMovementReport());
            return false;  // skip original method
        }
    }
}
