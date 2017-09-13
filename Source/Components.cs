using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    class WorldComp : WorldComponent
    {
        public static List<Building_Dresser> DressersToUse { get; private set; }
        public static Dictionary<Pawn, PawnOutfits> PawnOutfits { get; private set; }

        public WorldComp(World world) : base(world)
        {
            if (DressersToUse != null)
            {
                DressersToUse.Clear();
            }
            DressersToUse = new List<Building_Dresser>();

            if (PawnOutfits != null)
            {
                PawnOutfits.Clear();
            }
            PawnOutfits = new Dictionary<Pawn, PawnOutfits>();
        }

        public static void AddDresser(Building_Dresser dresser)
        {
            bool added = false;
            for (int i = 0; i < DressersToUse.Count; ++i)
            {
                if (dresser.settings.Priority > DressersToUse[i].settings.Priority)
                {
                    added = true;
                    DressersToUse.Insert(i, dresser);
#if DEBUG
                    Log.Warning("Dresser inserted at index " + i + ". Number of Dressers to Use: " + DressersToUse.Count);
#endif
                }
                if (!added)
                {
                    DressersToUse.Add(dresser);
                }
            }
        }

        private List<PawnOutfits> tempPawnOutfits = null;
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.tempPawnOutfits = new List<PawnOutfits>(PawnOutfits.Count);
                foreach (PawnOutfits po in PawnOutfits.Values)
                {
                    this.tempPawnOutfits.Add(po);
                }
            }

            Scribe_Collections.Look(ref this.tempPawnOutfits, "pawnOutfits", LookMode.Deep, new object[0]);

            if (Scribe.mode == LoadSaveMode.PostLoadInit &&
                this.tempPawnOutfits != null)
            {
                foreach (PawnOutfits po in this.tempPawnOutfits)
                {
                    PawnOutfits.Add(po.Pawn, po);
                }
            }

            if (this.tempPawnOutfits != null &&
                (Scribe.mode == LoadSaveMode.Saving ||
                 Scribe.mode == LoadSaveMode.PostLoadInit))
            {
                this.tempPawnOutfits.Clear();
                this.tempPawnOutfits = null;
            }
        }
    }
}
