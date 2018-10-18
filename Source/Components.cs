using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using System;

namespace ChangeDresser
{
    public class WorldComp : WorldComponent
    {
        public static LinkedList<Building_Dresser> DressersToUse { get; private set; }

        public static Dictionary<Pawn, PawnOutfitTracker> PawnOutfits { get; private set; }
        public static List<Outfit> OutfitsForBattle { get; private set; }
        public static OutfitType GetOutfitType(Outfit outfit) { return OutfitsForBattle.Contains(outfit) ? OutfitType.Battle : OutfitType.Civilian; }
        public static ApparelColorTracker ApparelColorTracker = new ApparelColorTracker();

        private static int nextDresserOutfitId = 0;
        public static int NextDresserOutfitId
        {
            get
            {
                int id = nextDresserOutfitId;
                ++nextDresserOutfitId;
                return id;
            }
        }

        static WorldComp ()
        {
            DressersToUse = new LinkedList<Building_Dresser>();
        }

        public WorldComp(World world) : base(world)
        {
            if (DressersToUse != null)
            {
                DressersToUse.Clear();
            }
            else
            {
                DressersToUse = new LinkedList<Building_Dresser>();
            }

            if (PawnOutfits != null)
            {
                PawnOutfits.Clear();
            }
            else
            {
                PawnOutfits = new Dictionary<Pawn, PawnOutfitTracker>();
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

        public static bool AddApparel(Apparel apparel, Map map = null)
        {
            foreach (Building_Dresser d in DressersToUse)
            {
                if ((map == null || d.Map == map) && apparel != null &&
                    d.settings.AllowedToAccept(apparel))
                {
                    d.AddApparel(apparel);
                    return true;
                }
            }
            return false;
        }

        public static void AddDresser(Building_Dresser dresser)
        {
            if (!DressersToUse.Contains(dresser))
            {
                DressersToUse.AddLast(dresser);
            }
        }

        public static IEnumerable<Building_Dresser> GetDressers(Map map)
        {
            if (DressersToUse != null)
            {
                foreach (Building_Dresser d in DressersToUse)
                {
                    if (map == null ||
                        (d.Spawned && d.Map == map))
                    {
                        yield return d;
                    }
                }
            }
        }        

        public static bool HasDressers()
        {
            return DressersToUse.Count > 0;
        }

        public static bool HasDressers(Map map)
        {
            foreach (Building_Dresser d in DressersToUse)
            {
                if (d.Spawned && d.Map == map)
                    return true;
            }
            return false;
        }

        public static bool RemoveDesser(Building_Dresser dresser)
        {
            if (DressersToUse.Remove(dresser))
            {
                return true;
            }
            return false;
        }

        public static void SortDressersToUse()
        {
            LinkedList<Building_Dresser> l = new LinkedList<Building_Dresser>();
            foreach (Building_Dresser d in DressersToUse)
            {
                bool added = false;
                for (LinkedListNode<Building_Dresser> n = l.First; n != null; n = n.Next)
                {
                    if (d.settings.Priority > n.Value.settings.Priority)
                    {
                        added = true;
                        l.AddBefore(n, d);
                        break;
                    }
                }
                if (!added)
                {
                    l.AddLast(d);
                }
            }
            DressersToUse.Clear();
            DressersToUse = l;
        }

        private List<PawnOutfitTracker> tempPawnOutfits = null;
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.tempPawnOutfits = new List<PawnOutfitTracker>(PawnOutfits.Count);
                foreach (PawnOutfitTracker po in PawnOutfits.Values)
                {
                    if (po != null)
                        this.tempPawnOutfits.Add(po);
                }
            }

            Scribe_Values.Look<int>(ref nextDresserOutfitId, "nextDresserOutfitId", 0);
            Scribe_Collections.Look(ref this.tempPawnOutfits, "pawnOutfits", LookMode.Deep, new object[0]);
            Scribe_Deep.Look(ref ApparelColorTracker, "apparelColorTrack");

            List<Outfit> ofb = OutfitsForBattle;
            Scribe_Collections.Look(ref ofb, "outfitsForBattle", LookMode.Reference, new object[0]);
            OutfitsForBattle = ofb;

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (PawnOutfits == null)
                {
                    PawnOutfits = new Dictionary<Pawn, PawnOutfitTracker>();
                }

                if (OutfitsForBattle == null)
                {
                    OutfitsForBattle = new List<Outfit>();
                }

                PawnOutfits.Clear();
                if (this.tempPawnOutfits != null)
                {
                    foreach (PawnOutfitTracker po in this.tempPawnOutfits)
                    {
                        if (po != null && po.Pawn != null && !po.Pawn.Dead)
                        {
                            PawnOutfits.Add(po.Pawn, po);
                        }
                    }
                }

                for (int i = OutfitsForBattle.Count - 1; i >= 0; --i)
                {
                    if (OutfitsForBattle[i] == null)
                    {
                        OutfitsForBattle.RemoveAt(i);
                    }
                }

                if (ApparelColorTracker == null)
                {
                    ApparelColorTracker = new ApparelColorTracker();
                }

                ApparelColorTracker.PersistWornColors();
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
