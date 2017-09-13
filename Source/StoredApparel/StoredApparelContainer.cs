using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ChangeDresser.StoredApparel
{
    public static class StoredApparelContainer
    {
        public static Dictionary<int, StorageForPawn> StoredApparelSets = new Dictionary<int, StorageForPawn>();

        public static void Add(StorageForPawn s)
        {
            if (!StoredApparelSets.ContainsKey(s.Pawn.thingIDNumber))
            {
                StoredApparelSets.Add(s.Pawn.thingIDNumber, s);
            }
            else
            {
                Log.Warning("StoredApparelContainer StoredApparelSets already have an instance for " + s.Pawn.NameStringShort);
            }
        }

        public static void AddApparelSet(StoredApparelSet set)
        {
            if (set != null)
            {
#if DEBUG
                Log.Warning("SAC.AddApparelSet: Add " + set.Name + ", IsTemp: " + set.IsTemp);
#endif
                StorageForPawn s;
                if (!StoredApparelSets.TryGetValue(set.Pawn.thingIDNumber, out s))
                {
#if DEBUG
                    Log.Warning("SAC.AddApparelSet: No previous sets for pawn");
#endif
                    s = new StorageForPawn();
                    s.Pawn = set.Pawn;
                    StoredApparelSets.Add(set.Pawn.thingIDNumber, s);
                }

                s.Add(set);
            }
        }

        public static void Clear()
        {
            foreach (StorageForPawn s in StoredApparelSets.Values)
            {
                s.Clear();
            }
            StoredApparelSets.Clear();
        }

        public static bool IsApparelUsedInSets(Pawn pawn, Apparel apparel)
        {
            IEnumerable<StoredApparelSet> sets;
            if (TryGetApparelSets(pawn, out sets))
            {
                foreach (StoredApparelSet s in sets)
                {
                    if (s.IsApparelUsed(apparel))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool DoesPawnHaveApparelSets(Pawn pawn)
        {
            StorageForPawn s;
            if (StoredApparelSets.TryGetValue(pawn.thingIDNumber, out s))
            {
                return s.HasSets();
            }
            return false;
        }

        public static bool RemoveApparelSet(Pawn pawn, StoredApparelSet set, Building_Dresser dresser)
        {
            StorageForPawn s;
            if (StoredApparelSets.TryGetValue(pawn.thingIDNumber, out s))
            {
                return s.Remove(set, dresser);
            }
            return false;
        }

        public static bool TryGetApparelSets(Pawn pawn, out IEnumerable<StoredApparelSet> sets)
        {
            StorageForPawn s;
            if (StoredApparelSets.TryGetValue(pawn.thingIDNumber, out s))
            {
                sets = s.GetApparelSets();
                return true;
            }
            sets = null;
            return false;
        }

        public static bool TryGetWornApparelSet(Pawn pawn, out StoredApparelSet set)
        {
            StorageForPawn s;
            if (StoredApparelSets.TryGetValue(pawn.thingIDNumber, out s))
            {
                return s.TryGetWornApparelSet(out set);
            }
            set = null;
            return false;
        }

        public static bool TryGetBestApparelSet(Pawn pawn, bool forBattle, out StoredApparelSet set)
        {
            StorageForPawn s;
            if (StoredApparelSets.TryGetValue(pawn.thingIDNumber, out s))
            {
                return s.TryGetBestApparelSet(forBattle, out set);
            }
            set = null;
            return false;
        }

        public static void Remove(Pawn pawn, Apparel apparel, Building_Dresser dresser)
        {
            StorageForPawn s;
            if (StoredApparelSets.TryGetValue(pawn.thingIDNumber, out s))
            {
                s.Remove(apparel, dresser);
            }
        }

        public static bool TryGetAssignedApparel(Pawn pawn, out IEnumerable<Apparel> apparel)
        {
            StorageForPawn s;
            if (StoredApparelSets.TryGetValue(pawn.thingIDNumber, out s))
            {
                apparel = s.GetAssignedApparel();
                return true;
            }
            apparel = null;
            return false;
        }

        public static void Notify_ApparelRemoved(Pawn pawn, Apparel apparel)
        {
            StorageForPawn s;
            if (StoredApparelSets.TryGetValue(pawn.thingIDNumber, out s))
            {
                s.Notify_ApparelRemoved(apparel);
            }
        }
    }
}
