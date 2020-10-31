using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using HarmonyLib;
using RimWar.Planet;
using RimWar.RocketTools;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimWar.Harmony
{
    [HarmonyPatch(typeof(WorldFloodFiller), nameof(WorldFloodFiller.FloodFill), new[] { typeof(int), typeof(Predicate<int>), typeof(Func<int, int, bool>), typeof(int), typeof(IEnumerable<int>) })]
    public static class FloodFill_Patch
    {
        public static bool Prefix(WorldFloodFiller __instance, int rootTile, Predicate<int> passCheck, Func<int, int, bool> processor, Queue<int> ___openSet, List<int> ___traversalDistance, List<int> ___visited, int maxTilesToProcess = int.MaxValue, IEnumerable<int> extraRootTiles = null)
        {
            Queue<int> openSet = new Queue<int>();
            List<int> traversalDistance = new List<int>();
            List<int> visited = new List<int>();

            int tilesCount = Find.WorldGrid.TilesCount;
            int num = tilesCount;
            if (traversalDistance.Count != tilesCount)
            {
                traversalDistance.Clear();
                for (int i = 0; i < tilesCount; i++)
                {
                    traversalDistance.Add(-1);
                }
            }
            WorldGrid worldGrid = Find.WorldGrid;
            List<int> tileIDToNeighbors_offsets = worldGrid.tileIDToNeighbors_offsets;
            List<int> tileIDToNeighbors_values = worldGrid.tileIDToNeighbors_values;

            int num2 = 0;
            openSet.Clear();
            if (rootTile != -1)
            {
                visited.Add(rootTile);
                traversalDistance[rootTile] = 0;
                openSet.Enqueue(rootTile);
            }
            if (extraRootTiles != null)
            {
                visited.AddRange(extraRootTiles);
                IList<int> list = extraRootTiles as IList<int>;
                if (list != null)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        int num3 = list[j];
                        traversalDistance[num3] = 0;
                        openSet.Enqueue(num3);
                    }
                }
                else
                {
                    foreach (int extraRootTile in extraRootTiles)
                    {
                        traversalDistance[extraRootTile] = 0;
                        openSet.Enqueue(extraRootTile);
                    }
                }
            }
            while (openSet.Count > 0)
            {
                int num4 = openSet.Dequeue();
                int num5 = traversalDistance[num4];
                if (processor(num4, num5))
                {
                    break;
                }
                num2++;
                if (num2 == maxTilesToProcess)
                {
                    break;
                }
                int num6 = (num4 + 1 < tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_offsets[num4 + 1] : tileIDToNeighbors_values.Count;
                for (int k = tileIDToNeighbors_offsets[num4]; k < num6; k++)
                {
                    int num7 = tileIDToNeighbors_values[k];
                    if (traversalDistance[num7] == -1 && passCheck(num7))
                    {
                        visited.Add(num7);
                        openSet.Enqueue(num7);
                        traversalDistance[num7] = num5 + 1;
                    }
                }
            }

            ___openSet = openSet;
            ___traversalDistance = traversalDistance;
            ___visited = visited;
            return false;
        }

        static Exception Finalizer(Exception __exception)
        {
            if (__exception == null) return null;
            Log.Warning(string.Format("RIMWAR: sync problem led to floodfill double call! with error {0} in {1}", __exception.Message, __exception.StackTrace));
            return null;
        }
    }
}
