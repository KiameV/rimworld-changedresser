using System;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser.StoredApparel
{
    class StoredApparelContainer
    {
        private static Dictionary<string, List<StoredApparelSet>> storedApparelSets = new Dictionary<string, List<StoredApparelSet>>();

        public static void AddApparelSet(StoredApparelSet set)
        {
            if (set != null)
            {
                lock (storedApparelSets)
                {
                    List<StoredApparelSet> l;
                    if (!storedApparelSets.TryGetValue(set.ParentDresserId, out l))
                    {
                        l = new List<StoredApparelSet>();
                        storedApparelSets.Add(set.ParentDresserId, l);
                    }
                    l.Add(set);
                }
            }
        }

        public static void AddApparelSets(List<StoredApparelSet> sets)
        {
            if (sets != null && sets.Count > 0)
            {
                lock (storedApparelSets)
                {
                    List<StoredApparelSet> l;
                    if (!storedApparelSets.TryGetValue(sets[0].ParentDresserId, out l))
                    {
                        l = new List<StoredApparelSet>();
                        storedApparelSets.Add(sets[0].ParentDresserId, l);
                    }
                    l.AddRange(sets);
                }
            }
        }

        public static string Count
        {
            get
            {
                int c = 0;
                foreach (List<StoredApparelSet> s in storedApparelSets.Values)
                    c += s.Count;
                return storedApparelSets.Count + " " + c;
            }
        }

        internal static void Clear()
        {
            lock (storedApparelSets)
            {
                foreach (List<StoredApparelSet> sets in storedApparelSets.Values)
                    sets.Clear();
                storedApparelSets.Clear();
            }
        }

        internal static void Initialize(Dictionary<string, Pawn> pawnIdToPawn)
        {
            lock (storedApparelSets)
            {
                foreach (List<StoredApparelSet> sets in storedApparelSets.Values)
                    foreach (StoredApparelSet s in sets)
                        s.Initialize(pawnIdToPawn);
            }
        }

        public static bool DoesPawnHaveApparelSets(Pawn pawn)
        {
            if (pawn != null)
            {
                lock (storedApparelSets)
                {
                    foreach (List<StoredApparelSet> sets in storedApparelSets.Values)
                        foreach (StoredApparelSet s in sets)
                            if (s.IsOwnedBy(pawn))
                                return true;
                }
            }
            return false;
        }

        public static IEnumerable<KeyValuePair<string, List<StoredApparelSet>>> GetAllApparelSets()
        {
            return storedApparelSets;
        }

        public static List<StoredApparelSet> GetApparelSets(Building_Dresser dresser)
        {
            if (dresser != null)
                return GetApparelSets(dresser.ThingID);
            return new List<StoredApparelSet>();
        }

        private static List<StoredApparelSet> GetApparelSets(string dresserId)
        {
            if (dresserId != null)
            {
                lock (storedApparelSets)
                {
                    List<StoredApparelSet> l;
                    if (storedApparelSets.TryGetValue(dresserId, out l))
                        return l;
                }
            }
            return new List<StoredApparelSet>(0);
        }

        public static void RemoveApparelSet(StoredApparelSet set)
        {
            lock (storedApparelSets)
            {
                List<StoredApparelSet> sets = GetApparelSets(set.ParentDresserId);
                if (sets != null)
                    sets.Remove(set);
            }
        }

        public static List<StoredApparelSet> RemoveApparelSets(Building_Dresser dresser)
        {
            if (dresser != null)
            {
                lock (storedApparelSets)
                {
                    List<StoredApparelSet> l;
                    if (storedApparelSets.TryGetValue(dresser.ThingID, out l))
                    {
                        if (storedApparelSets.Remove(dresser.ThingID))
                            return l;
                    }
                }
            }
            return new List<StoredApparelSet>(0);
        }

        public static bool TryGetWornApparelSet(Pawn pawn, out StoredApparelSet set)
        {
            if (pawn != null)
            {
                lock (storedApparelSets)
                {
                    foreach (List<StoredApparelSet> sets in storedApparelSets.Values)
                        foreach (StoredApparelSet s in sets)
                            if (s.IsBeingWornBy(pawn))
                            {
                                set = s;
                                return true;
                            }
                }
            }
            set = null;
            return false;
        }

        public static bool TryGetBattleApparelSet(Pawn pawn, out StoredApparelSet set)
        {
            if (pawn != null)
            {
                lock (storedApparelSets)
                {
                    foreach (List<StoredApparelSet> sets in storedApparelSets.Values)
                        foreach (StoredApparelSet s in sets)
                        {
                            if (s.IsOwnedBy(pawn) && s.SwitchForBattle)
                            {
                                set = s;
                                return true;
                            }
                        }
                }
            }
            set = null;
            return false;
        }
    }
}
