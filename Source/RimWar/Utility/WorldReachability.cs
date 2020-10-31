using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;
using RimWorld;

namespace RimWar.Utility
{
    public static class WorldReachability
    {

        public static bool CanReach(int startTile, int destTile)
        {
            return Find.WorldReachability.CanReach(startTile, destTile);
        }
    }
}
