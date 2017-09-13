using RimWorld;
using System.Collections.Generic;
using Verse;
using System;
using Verse.AI;

namespace ChangeDresser
{
    class StoredApparel : IExposable
    {
        private static int ID = 0;

        private readonly int UniqueId;
        private Dictionary<Def, LinkedList<Apparel>> StoredApparelLookup = new Dictionary<Def, LinkedList<Apparel>>();
        public int Count { get; private set; }

        public StoredApparel()
        {
            this.UniqueId = ID;
            ++ID;
            this.Count = 0;
        }

        public IEnumerable<Apparel> Apparel
        {
            get
            {
                List<Apparel> l = new List<Apparel>(this.Count);
                foreach (LinkedList<Apparel> ll in this.StoredApparelLookup.Values)
                {
                    foreach (Apparel a in ll)
                    {
                        l.Add(a);
                    }
                }
                return l;
            }
        }

        public void AddApparel(Apparel apparel)
        {
            LinkedList<Apparel> l;
            if (!this.StoredApparelLookup.TryGetValue(apparel.def, out l))
            {
                l = new LinkedList<Apparel>();
                this.StoredApparelLookup.Add(apparel.def, l);
            }
            this.AddApparelToLinkedList(apparel, l);
        }

        private void AddApparelToLinkedList(Apparel apparel, LinkedList<Apparel> l)
        {
            QualityCategory q;
            if (!apparel.TryGetQuality(out q))
            {
                l.AddLast(apparel);
            }
            else
            {
                int hpPercent = apparel.HitPoints / apparel.MaxHitPoints;
                for (LinkedListNode<Apparel> n = l.First; n.Next != null; n = n.Next)
                {
                    QualityCategory nq;
                    if (!n.Value.TryGetQuality(out nq) ||
                        q > nq || 
                        (q == nq && hpPercent >= (n.Value.HitPoints / n.Value.MaxHitPoints)))
                    {
                        l.AddBefore(n, apparel);
                        return;
                    }
                }
                l.AddLast(apparel);
            }
            ++this.Count;
        }

        public bool Contains(Apparel apparel)
        {
            LinkedList<Apparel> l;
            if (this.StoredApparelLookup.TryGetValue(apparel.def, out l))
            {
                return l.Contains(apparel);
            }
            return false;
        }

        public void Clear()
        {
            foreach (LinkedList<Apparel> l in this.StoredApparelLookup.Values)
            {
                l.Clear();
            }
            this.StoredApparelLookup.Clear();
        }

        public bool RemoveBestApparel(Def apparelDef, out Apparel apparel)
        {
            LinkedList<Apparel> l;
            if (this.StoredApparelLookup.TryGetValue(apparelDef, out l))
            {
                if (l.Count > 0)
                {
                    apparel = l.First.Value;
                    l.RemoveFirst();
                    --this.Count;
                    return true;
                }
            }
            apparel = null;
            return false;
        }

        public bool RemoveApparel(Apparel apparel)
        {
            LinkedList<Apparel> l;
            if (this.StoredApparelLookup.TryGetValue(apparel.def, out l))
            {
                if (l.Remove(apparel))
                {
                    --this.Count;
                    return true;
                }
            }
            return false;
        }

        internal bool TryRemoveBestApparel(ThingDef def, ThingFilter filter, out Apparel apparel)
        {
            LinkedList<Apparel> l;
            if (this.StoredApparelLookup.TryGetValue(def, out l))
            {
                for (LinkedListNode<Apparel> n = l.First; n.Next != null; n = n.Next)
                {
                    if (filter.Allows(n.Value))
                    {
                        l.Remove(n);
                        apparel = n.Value;
                        return true;
                    }
                }
            }
            apparel = null;
            return false;
        }

        public List<Apparel> RemoveFilteredApparel(ThingFilter filter)
        {
            List<Apparel> removed = new List<Apparel>(0);
            foreach (LinkedList<Apparel> ll in this.StoredApparelLookup.Values)
            {
                
            }
            return removed;
        }

        private List<Apparel> tempApparelList = null;
        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.tempApparelList = new List<Apparel>(this.Count);
                foreach (LinkedList<Apparel> l in this.StoredApparelLookup.Values)
                {
                    this.tempApparelList.AddRange(l);
                }
            }

            Scribe_Collections.Look(ref this.tempApparelList, "storedApparel", LookMode.Deep, new object[0]);
            

            if (Scribe.mode == LoadSaveMode.PostLoadInit && 
                this.tempApparelList != null)
            {
                Def lastDef = null;
                LinkedList<Apparel> l = null;
                foreach (Apparel apparel in this.tempApparelList)
                {
                    if (lastDef == null || lastDef != apparel.def)
                    {
                        lastDef = apparel.def;
                        if (!this.StoredApparelLookup.TryGetValue(lastDef, out l))
                        {
                            l = new LinkedList<Apparel>();
                            this.StoredApparelLookup.Add(lastDef, l);
                        }
                    }
                    l.AddLast(apparel);
                }
            }

            if (this.tempApparelList != null && 
                (Scribe.mode == LoadSaveMode.Saving ||
                 Scribe.mode == LoadSaveMode.PostLoadInit))
            {
                this.tempApparelList.Clear();
                this.tempApparelList = null;
            }
        }

        public override bool Equals(object obj)
        {
            return obj != null && this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return this.UniqueId;
        }

        public Apparel FindBetterApparel(ref float baseApparelScore, Pawn pawn, Outfit currentOutfit, Building dresser)
        {
            Apparel apparel = null;
            foreach (LinkedList<Apparel> ll in this.StoredApparelLookup.Values)
            {
                foreach (Apparel apparel in ll)
                {
                    if (currentOutfit.filter.Allows(apparel))
                    {
                        if (!apparel.IsForbidden(pawn))
                        {
                            float newApparelScore = JobGiver_OptimizeApparel.ApparelScoreGain(pawn, apparel);
                            if (newApparelScore >= 0.05f && newApparelScore >= baseApparelScore)
                            {
                                if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
                                {
                                    if (ReservationUtility.CanReserveAndReach(pawn, dresser, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1))
                                    {
                                        apparel = apparel;
                                        baseApparelScore = newApparelScore;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return apparel;
        }
    }
}
