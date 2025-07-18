﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWar.Planet
{
    public class Warband : WarObject
    {        
        private int lastEventTick = 0;        
        private int ticksPerMove = 2800;
        private int searchTick = 60;               

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksPerMove, "ticksPerMove", 2800, false);                       
        }

        public override WorldObjectDef GetDef => this.def;

        public Warband()
        {

        }

        public override int RimWarPoints { get => base.RimWarPoints; set => base.RimWarPoints = value; }
        public override bool MovesAtNight { get => base.MovesAtNight; set => base.MovesAtNight = value; }
        public override float DetectionModifier => .8f;
        public override float MovementModifier => (2500f/(float)TicksPerMove);

        public override bool NightResting
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                if (pather.Moving && pather.nextTile == pather.Destination && Caravan_PathFollower.IsValidFinalPushDestination(pather.Destination) && Mathf.CeilToInt(pather.nextTileCostLeft / 1f) <= 10000)
                {
                    return false;
                }
                if (MovesAtNight)
                {
                    return !CaravanNightRestUtility.RestingNowAt(base.Tile);
                }
                return CaravanNightRestUtility.RestingNowAt(base.Tile);
            }
        }

        public override int TicksPerMove
        {
            get
            {
                return this.ticksPerMove;
            }
            set
            {
                this.ticksPerMove = value;
            }
        }      

        public override void Notify_Player()
        {
            base.Notify_Player();
            if(!playerNotified && this.DestinationTarget != null)
            {
                Options.SettingsRef settingsRef = new Options.SettingsRef();
                if (this.DestinationTarget.Faction == Faction.OfPlayer && this.Faction.HostileTo(Faction.OfPlayer) && Find.WorldGrid.TraversalDistanceBetween(this.Tile, this.DestinationTarget.Tile) <= settingsRef.letterNotificationRange && Rand.Chance(.4f))
                {
                    playerNotified = true;
                    StringBuilder stringBuilder = new StringBuilder();
                    float num6 = (float)Utility.ArrivalTimeEstimator.EstimatedTicksToArrive(base.Tile, this.DestinationTarget.Tile, this) / 60000f;
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.Append("RW_EstimatedTimeToDestination".Translate(num6.ToString("0.#")));
                    Find.LetterStack.ReceiveLetter("RW_LetterApproachingThreatEvent".Translate(), "RW_LetterApproachingThreatEventText".Translate(this.Name, this.RimWarPoints, this.DestinationTarget.Label, stringBuilder), RimWarDefOf.RimWar_WarningEvent);
                }
            }
        }

        //NextSearchTick
        //NextSearchTickIncrement (override by type)
        //ScanRange (override by type, base is 1f)
        //EngageNearbyWarObject --> IncidentUtility -- > ImmediateAction
        //EngageNearbyCaravan --> IncidentUtility --> ImmediateAction
        //NotifyPlayer
        //NextMoveTick
        //NextMoveTickIncrement (default is settings based)
        //ArrivalAction

        //public override int NextSearchTickIncrement => Rand.Range(500, 600);
        //public override float ScanRange => 1f;

        protected override void Tick()
        {
            base.Tick();            
        }

        public override void EngageNearbyCaravan(Caravan car)
        {
            if(car.Faction != null && car.Faction == Faction.OfPlayer && this.Faction.HostileTo(car.Faction))
            {
                if (ShouldInteractWith(car, this) || ((car.PlayerWealthForStoryteller / 80) <= (this.RimWarPoints) && CaravanDetected(car)))
                {
                    this.interactable = false;
                    IncidentUtility.DoCaravanAttackWithPoints(this, car, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                }
            }
            else
            {
                if(ShouldInteractWith(car, this))
                {
                    this.interactable = false;
                    IncidentUtility.DoCaravanAttackWithPoints(this, car, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                }
            }            
        }

        public override void EngageNearbyWarObject(WarObject rwo)
        {
            if (rwo.Faction != null && rwo.Faction.HostileTo(this.Faction))
            {
                IncidentUtility.ResolveRimWarBattle(this, rwo);
                //ImmediateAction(rwo);                
            }
        }

        public override void ArrivalAction()
        {
            //Log.Message("beginning arrival actions - warband; destination: " + this.DestinationTarget.Label + " parent: " + this.ParentSettlement.Label);
            WorldObject wo = this.DestinationTarget;
            if(wo != null)
            {
                if (wo.Faction != this.Faction)
                {
                    if (wo.Faction.HostileTo(this.Faction))
                    {
                        if (wo.Faction == Faction.OfPlayer)
                        {
                            //Do Raid
                            if (wo is Settlement)
                            {
                                //RimWorld.Planet.Settlement playerSettlement = Find.WorldObjects.SettlementAt(this.Tile);
                                Settlement playerSettlement = wo as Settlement;
                                if (playerSettlement != null)
                                {
                                    //Raid Player Map
                                    if (this.launched && Options.Settings.Instance.allowDropPodRaids)
                                    {
                                        PawnsArrivalModeDef arrivalDef = PawnsArrivalModeDefOf.EdgeDrop;
                                        if (Rand.Chance(.35f))
                                        {
                                            arrivalDef = PawnsArrivalModeDefOf.CenterDrop;
                                        }
                                        else if (Rand.Chance(.2f))
                                        {
                                            arrivalDef = RimWarDefOf.RandomDrop;
                                        }

                                        IncidentUtility.DoRaidWithPoints(this, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(arrivalDef));
                                    }
                                    else
                                    {
                                        IncidentUtility.DoRaidWithPoints(this, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                                    }
                                }
                            }
                            else if (wo is Caravan)
                            {
                                //Caravan playerCaravan = Find.WorldObjects.PlayerControlledCaravanAt(this.Tile);
                                Caravan playerCaravan = wo as Caravan;
                                if (playerCaravan != null)
                                {
                                    //Raid player caravan
                                    IncidentUtility.DoCaravanAttackWithPoints(this, playerCaravan, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                                }
                            }
                        }
                        else
                        {
                            if (wo is Settlement)
                            {
                                Settlement factionSettlement = wo as Settlement;
                                //RimWarSettlementComp settlement = WorldUtility.GetRimWarSettlementAtTile(this.Tile);
                                RimWarSettlementComp rwsc = factionSettlement.GetComponent<RimWarSettlementComp>();
                                if (rwsc != null)
                                {
                                    if (rwsc.parent.Faction == Faction.OfPlayer)
                                    {
                                        //RimWorld.Planet.Settlement playerSettlement = Find.World.worldObjects.SettlementAt(this.Tile);                                        
                                        IncidentUtility.DoRaidWithPoints(this, factionSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                                    }
                                    else
                                    {
                                        IncidentUtility.ResolveWarObjectAttackOnSettlement(this, this.ParentSettlement, rwsc, WorldUtility.GetRimWarDataForFaction(this.Faction));
                                    }
                                }
                            }
                            else if (wo is WarObject)
                            {
                                IncidentUtility.ResolveWorldEngagement(this, wo);
                            }
                        }
                    }
                    else
                    {
                        if (wo.Faction == Faction.OfPlayerSilentFail) // reinforce player
                        {
                            //RimWorld.Planet.Settlement playerSettlement = Find.World.worldObjects.SettlementAt(this.Tile);
                            if (wo is Settlement)
                            {
                                Settlement playerSettlement = wo as Settlement;
                                if (playerSettlement != null)
                                {
                                    //Raid Player Map
                                    if ((this.rimwarData.behavior == RimWarBehavior.Warmonger) || (this.rimwarData.behavior == RimWarBehavior.Aggressive && Rand.Chance(.5f)))
                                    {
                                        if (this.launched && Options.Settings.Instance.allowDropPodRaids)
                                        {
                                            IncidentUtility.DoRaidWithPoints(this, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(RimWarDefOf.RandomDrop));
                                        }
                                        else
                                        {
                                            IncidentUtility.DoRaidWithPoints(this, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                                        }
                                    }
                                    else
                                    {
                                        if (this.launched && Options.Settings.Instance.allowDropPodRaids)
                                        {
                                            IncidentUtility.DoRaidWithPoints(this, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.CenterDrop));
                                        }
                                        else
                                        {
                                            IncidentUtility.DoReinforcementWithPoints(this, playerSettlement, WorldUtility.GetRimWarDataForFaction(this.Faction), IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.WarSettlementComp.RimWarPoints += Mathf.RoundToInt(this.RimWarPoints / 2f);
                    this.WarSettlementComp.PointDamage += Mathf.RoundToInt(this.PointDamage / 2f);
                    List<Map> maps = Find.Maps;
                    for (int i =0; i < maps.Count; i++)
                    {
                        RimWorld.Planet.Settlement sBase = maps[i].info.parent as RimWorld.Planet.Settlement;
                        if(sBase != null && sBase.Faction != null && sBase.Tile == this.ParentSettlement.Tile)
                        {
                            //reinforcement against player
                            this.WarSettlementComp.RimWarPoints -= Mathf.RoundToInt(this.RimWarPoints / 2f);
                            this.WarSettlementComp.PointDamage += Mathf.RoundToInt(this.PointDamage / 2f);
                            IncidentUtility.DoRaidWithPoints(this, this.ParentSettlement, this.rimwarData, IncidentUtility.PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                        }
                    }
                }
                base.ArrivalAction();
            }
            else
            {
                //Log.Message("this tile: " + this.Tile + " parent settlement tile: " + this.ParentSettlement.Tile);
                if(this.Tile == ParentSettlement.Tile)
                {
                    if(Find.World.worldObjects.AnyMapParentAt(this.Tile))
                    {
                        //reinforce
                        //Log.Message("attempting to reinforce");
                        //Log.Message("map is spawn " + Find.World.worldObjects.MapParentAt(this.Tile).Spawned);
                        //Log.Message("map " + Find.World.worldObjects.MapParentAt(this.Tile).Map + " has faction " + Find.World.worldObjects.MapParentAt(this.Tile).Faction);
                        this.WarSettlementComp.RimWarPoints += Mathf.RoundToInt(this.RimWarPoints / 2f);
                        this.WarSettlementComp.PointDamage += Mathf.RoundToInt(this.PointDamage / 2f);
                        //IncidentUtility.DoRaidWithPoints(this, this.ParentSettlement, this.rimwarData, PawnsArrivalModeDefOf.EdgeWalkIn, PawnGroupKindDefOf.Combat);
                    }
                    else
                    {
                        //Log.Message("parent settlement points: " + this.ParentSettlement.RimWarPoints);
                        if (wo.Faction != this.Faction) //could happen if parent town is taken over while army is away, in which case - perform another raid
                        {

                        }
                        this.WarSettlementComp.RimWarPoints += Mathf.RoundToInt(this.RimWarPoints / 2f);
                        this.WarSettlementComp.PointDamage += Mathf.RoundToInt(this.PointDamage / 2f);
                    }
                    base.ArrivalAction();
                }
                else
                {
                    RimWarSettlementComp _rwsc = wo.GetComponent<RimWarSettlementComp>();
                    if (_rwsc != null && _rwsc.parent != null && _rwsc.parent.Faction == this.Faction)
                    {
                        _rwsc.RimWarPoints += Mathf.RoundToInt(this.RimWarPoints / 2f);
                        _rwsc.PointDamage += Mathf.RoundToInt(this.PointDamage / 2f);
                        base.ArrivalAction();
                    }
                }
                ValidateParentSettlement();
                FindParentSettlement();
                this.DestinationTarget = ParentSettlement;
                PathToTarget(DestinationTarget);
            }
            //Log.Message("ending arrival actions");            
        }       
    }
}
