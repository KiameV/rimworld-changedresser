using RimWorld;
using System.Collections.Generic;
using Verse;
using System;
using System.Text;

namespace ChangeDresser.StoredApparel
{
    public class StorageForPawn : IExposable
    {
        public Pawn Pawn;
        private List<StoredApparelSet> ApparelSets = new List<StoredApparelSet>();
        private Dictionary<int, Apparel> ApparelForPawn = new Dictionary<int, Apparel>();
        private StoredApparelSet TempStoredApparelSet = null;

        public void Clear()
        {
            this.ApparelSets.Clear();
            this.ApparelForPawn.Clear();
        }

        public bool HasSets()
        {
            return this.ApparelSets.Count > 0;
        }
        
        public void ExposeData()
        {
#if DEBUG
            Log.Warning("SFP: Scribe Mode: " + Scribe.mode.ToString());
#endif
            List<Apparel> l = null;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
#if DEBUG
                Log.Warning("SFP: Pre-Save ApparelForPawn.Count: " + this.ApparelForPawn.Count);
#endif
                l = new List<Apparel>(this.ApparelForPawn.Count);
                foreach (Apparel assigned in this.ApparelForPawn.Values)
                {
                    bool include = true;
                    // Remove the clothing which will be deep-saved by the pawn here
                    foreach (Apparel worn in this.Pawn.apparel.WornApparel)
                    {
                        if (worn.thingIDNumber == assigned.thingIDNumber)
                        {
                            include = false;
                            break;
                        }
                    }
#if DEBUG
                    Log.Warning("SFP: Include " + assigned.ThingID + ": " + include);
#endif
                    if (include)
                    {
                        l.Add(assigned);
                    }
                }
#if DEBUG
                StringBuilder sb = new StringBuilder("SFP: Saving ApparelForGroup: ");
                foreach (Apparel a in l)
                {
                    sb.Append(a.ThingID);
                    sb.Append(", ");
                }
                Log.Warning(sb.ToString());
#endif
            }

