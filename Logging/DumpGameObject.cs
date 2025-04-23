using System.Text;
using UnityEngine;

namespace Geoshape.Logging
{
    public static class DumpGameObject
    {
        /// <summary>
        /// Print a hierarchy of gameobjects with their components 
        /// </summary>
        public static void DumpToLog(this GameObject obj, int maxDepth = 2)
        {
            StringBuilder output = new StringBuilder();
            DumpGameObjectRecursive(obj, ref output, maxDepth: maxDepth);

            if (output.Length > 10000)
                output.AppendLine("Safety limit of 10000 characters exceeded");

            Debug.Log(output);
        }

        private static void DumpGameObjectRecursive(GameObject obj,
            ref StringBuilder output, int maxDepth, int depth = 0)
        {
            output.AppendLine();
            if (output.Length > 10000) return; // Safety limit

            // Print gameobject name
            string objectPadding = new string(' ', depth * 4);
            output.AppendLine($"{objectPadding}gameobject with name {obj.name}");

            // Print the type of every component in the gameobject, indented one level relative to obj
            string componentPadding = new string(' ', depth * 4 + 4);
            foreach (Component component in obj.GetComponents<Component>())
                output.AppendLine($"{componentPadding}component of type {component.GetType().FullName}");

            // Call this function for every child object until maxdepth is reached
            if (depth >= maxDepth) return;
            for (int i = 0; i < obj.transform.childCount; i++)
                DumpGameObjectRecursive(obj.transform.GetChild(i).gameObject, ref output, maxDepth, depth + 1);
        }
    }
}
