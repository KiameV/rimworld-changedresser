using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using System;
using System.Diagnostics;

namespace ChangeDresser
{
    class WorldComp : WorldComponent
    {
        private static Stopwatch stopWatch = null;

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

        public static bool AddApparel(Apparel apparel)
        {
            foreach (Building_Dresser d in DressersToUse)
            {
                if (d.settings.AllowedToAccept(apparel))
                {
                    d.AddApparel(apparel);
                    return true;
                }
            }
            return false;
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
                    break;
                }
            }
            if (!added)
            {
                DressersToUse.Add(dresser);
            }

            if (stopWatch == null)
            {
                stopWatch = new Stopwatch();
                stopWatch.Start();
            }
        }

        public static bool RemoveDesser(Building_Dresser dresser)
        {
            if (DressersToUse.Remove(dresser))
            {
                if (DressersToUse.Count == 0)
                {
                    stopWatch.Stop();
                    stopWatch = null;
                }
                return true;
            }
            return false;
        }

        public static void SortDressersToUse()
        {
            if (stopWatch == null)
            {
#if DEBUG || DRESSER_LIST_DEBUG
                Log.Warning("WorldComp.SortDressersToUse: stopWatch null. RETURN");
#endif
                return;
            }

            if (stopWatch.ElapsedTicks < TimeSpan.TicksPerMinute)
            {
#if DEBUG || DRESSER_LIST_DEBUG
                Log.Warning("WorldComp.SortDressersToUse: stopWatch.ElapsedTicks < TimeSpan.TicksPerMinute. RETURN");
#endif
                return;
            }

            for (int i = 0; i < DressersToUse.Count - 1; ++i)
            {
                if (DressersToUse[i].settings.Priority < DressersToUse[i + 1].settings.Priority)
                {
                    Building_Dresser tmp = DressersToUse[i];
                    DressersToUse[i] = DressersToUse[i + 1];
                    DressersToUse[i + 1] = tmp;
                }
            }
            stopWatch.Reset();

#if DEBUG || DRESSER_LIST_DEBUG
            foreach (Building_Dresser d in DressersToUse)
            {
                Log.Warning(d.Label + " " + d.settings.Priority + ", ");
            }
#endif
        }

        private List<PawnOutfits> tempPawnOutfits = null;
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.tempPawnOutfits = new List<PawnOutfits>(PawnOutfits.Count);
                foreach (PawnOutfits po in PawnOutfits.Values)
                {
                    if (po != null)
                        this.tempPawnOutfits.Add(po);
                }
            }

            Scribe_Collections.Look(ref this.tempPawnOutfits, "pawnOutfits", LookMode.Deep, new object[0]);

            List<Outfit> ofb = OutfitsForBattle;
            Scribe_Collections.Look(ref ofb, "outfitsForBattle", LookMode.Reference, new object[0]);
            OutfitsForBattle = ofb;

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (PawnOutfits == null)
                {
                    PawnOutfits = new Dictionary<Pawn, PawnOutfits>();
                }

                if (OutfitsForBattle == null)
                {
                    OutfitsForBattle = new List<Outfit>();
                }

                PawnOutfits.Clear();
                if (this.tempPawnOutfits != null)
                {
                    foreach (PawnOutfits po in this.tempPawnOutfits)
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
