using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Text;

namespace ChangeDresser.StoredApparel
{
    public class StoredApparelSet : IExposable
    {
        /// <summary>
        /// Used as a temp value until the Pawns are loaded and Initialize is called
        /// </summary>
        private string isBeingWornByPawnId = "";
        /// <summary>
        /// Used as a temp value until the Pawns are loaded and Initialize is called
        /// </summary>
        private string isOwnedByPawnId = "";

        private string parentDresserId;
        private Pawn owner = null;
        private Pawn isBeingWornBy = null;
        private List<Apparel> assignedApparel = new List<Apparel>();
        private List<string> forcedApparelIds = new List<string>();

        public string Name = "";
        public bool SwitchForBattle = false;

        public List<Apparel> Apparel { get { return this.assignedApparel; } }
        public List<string> ForcedApparelIds { get { return this.forcedApparelIds; } }
        public void ClearRestriction() { this.owner = null; }
        public bool IsBeingWorn { get { return this.isBeingWornBy != null; } }
        public bool IsRestricted { get { return HasOwner; } }
        public string OwnerName { get { return this.owner.Name.ToStringShort; } }
        public bool HasOwner { get { return this.owner != null; } }
        public bool HasName { get { return this.Name != null && this.Name.Trim().Length > 0; } }
        public Pawn Owner { get { return this.owner; } }
        public string ParentDresserId { get { return this.parentDresserId; } }

        public StoredApparelSet()
        {

        }

        public StoredApparelSet(Building_Dresser parentDresser)
        {
            this.parentDresserId = parentDresser.ThingID;
        }

        /// <summary>
        /// For use by depricated code only
        /// </summary>
        public StoredApparelSet(Building_Dresser parentDresser, string isOwnedByPawnId, string isBeingWornByPawnId)
        {
            this.parentDresserId = parentDresser.ThingID;
            this.isOwnedByPawnId = isOwnedByPawnId;
            this.isBeingWornByPawnId = isBeingWornByPawnId;
        }

        internal void Initialize(Dictionary<string, Pawn> pawnIdToPawn)
        {
            if (this.isOwnedByPawnId != null && this.isOwnedByPawnId.Trim().Length > 0)
            {
                if (!pawnIdToPawn.TryGetValue(this.isOwnedByPawnId, out this.owner))
                {
                    Log.Warning("Unable to find owner [" + this.isOwnedByPawnId + "] for Storage Group [" + this.Name + "]");
                    this.owner = null;
                }
            }
            this.isOwnedByPawnId = null;

            if (this.isBeingWornByPawnId != null && this.isBeingWornByPawnId.Trim().Length > 0)
            {
                if (!pawnIdToPawn.TryGetValue(this.isBeingWornByPawnId, out this.isBeingWornBy))
                {
                    Log.Warning("Unable to find owner [" + this.isBeingWornByPawnId + "] for Storage Group [" + this.Name + "]");
                    this.isBeingWornBy = null;
                }
            }
            this.isBeingWornByPawnId = null;
        }

        public bool IsBeingWornBy(Pawn pawn)
        {
            if (pawn != null && this.isBeingWornBy != null &&
                pawn.ThingID.Equals(this.isBeingWornBy))
            {
                this.isBeingWornBy.Name = pawn.Name;
                return true;
            }
            return false;
        }

        public bool IsOwnedBy(Pawn pawn)
        {
            if (this.owner != null && pawn != null)
                return this.owner.ThingID.Equals(pawn.ThingID);
            return false;
        }

        public void SetOwner(Pawn pawn)
        {
            this.owner = pawn;
        }

        public void SetApparel(List<Apparel> apparel)
        {
            this.assignedApparel = apparel;
        }

        public void SetForcedApparel(List<string> forcedApparelIds)
        {
            this.forcedApparelIds = forcedApparelIds;
        }

