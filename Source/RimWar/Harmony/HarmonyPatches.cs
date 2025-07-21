using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.AI;
using System.Reflection.Emit;
using RimWar.Planet;
using RimWar.Utility;
using RimWar;
using RimWorld.BaseGen;


namespace RimWar.Harmony
{
    [StaticConstructorOnStartup]
    public static class RimWarHarmonyPatches
    {
        static RimWarHarmonyPatches()
        {
            // REMOVE ALL THIS MANUAL PATCHING - let HugsLib handle it automatically
            // var harmonyInstance = new HarmonyLib.Harmony("rimworld.torann.rimwar");
            // try { ... } catch { ... }
            
            // KEEP ONLY NON-HARMONY INITIALIZATION:
            Log.Message("RimWar: Harmony patches initialized");
        }
        
        // Keep all your existing patch methods as-is
        public static void PodsArrived_SettlementHasAttackers_Postfix(List<ActiveTransporterInfo> pods, int tile, Settlement ___settlement)
        {
            if(___settlement != null && ___settlement.HasMap)
            {
                Map map = ___settlement.Map;
                RimWarSettlementComp rwsc = ___settlement.GetComponent<RimWarSettlementComp>();
                if(rwsc != null)
                {
                    if (rwsc.PointDamage > 0)
                    {
                        int sdmg = rwsc.PointDamage;
                        IEnumerable<Pawn> list = from p in map.mapPawns.AllPawnsSpawned
                                                 where (p.Faction != null && p.Faction == rwsc.parent.Faction)
                                                 select p;
                        if (list != null)
                        {
                            while (sdmg > 0)
                            {
                                float ptDam = Mathf.Clamp(Rand.Range(2f, 10f), 0, sdmg);
                                sdmg -= Mathf.RoundToInt(ptDam * 2f);
                                DamageInfo dinfo = new DamageInfo(RimWarDefOf.RW_CombatInjury, ptDam);
                                try { list.RandomElement().TakeDamage(dinfo); } catch { }
                            }
                        }
                    }
                    if (rwsc.UnderAttack)
                    {
                        //Log.Message("generate settlement attackers");
                        IncidentUtility.GenerateSettlementAttackers(rwsc, map);
                    }
                }
            }
            //else
            //{
            //    Log.Message("no map found");
            //}
        }

        public static void AttackInjuredSettlement_Postfix(Caravan caravan, Map map, Func<Pawn,IntVec3> spawnCellGetter, CaravanDropInventoryMode dropInventoryMode, bool draftColonists)
        {
            WorldObject wo = Find.WorldObjects.WorldObjectAt(map.Tile, WorldObjectDefOf.Settlement);
            if (wo != null)
            {
                RimWarSettlementComp rwsc = wo.GetComponent<RimWarSettlementComp>();
                if (rwsc != null)
                {
                    if (rwsc.PointDamage > 0)
                    {
                        int sdmg = rwsc.PointDamage;
                        IEnumerable<Pawn> list = from p in map.mapPawns.AllPawnsSpawned
                                                 where (p.Faction != null && p.Faction == wo.Faction)
                                                 select p;
                        if (list != null)
                        {
                            while (sdmg > 0)
                            {
                                float ptDam = Mathf.Clamp(Rand.Range(2f, 10f), 0, sdmg);
                                sdmg -= Mathf.RoundToInt(ptDam * 2f);
                                DamageInfo dinfo = new DamageInfo(RimWarDefOf.RW_CombatInjury, ptDam);
                                try { list.RandomElement().TakeDamage(dinfo); } catch { }
                            }
                        }
                    }
                    if(rwsc.UnderAttack)
                    {
                        //Log.Message("generate settlement attackers");
                        IncidentUtility.GenerateSettlementAttackers(rwsc, map);
                    }
                }
            }
        }

