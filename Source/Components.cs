using ChangeDresser.StoredApparel;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    class WorldComp : WorldComponent
    {
        public WorldComp(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            StoredApparelContainer.Clear();
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
