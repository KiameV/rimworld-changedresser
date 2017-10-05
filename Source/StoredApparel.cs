using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using System;
using System.Text;

namespace ChangeDresser
{
    class StoredApparel
    {
        private static int ID = 0;
        private readonly int UniqueId;

        private readonly Building_Dresser Dresser;
        private Dictionary<Def, LinkedList<Apparel>> StoredApparelLookup = new Dictionary<Def, LinkedList<Apparel>>();
        //public bool FilterApparel { get; set; }
        public int Count
        {
            get
            {
                int count = 0;
                foreach (LinkedList<Apparel> ll in this.StoredApparelLookup.Values)
                {
                    count += ll.Count;
                }
                return count;
            }
        }

        public StoredApparel(Building_Dresser dresser)
        {
            this.UniqueId = ID;
            ++ID;

            this.Dresser = dresser;

            //this.FilterApparel = true;
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
            if (apparel != null)
            {
                LinkedList<Apparel> l;
                if (!this.StoredApparelLookup.TryGetValue(apparel.def, out l))
                {
                    l = new LinkedList<Apparel>();
                    this.StoredApparelLookup.Add(apparel.def, l);
                }
                this.AddApparelToLinkedList(apparel, l);
                //this.FilterApparel = true;
            }
        }

        private void AddApparelToLinkedList(Apparel apparel, LinkedList<Apparel> l)
        {
            if (!l.Contains(apparel))
            {
                float score = JobGiver_OptimizeApparel.ApparelScoreRaw(null, apparel);
                for (LinkedListNode<Apparel> n = l.First; n != null; n = n.Next)
                {
                    float nScore = JobGiver_OptimizeApparel.ApparelScoreRaw(null, apparel);
                    if (score >= nScore)
                    {
                        l.AddBefore(n, apparel);
                        return;
                    }
                    else if (score < nScore)
                    {
                        l.AddAfter(n, apparel);
                        return;
                    }
                }
                l.AddLast(apparel);
            }

            /*
#if TRACE
            Log.Message("Start StoredApparel.AddApparelToLinkedList");
            Log.Warning("Apparel: " + apparel.Label);
            StringBuilder sb = new StringBuilder("LinkedList: ");
            foreach (Apparel a in l)
            {
                sb.Append(a.LabelShort);
                sb.Append(", ");
            }
            Log.Warning(sb.ToString());
#endif
            QualityCategory q;
            if (!apparel.TryGetQuality(out q))
            {
#if TRACE
                Log.Message("AddLast - quality not found");
#endif
                l.AddLast(apparel);
            }
            else
            {
#if TRACE
                Log.Message("HP: " + apparel.HitPoints + " HPMax: " + apparel.MaxHitPoints);
#endif
                int hpPercent = apparel.HitPoints / apparel.MaxHitPoints;
                for (LinkedListNode<Apparel> n = l.First; n != null; n = n.Next)
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
#if TRACE
            Log.Message("End StoredApparel.AddApparelToLinkedList");
#endif
            */
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
            //this.FilterApparel = false;
        }

        public bool TryRemoveApparel(ThingDef def, out Apparel apparel)
        {
            LinkedList<Apparel> l;
            if (this.StoredApparelLookup.TryGetValue(def, out l))
            {
                if (l.Count > 0)
                {
                    apparel = l.First.Value;
                    l.RemoveFirst();
                    return true;
                }
            }
            apparel = null;
            return false;
        }

        public bool TryRemoveBestApparel(ThingDef def, out Apparel apparel)
        {
            LinkedList<Apparel> l;
            if (this.StoredApparelLookup.TryGetValue(def, out l))
            {
                if (l.Count > 0)
                {
                    apparel = l.First.Value;
                    l.RemoveFirst();
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
                return l.Remove(apparel);
            }
            return false;
        }

        internal bool TryRemoveBestApparel(ThingDef def, ThingFilter filter, out Apparel apparel)
        {
#if DEBUG
            Log.Message(Environment.NewLine + "Start StoredApparel.TryRemoveBestApperal Def: " + def.label);
#endif
            LinkedList<Apparel> l;
            if (this.StoredApparelLookup.TryGetValue(def, out l))
            {
#if DEBUG
                Log.Warning("Apparel List found Count: " + l.Count);
#endif
                for (LinkedListNode<Apparel> n = l.First; n != null; n = n.Next)
                {
#if DEBUG
                    Log.Warning("Apparel " + n.Value.Label);
#endif
                    try
                    {
                        if (filter.Allows(n.Value))
                        {
                            l.Remove(n);
                            apparel = n.Value;
#if DEBUG
                            Log.Warning("Start StoredApparel.TryRemoveBestApperal Return: True Apparel:" + apparel.LabelShort + Environment.NewLine);
#endif
                            return true;
                        }
#if DEBUG
                        else
                            Log.Warning("Filter rejected");
#endif
                    }
                    catch
                    {
                        Log.Error("catch");
                    }
                }
            }
            apparel = null;
#if DEBUG
            Log.Message("End StoredApparel.TryRemoveBestApperal Return: False" + Environment.NewLine);
#endif
            return false;
        }

        public List<Apparel> GetFilteredApparel(ThingFilter filter)
        {
            List<Apparel> toRemove = new List<Apparel>(0);
            foreach (LinkedList<Apparel> ll in this.StoredApparelLookup.Values)
            {
                for(LinkedListNode<Apparel> n = ll.First; n != null; n = n.Next)
                {
                    if (!this.Dresser.settings.filter.Allows(n.Value))
                    {
                        toRemove.Add(n.Value);
                    }
                }
            }
            return toRemove;
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
            Apparel betterApparel = null;
            foreach (LinkedList<Apparel> ll in this.StoredApparelLookup.Values)
            {
                foreach (Apparel apparel in ll)
                {
                    if (currentOutfit.filter.Allows(apparel))
                    {
                        if (!apparel.IsForbidden(pawn))
                        {
                            float gain = JobGiver_OptimizeApparel.ApparelScoreGain(pawn, apparel);
                            if (gain >= 0.05f && gain > baseApparelScore)
                            {
                                if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
                                {
                                    if (ReservationUtility.CanReserveAndReach(pawn, dresser, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1))
                                    {
                                        betterApparel = apparel;
                                        baseApparelScore = gain;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return betterApparel;
        }
    }
}