        private static int settlementGenPoints = 0;
        public static bool GenStep_Map_ID_Prefix(IntVec3 c, Map map, GenStepParams parms)
        {
            if(map != null)
            {
                WorldObject wo = Find.WorldObjects.WorldObjectAt(map.Tile, WorldObjectDefOf.Settlement);
                if (wo != null)
                {
                    RimWarSettlementComp rwsc = wo.GetComponent<RimWarSettlementComp>();
                    if(rwsc != null)
                    {
                        settlementGenPoints = rwsc.RimWarPoints;
                    }
                }

            }
            return true;
        }

        public static bool GenStep_Map_Params_Prefix(string symbol, ref ResolveParams resolveParams, string customNameForPath)
        {
            if(resolveParams.pawnGroupMakerParams != null)
            {
                if (settlementGenPoints != 0)
                {
                    resolveParams.pawnGroupMakerParams.points = Mathf.Clamp(settlementGenPoints, 0, 20000);
                }
            }
            return true;            
        }

        public static void WorldCapitolOverlay()
        {
            List<Settlement> sList = Find.WorldObjects.Settlements;
            float averageTileSize = Find.WorldGrid.AverageTileSize;
            float transitionPct = ExpandableWorldObjectsUtility.RawTransitionPct;
            float num = (Find.WorldCameraDriver.altitude / 100f) -.75f;
            foreach (Settlement wos in sList)
            {
                RimWarSettlementComp rwsc = wos.GetComponent<RimWarSettlementComp>();
                if(rwsc != null)
                {
                    if (rwsc.isCapitol)
                    {
                        Vector3 dPos = wos.DrawPos;
                        if (transitionPct > 0f)
                        {
                            dPos.x += -.15f;
                            dPos.y += .25f;

                            WorldRendererUtility.DrawQuadTangentialToPlanet(dPos, num, .015f, RimWarMatPool.Material_CapitolStar_se);
                        }
                        else
                        {
                            dPos.x += -.1f;
                            dPos.y += .2f;
                            WorldRendererUtility.DrawQuadTangentialToPlanet(dPos, 0.7f * averageTileSize, 0.015f, RimWarMatPool.Material_CapitolStar_se);
                        }
                    }
                    if(rwsc.UnderAttack)
                    {
                        Vector3 dPos = wos.DrawPos;
                        if (transitionPct > 0f)
                        {
                            dPos.x += .5f;
                            dPos.y += .35f;

                            WorldRendererUtility.DrawQuadTangentialToPlanet(dPos, num * .6f, .015f, RimWarMatPool.Material_BattleSite);
                        }
                        else
                        {
                            dPos.x += .25f;
                            dPos.y += .25f;
                            WorldRendererUtility.DrawQuadTangentialToPlanet(dPos, 0.5f * averageTileSize, 0.015f, RimWarMatPool.Material_BattleSite);
                        }
                    }
                    //WorldRendererUtility.DrawQuadTangentialToPlanet(dPos, averageTileSize, 0.015f, RimWarMatPool.Material_CapitolStar_se);

                }
            }
        }

