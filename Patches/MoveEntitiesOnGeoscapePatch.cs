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
            Family movables = (Family)AccessTools.Field(typeof(GeoscapeMovementSystem), "_movables").GetValue(__instance);
            foreach (Entity movable in movables)
            {
                // Do not perform calculations for entities without goal or registered path
                if (!movable.HasGoal()) continue;
                if (!GreatCircleArc.TryGetArc(movable, out GreatCircleArc arc))
                {
                    Debug.Log($"[Geoshape] No great circle arc found for entity {movable.Name()} (ID: {movable.ID})");
                    continue;
                }
                
                // Get the elapsed time, multiply by the speed and move that much distance along the great circle
                float hours = (float)(report.after - report.before).TotalHours;
                float distance_km = AircraftSystem.ToKPH(movable.Speed()) * hours;
                Vector2 newPosition = arc.MoveDistanceFrom(movable.Position(), distance_km);
                movable.AddPosition(newPosition);
            }

            __instance.World.HandleEvent(new GeoscapeMovementReport());
            return false;  // skip original method
        }
    }
}