        public void SetBeingWornBy(Pawn pawn)
        {
            this.isBeingWornBy = pawn;
        }

        public void SwapApparel(Pawn pawn)
        {
            if (pawn == null)
            {
                Log.Warning("ChangeDresser.StoredApparelSet.SwapApparel: Pawn should never be null here");
                return;
            }

            // Remove apparel from pawn
            List<Apparel> wasWearing = new List<Apparel>(pawn.apparel.WornApparel);
            List<Apparel> wasForced = new List<Apparel>(pawn.outfits.forcedHandler.ForcedApparel);
            foreach (Apparel a in wasWearing)
            {
                pawn.apparel.Remove(a);
            }

            // Dress the pawn
            foreach (Apparel a in this.assignedApparel)
            {
                pawn.apparel.Wear(a);
                if (this.forcedApparelIds.Contains(a.ThingID))
                {
                    pawn.outfits.forcedHandler.ForcedApparel.Add(a);
                }
            }

            // Replace what this StoredApparelSet holds - now holds what the pawn was wearing
            this.assignedApparel.Clear();
            this.assignedApparel = wasWearing;
            this.forcedApparelIds.Clear();
            foreach (Apparel a in wasForced)
            {
                this.forcedApparelIds.Add(a.ThingID);
            }

            // Switch the IsBeingWornBy state
            if (this.isBeingWornBy == null)
                this.isBeingWornBy = pawn;
            else
                this.isBeingWornBy = null;
        }

        public void ExposeData()
        {
            bool isOldSave = false;

            Scribe_Values.Look<string>(ref this.parentDresserId, "parentDresser", "", false);
            Scribe_Values.Look<string>(ref this.Name, "name", "", false);
            
            if (Scribe.mode == LoadSaveMode.Saving && this.HasOwner)
                this.isOwnedByPawnId = this.owner.ThingID;
            Scribe_Values.Look<string>(ref this.isOwnedByPawnId, "ownerId", null, false);
            if (Scribe.mode == LoadSaveMode.LoadingVars && this.isOwnedByPawnId == null)
            {
                // Old value
                Scribe_Values.Look<string>(ref this.isOwnedByPawnId, "restrictToPawnId", null, false);
                if (this.isOwnedByPawnId != null)
                {
                    isOldSave = true;
                }
            }
            
            if (Scribe.mode == LoadSaveMode.Saving && this.IsBeingWorn)
                this.isBeingWornByPawnId = this.isBeingWornBy.ThingID;
            Scribe_Values.Look<string>(ref this.isBeingWornByPawnId, "isBeingWornById", null, false);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.isOwnedByPawnId = "";
                this.isBeingWornByPawnId = "";
            }

            if (isOldSave)
            {
                Scribe_Values.Look<bool>(ref this.SwitchForBattle, "forceSwitchBattle", false, false);
            }
            else
            {
                Scribe_Values.Look<bool>(ref this.SwitchForBattle, "switchForBattle", false, false);
            }
            Scribe_Collections.Look(ref this.assignedApparel, "apparelList", LookMode.Deep, new object[0]);
            Scribe_Collections.Look(ref this.forcedApparelIds, "forcedApparel", LookMode.Value, new object[0]);
        }

        public void ClearWornBy()
        {
            this.isBeingWornBy = null;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
                return this.GetHashCode() == obj.GetHashCode();
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(this.Name);
            sb.Append(" Has Owner: ");
            if (this.HasOwner)
                sb.Append(this.owner.Name.ToStringShort);
            else
                sb.Append("no");

            sb.Append(" BeingWorn: ");
            if (this.IsBeingWorn)
                sb.Append(this.isBeingWornBy.Name.ToStringShort);
            else
                sb.Append("no");

            sb.Append(" Apparel:");
            foreach (Apparel a in this.Apparel)
            {
                sb.Append(" ");
                sb.Append(a.ThingID);
            }
            return sb.ToString();
        }
    }
}
