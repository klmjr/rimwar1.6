using System;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using System.Linq;

namespace RimWar.Planet
{
    public class TransportPodsArrivalAction_GiveSupplies : TransportersArrivalAction
    {
        private Settlement settlement;
        private PawnsArrivalModeDef arrivalMode;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Settlement>(ref this.settlement, "settlement", false);
            Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
        }

        public TransportPodsArrivalAction_GiveSupplies()
        {
        }

        public TransportPodsArrivalAction_GiveSupplies(Settlement _settlement, PawnsArrivalModeDef _arrivalMode)
        {
            this.settlement = _settlement;
            this.arrivalMode = _arrivalMode;
        }

        public bool ShouldUseLongEvent(List<ActiveTransporter> pods, int tile)
        {
            return !settlement.HasMap;
        }

        public FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
            if (!(bool)floatMenuAcceptanceReport)
            {
                return floatMenuAcceptanceReport;
            }
            if (settlement != null && settlement.Tile != destinationTile)
            {
                return false;
            }
            return CanGiveSupplies(pods, settlement);
        }

        public static FloatMenuAcceptanceReport CanGiveSupplies(IEnumerable<IThingHolder> pods, Settlement settlement)
        {
            if (settlement == null || !settlement.Spawned)
            {
                return false;
            }
            RimWarSettlementComp rwsc = settlement.GetComponent<RimWarSettlementComp>();
            if (rwsc == null)
            {
                return false;
            }
            return true;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, Settlement settlement)
        {
            foreach (FloatMenuOption floatMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(
                () => CanGiveSupplies(pods, settlement),
                () => new TransportPodsArrivalAction_GiveSupplies(settlement, PawnsArrivalModeDefOf.EdgeDrop),
                "RW_GiveSupplies".Translate(settlement.Label),
                (tile, arrivalAction) => representative.TryLaunch(tile, arrivalAction),
                settlement.Tile))
            {
                yield return floatMenuOption;
            }
        }

        public override bool GeneratesMap => true;
        
        public override void Arrived(List<ActiveTransporterInfo> pods, PlanetTile tile)
        {
            Thing lookTarget = TransportersArrivalActionUtility.GetLookTarget(pods);
            bool num = !settlement.HasMap;
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
            
            // Transfer supplies to settlement
            RimWarSettlementComp rwsc = settlement.GetComponent<RimWarSettlementComp>();
            if (rwsc != null)
            {
                // Add logic to transfer supplies here
                rwsc.RimWarPoints += 100; // Example - adjust as needed
            }
            
            TaggedString letterLabel = "RW_SuppliesDelivered".Translate();
            TaggedString letterText = "RW_SuppliesDeliveredDesc".Translate(settlement.Label);
            
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, lookTarget);
            arrivalMode.Worker.TravellingTransportersArrived(pods, orGenerateMap);
        }
    }
}
