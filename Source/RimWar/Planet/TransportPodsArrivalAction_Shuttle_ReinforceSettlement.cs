using System;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using System.Linq;

namespace RimWar.Planet
{
    public class TransportPodsArrivalAction_Shuttle_ReinforceSettlement : TransportPodsArrivalAction_Shuttle
    {
        private Settlement settlement;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref settlement, "settlement");
        }

        public TransportPodsArrivalAction_Shuttle_ReinforceSettlement()
        {
        }

        public TransportPodsArrivalAction_Shuttle_ReinforceSettlement(Settlement _settlement, MapParent _mapParent)
        {
            this.settlement = _settlement;
            this.mapParent = _mapParent;
        }

        public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
        {
            Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
            bool num = !this.settlement.HasMap;
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, null);
            Settlement settlement;
            if ((settlement = (orGenerateMap.Parent as Settlement)) != null && settlement.Faction != Faction.OfPlayer)
            {
                TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
                TaggedString letterText = "LetterTransportPodsLandedInEnemyBase".Translate(settlement.Label).CapitalizeFirst();
                //SettlementUtility.AffectRelationsOnAttacked_NewTmp(ssettlement, ref letterText);
                if (num)
                {
                    Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                    PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
                }
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTarget);
            }
            foreach (ActiveDropPodInfo pod in pods)
            {
                pod.missionShuttleHome = missionShuttleHome;
                pod.missionShuttleTarget = missionShuttleTarget;
                pod.sendAwayIfQuestFinished = sendAwayIfQuestFinished;
                pod.questTags = questTags;
            }
            PawnsArrivalModeDefOf.Shuttle.Worker.TravelingTransportPodsArrived(pods, orGenerateMap);
            Messages.Message("MessageShuttleArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
            RimWarSettlementComp rwsc = settlement.GetComponent<RimWarSettlementComp>();
            if (rwsc != null && rwsc.UnderAttack)
            {
                IncidentUtility.GenerateSettlementAttackers(rwsc, orGenerateMap);
            }
        }
    }
}