            Scribe_References.Look(ref this.Pawn, "pawn");
            Scribe_Collections.Look(ref l, "apparelForGroups", LookMode.Deep, new object[0]);
            Scribe_Deep.Look(ref this.TempStoredApparelSet, "tempStoredApparelSet", null, false);
            Scribe_Collections.Look(ref this.ApparelSets, "apparelSets", LookMode.Deep, new object[0]);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
#if DEBUG
                Log.Warning("SFP: LoadingVars");
                Log.Warning("SFP: ApparelForPawn " + (string)((ApparelForPawn == null) ? "null" : "not null"));
                Log.Warning("SFP: l " + (string)((l == null) ? "null" : "not null"));
#endif
                this.ApparelForPawn.Clear();
                foreach (Apparel a in l)
                {
                    this.Add(a);
                }
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.ApparelSets == null)
                    this.ApparelSets = new List<StoredApparelSet>(0);
                if (this.ApparelForPawn == null)
                    this.ApparelForPawn = new Dictionary<int, Apparel>();
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach (Apparel a in this.Pawn.apparel.WornApparel)
                {
                    if (StoredApparelContainer.IsApparelUsedInSets(this.Pawn, a))
                    {
                        this.Add(a);
                    }
                }
#if DEBUG
                Log.Warning("SFP: Post-Load ApparelForPawn.Count: " + this.ApparelForPawn.Count);
#endif
            }
        }

        public void Add(Apparel apparel)
        {
            if (!this.ApparelForPawn.ContainsKey(apparel.thingIDNumber))
            {
#if DEBUG
                Log.Warning("SAC.Add(apparel): Add new apparel " + apparel.ThingID);
#endif
                this.ApparelForPawn.Add(apparel.thingIDNumber, apparel);
            }
        }

        public void Add(StoredApparelSet set)
        {
            if (set.IsTemp)
            {
                this.TempStoredApparelSet = set;
            }
            else
            {
                bool found = false;
                foreach (StoredApparelSet s in this.ApparelSets)
                {
                    found = s.Equals(set);
                    if (found)
                    {
#if DEBUG
                        Log.Warning("SAC.Add(StoredApparelSet): " + set.Name + " == " + s.Name);
#endif
                        break;
                    }
#if DEBUG
                    else
                    {
                        Log.Warning("SAC.Add(StoredApparelSet): " + set.Name + " != " + s.Name);
                    }
#endif
                }
                if (!found)
                {
#if DEBUG
                    Log.Warning("SAC.Add(StoredApparelSet): Adding " + set.Name);
#endif
                    ApparelSets.Add(set);
                }

                foreach (Apparel a in set.AssignedApparel)
                {
                    this.Add(a);
                }
            }
        }

        public bool Remove(StoredApparelSet set, Building_Dresser dresser)
        {
            if (ApparelSets.Remove(set))
            {
                foreach (Apparel a in set.AssignedApparel)
                {
                    this.Remove(a, dresser);
                }
                return true;
            }
            return false;
        }

        public void Remove(Apparel a, Building_Dresser dresser)
        {
            foreach (StoredApparelSet s in ApparelSets)
            {
                if (s.IsApparelUsed(a))
                {
                    return;
                }
            }

            this.ApparelForPawn.Remove(a.thingIDNumber);
            dresser.StoredApparel.Add(a);
        }

        public IEnumerable<StoredApparelSet> GetApparelSets()
        {
            return this.ApparelSets;
        }

        public bool TryGetWornApparelSet(out StoredApparelSet set)
        {
            foreach (StoredApparelSet s in this.ApparelSets)
            {
#if DEBUG
                Log.Warning("SFP.TryGetWornApparelSet: " + s.Name + " isBeingWorn: " + s.IsBeingWorn);
#endif
                if (s.IsBeingWorn)
                {
#if DEBUG
                    Log.Warning("SFP.TryGetWornApparelSet: Use " + s.Name);
#endif
                    set = s;
                    return true;
                }
            }
            set = null;
            return false;
        }

        public bool TryGetBestApparelSet(bool forBattle, out StoredApparelSet set)
        {
#if DEBUG
            Log.Warning("SFP.TryGetBestApparelSet: forBattle: " + forBattle);
#endif
            if (this.TempStoredApparelSet != null && 
                forBattle == this.TempStoredApparelSet.ForBattle)
            {
#if DEBUG
                Log.Warning("SFP.TryGetBestApparelSet: Use TempStoredApparelSet");
#endif
                set = this.TempStoredApparelSet;
                this.TempStoredApparelSet = null;
                return true;
            }

            set = null;
            foreach (StoredApparelSet s in this.ApparelSets)
            {
                if (forBattle == s.ForBattle)
                {
                    set = s;
                    if (s.SwitchedFrom)
                        return true;
                }
            }
            if (set != null)
            {
#if DEBUG
                Log.Warning("SFP.TryGetBestApparelSet: Use " + set.Name);
#endif
                return true;
            }
#if DEBUG
            Log.Warning("SFP.TryGetBestApparelSet: None Found.");
#endif
            return false;
        }

        public IEnumerable<Apparel> GetAssignedApparel()
        {
            return this.ApparelForPawn.Values;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(" | ");

            sb.Append("Pawn: ");
            if (this.Pawn == null)
                sb.Append("null");
            else
                sb.Append(this.Pawn.NameStringShort);
            sb.Append(" | ");
            
            sb.Append("ApparelSets: ");
            if (this.ApparelSets == null)
                sb.Append("null");
            else
            {
                foreach (StoredApparelSet s in ApparelSets)
                {
                    sb.Append(s.Name);
                    sb.Append(": ");
                    foreach (Apparel a in s.AssignedApparel)
                    {
                        sb.Append(a.LabelShort);
                        sb.Append(", ");
                    }
                }
            }
            sb.Append(" | ");

            sb.Append("ApparelForPawn: ");
            if (this.ApparelForPawn == null)
                sb.Append("null");
            else
            {
                foreach (Apparel a in ApparelForPawn.Values)
                {
                    sb.Append(a.LabelShort);
                    sb.Append(", ");
                }
            }
            sb.Append(" | ");
            return sb.ToString();
        }
    }
}
