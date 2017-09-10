using ChangeDresser.StoredApparel;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    // TODO add ticks for updating removed apparel ids
    class WorldComp : WorldComponent
    {
        public WorldComp(World world) : base(world)
        {
            StoredApparelContainer.Clear();
        }

        /*private static List<int> removedApparelIds = new List<int>();

        public static void AssignedApparelRemoved(Pawn pawn, Apparel apparel, Building_Dresser dresser)
        {
            if (StoredApparelContainer.IsApparelUsedInSets(pawn, apparel, dresser))
            {
                removedApparelIds.Add(apparel.thingIDNumber);
            }
        }

        public static bool IsAssignedApparel(Apparel a)
        {
            for (int i = 0; i < removedApparelIds.Count; ++i)
            {
                if (removedApparelIds[i] == a.thingIDNumber)
                {
                    removedApparelIds.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }*/
        private List<StorageForPawn> stores;

        public override void ExposeData()
        {
            base.ExposeData();
#if DEBUG
            Log.Warning("WorldComp: " + Scribe.mode);
#endif

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.stores = new List<StorageForPawn>(StoredApparelContainer.StoredApparelSets.Count);

                foreach (StorageForPawn l in StoredApparelContainer.StoredApparelSets.Values)
                {
                    if (l.HasSets())
                    {
                        this.stores.Add(l);
                    }
                }
            }

            Scribe_Collections.Look(ref this.stores, "apparelSets", LookMode.Deep, new object[0]);
            //Scribe_Collections.Look(ref removedApparelIds, "removedApparelIds", LookMode.Deep, new object[0]);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                StoredApparelContainer.Clear();
#if DEBUG
                Log.Warning("WorldComp: stores count: " + this.stores.Count);
#endif
                if (stores != null)
                {
#if DEBUG
                    Log.Warning("WorldComp: Loading StorageForPawn: " + stores.Count);
#endif
                    foreach (StorageForPawn s in this.stores)
                    {
#if DEBUG
                        Log.Warning("WorldComp: Loading StorageForPawn: " + s.ToString());
#endif
                        StoredApparelContainer.Add(s);
                    }
                }
#if DEBUG
                else
                {
                    Log.Warning("WorldComp: Loading StorageForPawn: null");
                }
#endif
                this.stores.Clear();
                this.stores = null;
            }
        }
    }
}
