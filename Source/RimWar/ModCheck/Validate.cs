using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace RimWar.ModCheck
{
    public class Validate
    {
        public static bool EmpireIsActive
        {
            get
            {
                //foreach(ModMetaData mods in ModsConfig.ActiveModsInLoadOrder)
                //{
                //    Log.Message("mod id is: " + mods.PackageId);
                //}
                return ModsConfig.IsActive("FactionColonies");
            }
        }
    }
}
