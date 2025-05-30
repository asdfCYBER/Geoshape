﻿using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Xenonauts;

namespace Geoshape
{
    public class Geoshape : IMod
    {
        public static Geoshape Instance { get; private set; }

        private Harmony _harmony;

        public void Initialise()
        {
            if (Instance is null)
            {
                Instance = this;
                Debug.Log("[Geoshape] Geoshape initialised!");
            }
            else
            {
                Debug.Log("[Geoshape] Geoshape.Initialise was called but Geoshape is already initialised!");
            }
            try
            {
                _harmony = new Harmony("mods.asdfcyber.geoshape");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                Debug.Log("[Geoshape] No patching exceptions");
            }
            catch (Exception e)
            {
                Debug.Log($"[Geoshape] exception during patching: {e.GetType()} | {e.Message} | {e.InnerException}");
            }

            // TODO: delay tests until strategy layer is loaded to prevent crashing
            #if DEBUG
            //Tests.GeometryTest.Test();
            #endif
        }
    }
}
