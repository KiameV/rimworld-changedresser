using ChangeDresser.StoredApparel;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    class WorldComp : WorldComponent
    {
        public WorldComp(World world) : base(world) { }
        
        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                StoredApparelContainer.Clear();
            }

            List<StoredApparelSet> sets = null;
            
            if (!Settings.LinkGroupsToDresser && Scribe.mode == LoadSaveMode.Saving)
            {
                sets = new List<StoredApparelSet>(StoredApparelContainer.GetAllApparelSets());
            }

            Scribe_Collections.Look(ref sets, "storedApparelSet", LookMode.Deep, new object[0]);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (sets != null && sets.Count > 0)
                {
                    StoredApparelContainer.AddApparelSets(sets);
                }
            }
        }
    }

    class GameComp : GameComponent
    {
        public GameComp() { }

        public GameComp(Game game) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Dictionary<string, Pawn> pawnIdToPawn = new Dictionary<string, Pawn>();
            foreach (Pawn p in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
                pawnIdToPawn.Add(p.ThingID, p);
            StoredApparelContainer.Initialize(pawnIdToPawn);
        }
    }
}
