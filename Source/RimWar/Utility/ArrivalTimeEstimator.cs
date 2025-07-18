using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWar.Planet;

namespace RimWar.Utility    
{
    public static class ArrivalTimeEstimator
    {
        public static int EstimatedTicksToArrive(PlanetTile from, PlanetTile to, WarObject warObject)
        {
            // Existing WarObject method
            using (WorldPath worldPath = warObject.pather.curPath ?? GeneratePathForWarObject(from.tileId, to.tileId, warObject))
            {
                if(!worldPath.Found)
                {
                    return 0;
                }
                return CaravanArrivalTimeEstimator.EstimatedTicksToArrive(from.tileId, to.tileId, worldPath, 0, warObject.TicksPerMove, Verse.Find.TickManager.TicksAbs);
            }
        }

        // Add this method for LaunchedWarObjects (including LaunchedWarband)
        public static int EstimatedTicksToArrive(PlanetTile from, PlanetTile to, LaunchedWarObject launchedWarObject)
        {
            // LaunchedWarObjects fly directly, so simple distance calculation
            float distance = Find.WorldGrid.ApproxDistanceInTiles(from.tileId, to.tileId);
            
            // Use the LaunchedWarObject's travel speed (from LaunchedWarObject.TravelSpeed)
            // LaunchedWarObjects travel at 0.00025f tiles per tick
            float travelTimePerTile = 1f / 0.00025f; // 4000 ticks per tile
            
            return Mathf.RoundToInt(distance * travelTimePerTile);
        }

        private static WorldPath GeneratePathForWarObject(int fromTile, int toTile, WarObject warObject)
        {
            // This mirrors what the WarObject does internally for pathfinding
            PlanetLayer layer = PlanetLayer.Selected ?? Find.WorldGrid.Surface;
            PlanetTile startTile = new PlanetTile(fromTile, layer);
            PlanetTile endTile = new PlanetTile(toTile, layer);
            
            using (var pathing = new WorldPathing(layer))
            {
                return pathing.FindPath(startTile, endTile, null);
            }
        }
    }
}
