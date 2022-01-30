using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace ChangeDresser
{
    public class WorldComp : WorldComponent
    {
        public static LinkedList<Building_Dresser> DresserStorageOrder { get; set; }
        public static LinkedList<Building_Dresser> DresserPullOrder { get; set; }
        public static ICollection<Building_Dresser> DressersToUse => DresserPullOrder;
        public static int DresserCount => DresserPullOrder.Count;

        public static Dictionary<Pawn, PawnOutfitTracker> PawnOutfits { get; private set; }
        public static List<Outfit> OutfitsForBattle { get; private set; }
        public static OutfitType GetOutfitType(Outfit outfit) { return OutfitsForBattle.Contains(outfit) ? OutfitType.Battle : OutfitType.Civilian; }
        public static ApparelColorTracker ApparelColorTracker = new();

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
            DresserStorageOrder = new LinkedList<Building_Dresser>();
            DresserPullOrder = new LinkedList<Building_Dresser>();
        }

        public WorldComp(World world) : base(world)
        {
            DresserStorageOrder?.Clear();
            DresserStorageOrder = new LinkedList<Building_Dresser>();
            DresserPullOrder?.Clear();
            DresserPullOrder = new LinkedList<Building_Dresser>();

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

        public static void SortApparel()
        {
            var d = new Dialog_MessageBox(
                "ChangeDresser.SaveFirst".Translate(), 
                "Yes".Translate(), ()=>
                {
                    List<Apparel> dropped = new();
                    foreach (var d in DressersToUse)
                    {
                        dropped.AddRange(d.EmptyNoDrop());
                    }

                    foreach (var a in dropped)
                    {
                        AddApparel(a);
                    }

                    Messages.Message("Done re-sorting apparel", MessageTypeDefOf.PositiveEvent);
                },
                "No".Translate());
            Find.WindowStack.Add(d);
        }

        public static bool AddApparel(Apparel apparel, Map map = null)
        {
            if (apparel == null)
                return true;
            if (map == null || apparel.Map == null)
                return AddApparelAnyDresser(apparel);

            foreach (PawnOutfitTracker t in PawnOutfits.Values)
                if (t.ContainsCustomApparel(apparel))
                {
                    if (apparel.Spawned)
                        apparel.DeSpawn();
                    return true;
                }

            foreach (Building_Dresser d in GetDressers(map))
            {
                if (d.settings.AllowedToAccept(apparel))
                {
                    d.AddApparel(apparel);
                    return true;
                }
            }
            return false;
        }

        private static bool AddApparelAnyDresser(Apparel apparel)
        {
            if (apparel == null)
                return true;
            foreach (PawnOutfitTracker t in PawnOutfits.Values)
                if (t.ContainsCustomApparel(apparel))
                {
                    if (apparel.Spawned)
                        apparel.DeSpawn();
                    return true;
                }

            foreach (Building_Dresser d in DresserStorageOrder)
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
            if (dresser == null || dresser.Map == null)
            {
                Log.Error("[Change Dresser] Cannot add ChangeDresser that is either null or has a null map.");
                return;
            }

            Task t = Task.Factory.StartNew(() =>
            {
                if (!DresserStorageOrder.Contains(dresser))
                {
                    AddSorted(DresserStorageOrder, dresser);
                }
            });
            if (!DresserPullOrder.Contains(dresser))
            {
                AddSorted(DresserPullOrder, dresser);
            }
            Task.WaitAll(t);
        }

        public static IEnumerable<Building_Dresser> GetDressers(Map map)
        {
            foreach (Building_Dresser d in DresserPullOrder)
            {
                if (map == null ||
                    (d.Spawned && d.Map == map))
                {
                    yield return d;
                }
            }
        }

        public static void ClearAll()
        {
            DresserStorageOrder.Clear();
            DresserPullOrder.Clear();
            PawnOutfits.Clear();
            OutfitsForBattle.Clear();
            ApparelColorTracker.Clear();

        }

        public static void CleanupCustomOutfits()
		{
			foreach (PawnOutfitTracker t in PawnOutfits.Values)
				t.Clean();
		}

		public static bool HasDressers()
        {
            return DresserStorageOrder.Count > 0;
        }

        public static bool HasDressers(Map map)
        {
            foreach (Building_Dresser d in DresserStorageOrder)
            {
                if (d.Spawned && d.Map == map)
                    return true;
            }
            return false;
        }

        public static void RemoveDressers(Map map)
        {
            LinkedListNode<Building_Dresser> n = DresserStorageOrder.First;
            LinkedListNode<Building_Dresser> next;
            Building_Dresser d;
            while (n != null)
            {
                next = n.Next;
                d = n.Value;
                if (d.Map == null || d.Map == map)
                {
                    DresserStorageOrder.Remove(n);
                }
                n = next;
            }

            n = DresserPullOrder.First;
            while (n != null)
            {
                next = n.Next;
                d = n.Value;
                if (d.Map == null || d.Map == map)
                {
                    DresserPullOrder.Remove(n);
                }
                n = next;
            }
        }

        public static bool RemoveDesser(Building_Dresser dresser)
        {
            var b1 = DresserStorageOrder.Remove(dresser);
            var b2 = DresserPullOrder.Remove(dresser);
            return b1 && b2;
        }

        public static void SortDressersToUse()
        {
            Task t = Task.Factory.StartNew(() =>
            {
                Sort(DresserStorageOrder);
            });

            Sort(DresserPullOrder);

            Task.WaitAll(t);
        }

        private static void Sort(LinkedList<Building_Dresser> l)
        {
            var n = l.First;
            Stack<Building_Dresser> s = null;

            while (n != null && n.Next != null)
            {
                if (n.Value.settings.Priority < n.Next.Value.settings.Priority)
                {
                    if (s == null)
                        s = new();
                    s.Push(n.Value);
                    l.Remove(n);
                }
                n = n.Next;
            }

            if (s?.Count > 0)
            {
                while (s.Count != 0)
                    AddSorted(l, s.Pop());
            }
        }

        private static void AddSorted(LinkedList<Building_Dresser> l, Building_Dresser d)
        {
            var n = l.First;
            while (n != null)
            { 
                if (d.settings.Priority > n.Value.settings.Priority)
                {
                    l.AddBefore(n, d);
                    return;
                }
                n = n.Next;
            }
            l.AddLast(d);
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
