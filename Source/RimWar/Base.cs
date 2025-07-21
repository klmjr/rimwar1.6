using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using UnityEngine;

namespace RimWar
{
    public struct ConsolidatePoints : IExposable
    {
        public int points, delay;

        public ConsolidatePoints(int pts, int dly)
        {
            Mathf.Clamp(pts, 0, pts);
            Mathf.Clamp(dly, 0, dly);
            points = pts;
            delay = dly;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.points, "points", 0, false);
            Scribe_Values.Look<int>(ref this.delay, "delay", 0, false);
        }
    }

    public class Base : ModBase
    {
        public static Base Instance
        {
            get;
            private set;
        }

        // FIXED: Match the packageId from About.xml
        public override string ModIdentifier => "kajtherapper.rimwar16";

        public Base() 
        {
            Instance = this;
        }

        // ADDED: Proper initialization
        public override void DefsLoaded()
        {
            try
            {
                Log.Message("RimWar 1.6: Defs loaded successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"RimWar 1.6: Error during DefsLoaded: {ex}");
            }
        }

        // ADDED: Handle Harmony patches more safely
        public override void WorldLoaded()
        {
            try
            {
                Log.Message("RimWar 1.6: World loaded successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"RimWar 1.6: Error during WorldLoaded: {ex}");
            }
        }
    }
}