        public static IEnumerable<CodeInstruction> RimWar_CommsConsoleOptions_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Call && list[i].operand.ToString().Contains("Military"))
                {
                    list.Remove(list[i]);
                    list.InsertRange(i, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Call, (object)typeof(FactionDialogReMaker).GetMethod("RequestMilitaryAid_Scouts_Option"))
                        //new CodeInstruction(OpCodes.Ldloc_1),
                        //new CodeInstruction(OpCodes.Ldarg_1),
                        //new CodeInstruction(OpCodes.Ldloc_0),
                        //new CodeInstruction(OpCodes.Ldfld, (object)typeof(FactionDialogMaker).GetField("negotiator")),
                        //new CodeInstruction(OpCodes.Ldc_I4_1),
                        //new CodeInstruction(OpCodes.Ldloca_S),
                        //new CodeInstruction(OpCodes.Call, (object)typeof(FactionDialogReMaker).GetMethod("RequestMilitaryAid_Warband_Option"))
                    });

                    //int num = 1;
                    //list.InsertRange(i + num, new List<CodeInstruction>
                    //{
                    //    )
                    //});
                    //break;
                }
            }
            return list.AsEnumerable();
        }

        [HarmonyPatch(typeof(Faction), "RelationWith")]
        public static class FactionRelationCheck_Patch
        {
            private static bool Prefix(Faction __instance, List<FactionRelation> ___relations, Faction other, ref FactionRelation __result, bool allowNull = false)
            {
                if (other == __instance)
                {
                    return true;
                }
                for (int i = 0; i < ___relations.Count; i++)
                {
                    if (___relations[i].other == other)
                    {
                        __result = ___relations[i];
                        return false;
                    }
                }
                if (!allowNull)
                {
                    WorldUtility.CreateFactionRelation(__instance, other);
                    //Log.Message("forced faction relation between " + __instance.Name + " and " + other.Name);
                }
                __result = null;
                return false;
            }
        }

        //[HarmonyPatch(typeof(WorldObject), "Destroy")]
        //public static class SettlementDestroyed_Patch
        //{
        //    private static void Postfix(WorldObject __instance)
        //    {
        //        if (__instance is RimWorld.Planet.Settlement)
        //        {
        //            RimWarSettlementComp rwsc = __instance.GetComponent<RimWarSettlementComp>();
        //            if (rwsc != null && rwsc.isCapitol)
        //            {
        //                for (int i = 0; i < Find.WorldObjects.AllWorldObjects.Count; i++)
        //                {
        //                    WorldObject wo = Find.WorldObjects.AllWorldObjects[i];
        //                    if (wo is CapitolBuilding && wo.Tile == __instance.Tile)
        //                    {
        //                        wo.Destroy();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(FactionManager), "Remove")]
        public static class RemoveFaction_Patch
        {
            private static void Postfix(FactionManager __instance, Faction faction)
            {
                RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
                if (rwd != null)
                {
                    WorldUtility.RemoveRWDFaction(rwd);
                }
            }
        }

        public static void Pather_StartPath_WarObjects(Caravan_PathFollower __instance, Caravan ___caravan, int destTile, CaravanArrivalAction arrivalAction, ref bool __result, bool repathImmediately = false, bool resetPauseStatus = true)
        {
            if (__result == true)
            {
                if (arrivalAction is RimWar.Planet.CaravanArrivalAction_AttackWarObject)
                {
                    //Log.Message("assigning war object action: attack");
                    CaravanArrivalAction_AttackWarObject woAction = arrivalAction as CaravanArrivalAction_AttackWarObject;
                    woAction.wo.interactable = true;
                    RimWar.Planet.WorldUtility.Get_WCPT().AssignCaravanTargets(___caravan, woAction.wo);
                }
                else if (arrivalAction is RimWar.Planet.CaravanArrivalAction_EngageWarObject)
                {
                    //Log.Message("assigning war object action: engage");
                    CaravanArrivalAction_EngageWarObject woAction = arrivalAction as CaravanArrivalAction_EngageWarObject;
                    woAction.wo.interactable = true;
                    RimWar.Planet.WorldUtility.Get_WCPT().AssignCaravanTargets(___caravan, woAction.wo);
                }
                else
                {
                    WorldUtility.Get_WCPT().RemoveCaravanTarget(___caravan);
                }
            }
        }

        public static void AttackNow_SettlementReinforcement_Postfix(RimWorld.Planet.SettlementUtility __instance, Caravan caravan, RimWorld.Planet.Settlement settlement)
        {
            RimWarSettlementComp rwsc = settlement.GetComponent<RimWarSettlementComp>();
            if (rwsc != null && rwsc.ReinforcementPoints > 0)
            {
                //WorldUtility.CreateWarband((rwsc.ReinforcementPoints), WorldUtility.GetRimWarDataForFaction(rwsc.parent.Faction), settlement, settlement.Tile, settlement, WorldObjectDefOf.Settlement);  
            }
        }

        public static bool CallForAid_Replacement_Patch(Map map, Faction faction)
        {
            Faction ofPlayer = Faction.OfPlayer;
            int goodwillChange = -25;
            bool canSendMessage = false;
            string reason = "GoodwillChangedReason_RequestedMilitaryAid".Translate();
            faction.TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage, true, HistoryEventDefOf.RequestedMilitaryAid);
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = map;
            incidentParms.faction = faction;
            incidentParms.raidArrivalModeForQuickMilitaryAid = true;
            incidentParms.points = DiplomacyTuning.RequestedMilitaryAidPointsRange.RandomInRange;
            faction.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
            RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
            RimWarSettlementComp rwdTown = rwd.WorldSettlements.RandomElement().GetComponent<RimWarSettlementComp>();
            if (rwdTown != null)
            {
                int pts = Mathf.RoundToInt(rwdTown.RimWarPoints / 2);
                if (rwd.CanLaunch)
                {
                    WorldUtility.CreateLaunchedWarband(pts, rwd, rwdTown.parent as RimWorld.Planet.Settlement, rwdTown.parent.Tile, Find.WorldObjects.SettlementAt(map.Tile), WorldObjectDefOf.Settlement);
                }
                else
                {
                    WorldUtility.CreateWarband(pts, rwd, rwdTown.parent as RimWorld.Planet.Settlement, rwdTown.parent.Tile, Find.WorldObjects.SettlementAt(map.Tile), WorldObjectDefOf.Settlement);
                }
                rwdTown.RimWarPoints = pts;
                return false;
            }
            return true;
        }

        public static bool IncidentQueueAdd_Replacement_Prefix(IncidentQueue __instance, IncidentDef def, int fireTick, IncidentParms parms = null, int retryDurationTicks = 0)
        {
            if (def == IncidentDefOf.TraderCaravanArrival && fireTick == (Find.TickManager.TicksGame + 120000))
            {
                RimWarSettlementComp rwdTown = WorldUtility.GetClosestSettlementOfFaction(parms.faction, parms.target.Tile, 100);
                if (rwdTown != null)
                {
                    WorldUtility.CreateTrader(Mathf.RoundToInt(rwdTown.RimWarPoints / 2), WorldUtility.GetRimWarDataForFaction(rwdTown.parent.Faction), rwdTown.parent as RimWorld.Planet.Settlement, rwdTown.parent.Tile, Find.WorldObjects.SettlementAt(parms.target.Tile), WorldObjectDefOf.Settlement);
                    rwdTown.RimWarPoints = Mathf.RoundToInt(rwdTown.RimWarPoints / 2);
                    return false;
                }
            }
            return true;
        }

        public static bool TryAffectGoodwillWith_Reduction_Prefix(Faction __instance, Faction other, ref int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, HistoryEventDef reason = null, GlobalTargetInfo? lookTarget = default(GlobalTargetInfo?))
        {
            //if((__instance.IsPlayer || other.IsPlayer))
            //{
            //    if (reason == null || (reason != null && reason != "Rim War"))
            //    {
            //        goodwillChange = Mathf.RoundToInt(goodwillChange / 5);
            //    }
            //}
            return true;
        }

        public static bool TryResolveParms_Points_Prefix(IncidentParms parms)
        {
            return true;
            if (parms.points <= 1000)
            {
                return true;
            }
            return false;
        }

        public static bool Caravan_Give_Prefix(Caravan caravan, List<ThingCount> demands, List<Pawn> attackers)
        {
            List<WarObject> warObject = WorldUtility.GetHostileWarObjectsInRange(caravan.Tile, 1, caravan.Faction);
            //Log.Message("checking action give");
            if (warObject != null && warObject.Count > 0 && attackers != null && attackers.Count > 0)
            {
                //Log.Message("found " + warObject.Count + " warObjects");
                for (int i = 0; i < warObject.Count; i++)
                {
                    if (warObject[i].Faction != null)//&& warObject[i].Faction == attackers[0].Faction)
                    {
                        float marketValue = 0;
                        for (int j = 0; j < demands.Count; j++)
                        {
                            marketValue += (demands[j].Thing.MarketValue * demands[j].Count);
                        }
                        //Log.Message("market value of caravan ransom is " + marketValue);
                        int points = warObject[i].RimWarPoints + Mathf.RoundToInt(marketValue / 20);
                        //if (warObject[i].ParentSettlement != null)
                        //{
                        //    ConsolidatePoints reconstitute = new ConsolidatePoints(points, Mathf.RoundToInt(Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, warObject[i].ParentSettlement.Tile) * warObject[i].TicksPerMove) + Find.TickManager.TicksGame);
                        //    warObject[i].WarSettlementComp.SettlementPointGains.Add(reconstitute);
                        //    warObject[i].ImmediateAction(null);
                        //}
                        warObject[i].interactable = false;
                        break;
                    }
                }
            }

            return true;
        }

        public static void Caravan_MoveOn_Prefix(Caravan caravan)
        {
            //Log.Message("moving on...");
            //List<CaravanTargetData> ctd = WorldUtility.Get_WCPT().caravanTargetData;
            //if (ctd != null && ctd.Count > 0)
            //{
            //    Log.Message("1");
            //    for (int i = 0; i < ctd.Count; i++)
            //    {
            //        Log.Message("ctd " + i + " " + ctd[i].caravanTarget.Name);
            //        if (Find.WorldGrid.ApproxDistanceInTiles(caravan.Tile, ctd[i].CaravanTile) <= 2)
            //        {
            //            //ctd[i].shouldRegenerateCaravanTarget = true;
            //            //ctd[i].rwo = ctd[i].
            //        }
            //    }
            //}
        }

        [HarmonyPriority(10000)] // be sure to patch before other mod so def is not null
        public static bool IncidentWorker_Prefix(IncidentWorker __instance, IncidentParms parms, ref bool __result)
        {
            //Log.Message("def " + __instance.def);
            if (__instance.def == null)
            {
                Traverse.Create(root: __instance).Field(name: "def").SetValue(IncidentDefOf.RaidEnemy);
                __instance.def = IncidentDefOf.RaidEnemy;
            }
            //Log.Message("def tale " + __instance.def.tale);
            //Log.Message("def category tale " + __instance.def.category.tale);
            return true;
        }

        //private static void DrawFactionRow_WithFactionPoints_Postfix(Faction faction, float rowY, Rect fillRect, ref float __result)
        //{
        //    if (!Prefs.DevMode)
        //    {
        //        Rect rect = new Rect(35f, rowY + __result, 250f, 80f);
        //        StringBuilder stringBuilder = new StringBuilder();
        //        string text = stringBuilder.ToString();
        //        float width = fillRect.width - rect.xMax;
        //        float num = Text.CalcHeight(text, width);
        //        float num2 = Mathf.Max(80f, num);
        //        Rect position = new Rect(10f, rowY + 10f, 15f, 15f);
        //        Rect rect2 = new Rect(0f, rowY + __result, fillRect.width, num2);
        //        if (Mouse.IsOver(rect2))
        //        {
        //            GUI.DrawTexture(rect2, TexUI.HighlightTex);
        //        }
        //        Text.Font = GameFont.Small;
        //        Text.Anchor = TextAnchor.UpperLeft;
        //        Widgets.DrawRectFast(position, faction.Color);
        //        string label = "RW_FactionPower".Translate(WorldUtility.GetRimWarDataForFaction(faction) == null ? 0 : WorldUtility.GetRimWarDataForFaction(faction).TotalFactionPoints);
        //        label += "\n" + "RW_FactionBehavior".Translate(WorldUtility.GetRimWarDataForFaction(faction).behavior.ToString());
        //        Widgets.Label(rect, label);
        //        if (!faction.IsPlayer)
        //        {

        //        }
        //        __result += num2;
        //    }
        //}

        private static void Settlement_InspectString_WithPoints_Postfix(Settlement __instance, ref string __result)
        {
            if (__instance != null && !__instance.Faction.def.hidden)
            {
                RimWarSettlementComp rwsc = __instance.GetComponent<RimWarSettlementComp>();
                RimWarData rwd = WorldUtility.GetRimWarDataForFaction(__instance.Faction);
                if (rwsc != null && rwd != null)
                {
                    string text = "";
                    if (!__result.NullOrEmpty())
                    {
                        text += "\n";
                    }
                    if (rwsc.PointDamage > 0)
                    {
                        text += "RW_SettlementPointsDamaged".Translate(rwsc.RimWarPoints, rwsc.PointDamage) + "\n" + "RW_FactionBehavior".Translate(rwd.behavior.ToString());
                    }
                    else
                    {
                        text += "RW_SettlementPoints".Translate(rwsc.RimWarPoints) + "\n" + "RW_FactionBehavior".Translate(rwd.behavior.ToString());
                    }
                    if (rwd.GetCapitol != null && rwd.GetCapitol == __instance)
                    {
                        text += "\n" + "RW_Capitol".Translate();
                    }
                    if(rwsc.UnderAttack)
                    {
                        string attackers = "";
                        foreach(WarObject waro in rwsc.AttackingUnits)
                        {
                            attackers += "\n" + waro.Faction.Name + " " + waro.RimWarPoints + " (" + waro.PointDamage + ")";
                        }
                        text += "\n" + "RW_SettlementUnderAttackText".Translate(attackers);
                    }
                    if (rwsc.RWD.behavior == RimWarBehavior.Player)
                    {
                        text += "\n"+"RW_AggressionDefense".Translate(WorldUtility.Get_WCPT().minimumHeatForPlayerAction);
                    }
                    else if(rwsc.RWD.behavior == RimWarBehavior.Vassal)
                    {
                        text += "\n" + "RW_AggressionDefense".Translate(rwsc.vassalHeat);
                    }
                    else
                    {
                        text += "\n"+"RW_AggressionPoints".Translate(rwsc.PlayerHeat);
                    }
                    __result += text;
                }
            }
        }

        [HarmonyPatch(typeof(WorldPathPool), "GetEmptyWorldPath", null)]
        public class WorldPathPool_Prefix_Patch
        {
            public static bool Prefix(WorldPathPool __instance, ref List<WorldPath> ___paths, ref WorldPath __result)
            {
                //List<WorldPath> paths = Traverse.Create(root: __instance).Field(name: "paths").GetValue<List<WorldPath>>();
                for (int i = 0; i < ___paths.Count; i++)
                {
                    if (!___paths[i].inUse)
                    {
                        ___paths[i].inUse = true;
                        __result = ___paths[i];
                        return false;
                    }
                }
                if (___paths.Count > Find.WorldObjects.CaravansCount + 2 + (Find.WorldObjects.RoutePlannerWaypointsCount - 1))
                {
                    //Log.ErrorOnce("WorldPathPool leak: more paths than caravans. Force-recovering.", 664788);
                    ___paths.Clear();
                }
                WorldPath worldPath = new WorldPath();
                ___paths.Add(worldPath);
                worldPath.inUse = true;
                __result = worldPath;
                return false;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_Ambush_EnemyFaction), "CanFireNowSub", null)]
        public class CanFireNow_Ambush_EnemyFaction_RemovalPatch
        {
            public static bool Prefix(IncidentWorker_Ambush_EnemyFaction __instance, IncidentParms parms, ref bool __result)
            {
                Options.SettingsRef settingsRef = new Options.SettingsRef();
                if (settingsRef.restrictEvents)
                {
                    if (__instance != null && __instance.def.defName != "VisitorGroup" && __instance.def.defName != "VisitorGroupMax" && !__instance.def.defName.Contains("Cult") && parms.quest == null &&
                        !parms.forced && !__instance.def.workerClass.ToString().StartsWith("Rumor_Code") && !(parms.faction != null && parms.faction.Hidden))
                    {
                        __result = false;
                        //try
                        //{
                        //    Log.Message("Filtered event: " + __instance.def.defName);
                        //}
                        //catch
                        //{
                        //    Log.Message("filtered an event without a def...");
                        //}
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_CaravanDemand), "CanFireNowSub", null)]
        public class CanFireNow_CaravanDemand_RemovalPatch
        {
            public static bool Prefix(IncidentWorker_CaravanDemand __instance, IncidentParms parms, ref bool __result)
            {
                Options.SettingsRef settingsRef = new Options.SettingsRef();
                if (settingsRef.restrictEvents)
                {
                    if (__instance != null && __instance.def.defName != "VisitorGroup" && __instance.def.defName != "VisitorGroupMax" && !__instance.def.defName.Contains("Cult") && parms.quest == null &&
                        !parms.forced && !__instance.def.workerClass.ToString().StartsWith("Rumor_Code") && !(parms.faction != null && parms.faction.Hidden))
                    {
                        __result = false;
                        //try
                        //{
                        //    Log.Message("Filtered event: " + __instance.def.defName);
                        //}
                        //catch
                        //{
                        //    Log.Message("filtered an event without a def...");
                        //}
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_CaravanMeeting), "CanFireNowSub", null)]
        public class CanFireNow_CaravanMeeting_RemovalPatch
        {
            public static bool Prefix(IncidentWorker_CaravanMeeting __instance, IncidentParms parms, ref bool __result)
            {
                Options.SettingsRef settingsRef = new Options.SettingsRef();
                if (settingsRef.restrictEvents)
                {
                    if (__instance != null && __instance.def.defName != "VisitorGroup" && __instance.def.defName != "VisitorGroupMax" && !__instance.def.defName.Contains("Cult") && parms.quest == null &&
                        !parms.forced && !__instance.def.workerClass.ToString().StartsWith("Rumor_Code") && !(parms.faction != null && parms.faction.Hidden))
                    {
                        __result = false;
                        //try
                        //{SettlementProximityGoodwillUtility
                        //}
                        //catch
                        //{
                        //    Log.Message("filtered an event without a def...");
                        //}
                        return false;
                    }
                }
                return true;
            }
        }

        public static class SettlementProximity_NoVassalDegradation_Patch
        {
            // KEEP: The TargetMethod approach (safer)
            static MethodBase TargetMethod()
            {
                var method = typeof(SettlementProximityGoodwillUtility).GetMethod("AppendProximityGoodwillOffsets");
                if (method == null)
                {
                    Log.Warning("RimWar: SettlementProximityGoodwillUtility.AppendProximityGoodwillOffsets method not found - patch disabled");
                }
                return method;
            }
            
            // KEEP: Your existing Postfix method
            public static void Postfix(int tile, ref List<Pair<Settlement, int>> outOffsets, bool ignoreIfAlreadyMinGoodwill, bool ignorePermanentlyHostile)
            {
                List<Pair<Settlement, int>> rem = new List<Pair<Settlement, int>>(); 
                for(int i = 0; i < outOffsets.Count; i++)
                {
                    if(WorldUtility.IsVassalFaction(outOffsets[i].First.Faction))
                    {
                        rem.Add(outOffsets[i]);
                    }
                }

                for(int i = 0; i < rem.Count; i++)
                {
                    outOffsets.Remove(rem[i]);
                }
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_PawnsArrive), "CanFireNowSub", null)]
        public class CanFireNow_PawnsArrive_RemovalPatch
        {
            public static bool Prefix(IncidentWorker_PawnsArrive __instance, IncidentParms parms, ref bool __result)
            {
                Options.SettingsRef settingsRef = new Options.SettingsRef();
                if (settingsRef.restrictEvents)
                {
                    if (__instance != null && __instance.def.defName != "VisitorGroup" && __instance.def.defName != "VisitorGroupMax" && !__instance.def.defName.Contains("Cult") && parms.quest == null &&
                        !parms.forced && !__instance.def.workerClass.ToString().StartsWith("Rumor_Code"))
                    { 
                        if(parms.faction != null)
                        {
                            if(parms.faction.Hidden || WorldUtility.GetRimWarDataForFaction(parms.faction).behavior == RimWarBehavior.Excluded)
                            {
                                return true;
                            }
                        }
                        if (__instance.def == IncidentDefOf.RaidEnemy || __instance.def == IncidentDefOf.RaidFriendly || __instance.def == IncidentDefOf.TraderCaravanArrival)
                        {
                            __result = false;
                            //try
                            //{
                            //    Log.Message("Filtered event: " + __instance.def.defName);
                            //}
                            //catch
                            //{
                            //    Log.Message("filtered an event without a def...");
                            //}
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents")]
        public static class Patch_Page_CreateWorldParams_DoWindowContents
        {
            private static void Postfix(Page_CreateWorldParams __instance, Rect rect)
            {
                float y = rect.y + rect.height - 118f;
                Text.Font = GameFont.Small;
                string label = "RW_RimWar".Translate();
                if (Widgets.ButtonText(new Rect(0f, y, 150f, 32f), label))
                {
                    OpenSettingsWindow(__instance);
                }
            }

            public static void OpenSettingsWindow(Page_CreateWorldParams __instance)
            {
                //Find.WindowStack.TryRemove(typeof(EditWindow_Log));
                if (!Find.WindowStack.TryRemove(typeof(Options.RimWarSettingsWindow)))
                {
                    Options.RimWarSettingsWindow rwsw = new Options.RimWarSettingsWindow();
                    rwsw.page_ref = __instance;
                    Find.WindowStack.Add(rwsw);
                }
            }
        }

        [HarmonyPatch(typeof(FactionDialogMaker), "FactionDialogFor")]
        public static class CommsConsole_RimWarOptions_Patch
        {
            private static void Postfix(Pawn negotiator, Faction faction, ref DiaNode __result)
            {
                List<DiaOption> removeList = new List<DiaOption>();
                removeList.Clear();
                foreach (DiaOption x in __result.options)
                {
                    string text = Traverse.Create(root: x).Field(name: "text").GetValue<string>();
                    if (text.Contains("Request a trade caravan") && !removeList.Contains(x))
                    {
                        removeList.Add(x);
                    }
                    if (text.Contains("Request immediate military aid") && !removeList.Contains(x))
                    {
                        removeList.Add(x);
                    }
                }
                for (int i = 0; i < removeList.Count; i++)
                {
                    __result.options.Remove(removeList[i]);
                }
                //__result.options.Remove(__result.options[0]);
                //__result.options.Remove(__result.options[0]);
                __result.options.Insert(0, FactionDialogReMaker.RequestTraderOption(negotiator.Map, faction, negotiator));
                __result.options.Insert(1, FactionDialogReMaker.RequestMilitaryAid_Scouts_Option(negotiator.Map, faction, negotiator));
                __result.options.Insert(2, FactionDialogReMaker.RequestMilitaryAid_Warband_Option(negotiator.Map, faction, negotiator));
                __result.options.Insert(3, FactionDialogReMaker.RequestMilitaryAid_LaunchedWarband_Option(negotiator.Map, faction, negotiator));
            }
        }

        //[HarmonyPatch(typeof(IncidentWorker), "CanFireNow", null)]
        //public class CanFireNow_Monitor
        //{
        //    public static bool Prefix(IncidentWorker __instance, IncidentParms parms, ref bool __result)
        //    {                
        //        Log.Message("incident of " + __instance.def.defName + " with type " + __instance.GetType().ToString() + " attempting to fire with points " + parms.points + " against " + parms.target.ToString());
        //        return true;
        //    }
        //}

        // [HarmonyPatch]
        // public static class WorldReachability_CanReach_Patch
        // {
        //     static MethodBase TargetMethod()
        //     {
        //         // Try to find the method safely
        //         var method = typeof(WorldReachability).GetMethod("CanReach", 
        //             BindingFlags.Public | BindingFlags.Static,
        //             null,
        //             new Type[] { typeof(int), typeof(int) },
        //             null);
                    
        //         if (method == null)
        //         {
        //             Log.Warning("RimWar: WorldReachability.CanReach method not found - patch disabled for RimWorld 1.6");
        //         }
                
        //         return method; // Return null to disable patch if method doesn't exist
        //     }
            
        //     public static bool Prefix(ref bool __result, int startTile, int destTile)
        //     {
        //         // Add your patch logic here, or just return true to skip
        //         return true; // Continue with original method
        //     }
        // }
    }
}
