using Geoshape.Logging;
using HarmonyLib;
using UnityEngine;
using Xenonauts.Strategy.UI;


namespace Geoshape.Patches
{
    [HarmonyPatch(typeof(GeoscapeBaseBuildElement), nameof(GeoscapeBaseBuildElement.UpdateBaseIconPosition))]
    internal class GeoscapeUpdateBasePositionPatch
    {
        private static void Postfix(GeoscapeBaseBuildElement __instance, ref GameObject ____radarRootInWorldPos)
        {
            Debug.Log("UpdateBaseIconPosition patch called. Dump of instance:");
            __instance.gameObject.DumpToLog();

            Debug.Log("Dump of _radarRootInWorldPos:");
            ____radarRootInWorldPos.DumpToLog();
        }
    }
}
