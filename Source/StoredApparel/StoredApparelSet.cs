using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Text;
using System;

namespace ChangeDresser.StoredApparel
{
    public class StoredApparelSet : IExposable
    {
        private static int ID = 0;

        private readonly int uniqueId;

        public bool IsTemp = false;

        public string Name;

        public bool ForBattle = false;
        public bool SwitchedFrom = false;
        public bool IsBeingWorn = false;

        public Pawn Pawn;
        
        private List<Apparel> Apparel = new List<Apparel>();

        private List<int> ForcedApparel = null;

        public void SetForcedApparel(List<Apparel> forcedApparel)
        {
            this.ForcedApparel = new List<int>(forcedApparel.Count);
            foreach (Apparel a in forcedApparel)
            {
                this.ForcedApparel.Add(a.thingIDNumber);
            }
        }
        public bool WasForced(Apparel apparel)
        {
            if (this.ForcedApparel != null && this.ForcedApparel.Count > 0)
                return this.ForcedApparel.Contains(apparel.thingIDNumber);
            return false;
        }

        public IEnumerable<Apparel> AssignedApparel
        {
            get { return this.Apparel; }
            set
            {
                if (this.Apparel != null)
                    this.Apparel.Clear();
                this.Apparel = new List<Apparel>(value);
            }
        }

        public string TexPath
        {
            get
            {
                if (this.Apparel != null && this.Apparel.Count > 0)
                {
                    return this.Apparel[0].def.graphicData.texPath;
                }
                return null;
            }
        }

        public StoredApparelSet()
        {
            uniqueId = ID;
            ++ID;
        }

        public void Notify_ApparelChange(Apparel apparel)
        {
            this.Apparel.Clear();
            this.Apparel = new List<Apparel>(this.Pawn.apparel.WornApparel);
        }

        public bool IsApparelUsed(Apparel apparel)
        {
            if (apparel != null)
            {
                foreach (Apparel a in this.Apparel)
                {
                    if (a.thingIDNumber == apparel.thingIDNumber)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref this.Name, "name", "", false);

            Scribe_References.Look(ref this.Pawn, "pawn");
            Scribe_Values.Look<bool>(ref this.ForBattle, "forBattle", false, false);
            Scribe_Values.Look<bool>(ref this.IsBeingWorn, "isBeingWorn", false, false);
            Scribe_Values.Look<bool>(ref this.SwitchedFrom, "switchedFrom", false, false);
            Scribe_Values.Look<bool>(ref this.IsTemp, "isTemp", false, false);
            
            if (IsTemp)
                Scribe_Collections.Look(ref this.Apparel, "apparel", LookMode.Deep, new object[0]);
            else
                Scribe_Collections.Look(ref this.Apparel, "apparel", LookMode.Reference, new object[0]);
            Scribe_Collections.Look(ref this.ForcedApparel, "forcedApparel", LookMode.Deep, new object[0]);
        }

        public override bool Equals(object obj)
        {
            if (obj is StoredApparelSet)
            {
                return this.uniqueId == ((StoredApparelSet)obj).uniqueId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.uniqueId;
        }

        public void Add(Apparel apparel)
        {
            this.Apparel.Add(apparel);
        }

        public void Remove(Apparel apparel)
        {
            this.Apparel.Remove(apparel);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append("uniqueId: ");
            sb.Append(this.uniqueId.ToString());
            sb.Append(" | ");

            sb.Append("Name: ");
            sb.Append(this.Name);
            sb.Append(" | ");

            sb.Append("IsTemp: ");
            sb.Append(this.IsTemp.ToString());
            sb.Append(" | ");

            sb.Append("ForBattle: ");
            sb.Append(this.ForBattle.ToString());
            sb.Append(" | ");

            sb.Append("SwitchedFrom: ");
            sb.Append(this.SwitchedFrom.ToString());
            sb.Append(" | ");

            sb.Append("IsBeingWorn: ");
            sb.Append(this.IsBeingWorn.ToString());
            sb.Append(" | ");

            sb.Append("Pawn: ");
            if (this.Pawn == null)
                sb.Append("null");
            else
                sb.Append(this.Pawn.NameStringShort);
            sb.Append(" | ");

            sb.Append("Apparel: ");
            if (this.Apparel == null)
                sb.Append("null");
            else
            {
                foreach (Apparel a in this.Apparel)
                {
                    sb.Append(a.LabelShort);
                    sb.Append(", ");
                }
            }
            sb.Append(" | ");

            sb.Append("ForcedApparel: ");
            if (this.ForcedApparel == null)
                sb.Append("null");
            else
            {
                foreach (int i in this.ForcedApparel)
                {
                    sb.Append(i.ToString());
                    sb.Append(", ");
                }
            }
            sb.Append(" | ");
            return sb.ToString();
        }

        public void Notify_ApparelRemoved(Apparel apparel)
        {
            this.Apparel.Remove(apparel);
        }
    }
}
