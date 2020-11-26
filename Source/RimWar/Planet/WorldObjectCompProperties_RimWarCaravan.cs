using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using RimWar;

namespace RimWar.Planet
{
    public class WorldObjectCompProperties_RimWarCaravan : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_RimWarCaravan()
        {
            compClass = typeof(RimWarCaravanComp);
        }
    }
}
