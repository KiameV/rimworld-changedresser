using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    class WorldComp : WorldComponent
    {
        public static List<Building_Dresser> DressersToUse { get; private set; }
        public static Dictionary<Pawn, PawnOutfits> PawnOutfits { get; private set; }
        public static List<Outfit> OutfitsForBattle { get; private set; }

        public WorldComp(World world) : base(world)
        {
            if (DressersToUse != null)
            {
                DressersToUse.Clear();
            }
            else
            {
                DressersToUse = new List<Building_Dresser>();
            }

            if (PawnOutfits != null)
            {
                PawnOutfits.Clear();
            }
            else
            {
                PawnOutfits = new Dictionary<Pawn, PawnOutfits>();
            }

            if (OutfitsForBattle != null)
            {
                OutfitsForBattle.Clear();
            }
            else
            {
                OutfitsForBattle = new List<Outfit>();
            }
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

            List<Outfit> ofb = OutfitsForBattle;
            Scribe_Collections.Look(ref ofb, "outfitsForBattle", LookMode.Reference, new object[0]);
            OutfitsForBattle = ofb;

            if (Scribe.mode == LoadSaveMode.PostLoadInit &&
                this.tempPawnOutfits != null)
            {
                foreach (PawnOutfits po in this.tempPawnOutfits)
                {
                    if (po != null)
                    {
                        PawnOutfits.Add(po.Pawn, po);
                    }
                }
                for (int i = OutfitsForBattle.Count - 1; i >= 0; --i)
                {
                    if (OutfitsForBattle[i] == null)
                    {
                        OutfitsForBattle.RemoveAt(i);
                    }
                }

                if (PawnOutfits == null)
                {
                    PawnOutfits = new Dictionary<Pawn, PawnOutfits>();
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
