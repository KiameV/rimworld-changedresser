/*
 * MIT License
 * 
 * Copyright (c) [2017] [Travis Offtermatt]
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser.UI.DTO.StorageDTOs
{
    public class StorageGroupDTO : IExposable
    {
        private List<Apparel> apparelList = new List<Apparel>();
        private string name = "";
        private string restrictToPawnId = "";
        private string restrictToPawnName = "";
        private bool forceSwitchBattle = false;
        private string isBeingWornById = "";
        private string isBeingWornByName = "";

        public bool HasName()
        {
            return this.name.Length > 0;
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (value == null ||
                    (value != null && value.Trim().Length == 0))
                {
                    this.name = "";
                }
                else
                {
                    this.name = value.Trim();
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<string>(ref this.name, "name", "", false);
            Scribe_Values.LookValue<string>(ref this.restrictToPawnId, "restrictToPawnId", "", false);
            Scribe_Values.LookValue<string>(ref this.restrictToPawnName, "restrictToPawnName", "", false);
            Scribe_Values.LookValue<bool>(ref this.forceSwitchBattle, "forceSwitchBattle", false, false);
            Scribe_Values.LookValue<string>(ref this.isBeingWornById, "isBeingWornById", "", false);
            Scribe_Values.LookValue<string>(ref this.isBeingWornByName, "isBeingWornByName", "", false);
            Scribe_Collections.LookList(ref this.apparelList, "apparelList", LookMode.Deep, new object[0]);
        }

        public void SwapWith(Pawn pawn)
        {
            List<Apparel> wasWearing = new List<Apparel>(pawn.apparel.WornApparel);
            foreach (Apparel a in wasWearing)
            {
                pawn.apparel.Remove(a);
            }

            foreach (Apparel a in this.apparelList)
            {
                pawn.apparel.Wear(a);
            }

            this.apparelList.Clear();
            this.apparelList = wasWearing;
            if (this.IsBeingWorn)
            {
                this.isBeingWornById = "";
                this.isBeingWornByName = "";
            }
            else
            {
                this.isBeingWornById = pawn.ThingID;
                this.isBeingWornByName = pawn.Name.ToStringShort;
            }
        }

        public bool CanPawnAccess(Pawn pawn)
        {
            if (!this.IsRestricted)
                return true;
            if (pawn.ThingID.Equals(this.restrictToPawnId))
                return true;
            return false;
        }

        public void RestrictToPawn(Pawn pawn)
        {
            this.restrictToPawnId = pawn.ThingID;
            this.restrictToPawnName = pawn.Name.ToStringShort;
            this.isBeingWornById = "";
            this.isBeingWornByName = "";
        }

        public List<Apparel> Apparel { get { return this.apparelList; } }

        public bool IsRestricted
        {
            get
            {
                if (this.restrictToPawnId == "")
                    return false;
                if (this.restrictToPawnId.Length == 0)
                    return false;
                return true;
            }
        }

        public string RestrictToPawnId
        {
            get { return this.restrictToPawnId; }
            set { this.restrictToPawnId = value; }
        }

        public string RestrictToPawnName
        {
            get { return this.restrictToPawnName; }
            set { this.restrictToPawnName = value; }
        }

        public bool ForceSwitchBattle
        {
            get { return this.forceSwitchBattle; }
            set { this.forceSwitchBattle = value; }
        }

        public bool IsBeingWorn
        {
            get
            {
                if (this.isBeingWornById == "")
                    return false;
                if (this.isBeingWornById.Length == 0)
                    return false;
                return true;
            }
        }

        public void ClearWornBy()
        {
            this.isBeingWornById = "";
            this.isBeingWornByName = "";
        }

        public bool IsPawnWearing(Pawn pawn)
        {
            return pawn.ThingID.Equals(this.isBeingWornById);
        }

        public void Unrestrict()
        {
            this.restrictToPawnId = "";
            this.restrictToPawnName = "";
        }
    }
}
