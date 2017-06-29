using System;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser.StoredApparel
{
    class StoredApparelContainer
    {
        private static LinkedList<StoredApparelSet> storedApparelSets = new LinkedList<StoredApparelSet>();

        public static void AddApparelSet(StoredApparelSet set)
        {
            if (set != null)
            {
                storedApparelSets.AddLast(set);
            }
        }

        public static void AddApparelSets(IEnumerable<StoredApparelSet> sets)
        {
            if (sets != null)
                foreach (StoredApparelSet set in sets)
                    if (set != null)
                        storedApparelSets.AddLast(set);
        }

        internal static void Clear()
        {
            storedApparelSets.Clear();
        }

        internal static void Initialize(Dictionary<string, Pawn> pawnIdToPawn)
        {
            foreach (StoredApparelSet set in storedApparelSets)
                set.Initialize(pawnIdToPawn);
        }

        public static bool DoesPawnHaveApparelSets(Pawn pawn)
        {
            if (pawn != null)
            {
                foreach (StoredApparelSet set in storedApparelSets)
                    if (set.IsOwnedBy(pawn))
                        return true;
            }
            return false;
        }

        public static IEnumerable<StoredApparelSet> GetAllApparelSets()
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
                List<StoredApparelSet> l = new List<StoredApparelSet>();
                foreach (StoredApparelSet set in storedApparelSets)
                    if (set.ParentDresserId.Equals(dresserId))
                        l.Add(set);
                return l;
            }
            return new List<StoredApparelSet>(0);
        }

        public static void RemoveApparelSet(StoredApparelSet set)
        {
            storedApparelSets.Remove(set);
        }

        public static List<StoredApparelSet> RemoveApparelSets(Building_Dresser dresser)
        {
            if (dresser != null)
            {
                List<StoredApparelSet> l = new List<StoredApparelSet>();
                for (LinkedListNode<StoredApparelSet> n = storedApparelSets.First; n != null; n = n.Next)
                {
                    if (n.Value != null && dresser.ThingID.Equals(n.Value.ParentDresserId))
                    {
                        l.Add(n.Value);
                        storedApparelSets.Remove(n);
                    }
                }
                return l;
            }
            return new List<StoredApparelSet>(0);
        }

        public static bool TryGetWornApparelSet(Pawn pawn, out StoredApparelSet set)
        {
            if (pawn != null)
            {
                int i = 0;
                foreach (StoredApparelSet s in storedApparelSets)
                {
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
                foreach (StoredApparelSet s in storedApparelSets)
                    if (s.IsOwnedBy(pawn) && s.SwitchForBattle)
                    {
                        set = s;
                        return true;
                    }
            }
            set = null;
            return false;
        }
    }
}
