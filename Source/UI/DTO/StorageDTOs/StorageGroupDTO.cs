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
using System.Text;
using Verse;

namespace ChangeDresser.UI.DTO.StorageDTOs
{
    public class StorageGroupDTO : IExposable
    {

        private List<Apparel> apparelList = new List<Apparel>();
        private List<bool> forcedApparel = new List<bool>();
        private string name = "";
        private string restrictToPawnId = "";
        private string restrictToPawnName = "";
        private bool forceSwitchBattle = false;
        private string isBeingWornById = "";
        private string isBeingWornByName = "";

        private static int idCount = 0;
        public readonly int Id;

        public StorageGroupDTO()
        {
            this.Id = idCount;
            ++idCount;
#if (CHANGE_DRESSER_DEBUG)
            Log.Message("StorageGroupDTO Id: " + this.Id);
#endif
        }

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
            Scribe_Values.Look<string>(ref this.name, "name", "", false);
            Scribe_Values.Look<string>(ref this.restrictToPawnId, "restrictToPawnId", "", false);
            Scribe_Values.Look<string>(ref this.restrictToPawnName, "restrictToPawnName", "", false);
            Scribe_Values.Look<bool>(ref this.forceSwitchBattle, "forceSwitchBattle", false, false);
            Scribe_Values.Look<string>(ref this.isBeingWornById, "isBeingWornById", "", false);
            Scribe_Values.Look<string>(ref this.isBeingWornByName, "isBeingWornByName", "", false);
            Scribe_Collections.Look(ref this.apparelList, "apparelList", LookMode.Deep, new object[0]);

            List<bool> l = new List<bool>();
            Scribe_Collections.Look(ref l, "forcedApparel", LookMode.Value, new object[0]);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (l == null || l.Count < this.apparelList.Count)
                {
                    this.forcedApparel = new List<bool>(this.apparelList.Count);
                    for (int i = 0; i < this.apparelList.Count; ++i)
                    {
                        this.forcedApparel.Add(false);
                    }
                }
                else
                {
                    this.forcedApparel = new List<bool>(l);
                }
            }

            if (this.ForceSwitchBattle)
            {
                StorageGroupDTO dto;
                if (!BattleApparelGroupDTO.TryGetBattleApparelGroupForPawn(this.restrictToPawnId, out dto))
                {
                    BattleApparelGroupDTO.AddBattleGroup(this);
                }
            }
        }

        public void SwapWith(Pawn pawn)
        {
            List<Apparel> wasWearing = new List<Apparel>(pawn.apparel.WornApparel);
            List<Apparel> wasForced = new List<Apparel>(pawn.outfits.forcedHandler.ForcedApparel);
            foreach (Apparel a in wasWearing)
            {
                pawn.apparel.Remove(a);
            }

            for (int i = 0; i < this.apparelList.Count; ++i)
            {
                Apparel a = this.apparelList[i];
                pawn.apparel.Wear(a);
                if (this.ForcedApparel[i])
                {
                    pawn.outfits.forcedHandler.ForcedApparel.Add(a);
                }
            }

            this.apparelList.Clear();
            this.apparelList = wasWearing;

            this.forcedApparel.Clear();
            for (int i = 0; i < wasWearing.Count; ++i)
            {
                bool forced = false;
                if (wasForced.Contains(wasWearing[i]))
                {
                    forced = true;
                }
                this.ForcedApparel.Add(forced);
            }

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
            this.restrictToPawnName = pawn.Name.ToStringShort;

            if (!pawn.ThingID.Equals(this.restrictToPawnId))
            {
                if (this.IsRestricted && this.ForceSwitchBattle)
                {
                    BattleApparelGroupDTO.RemoveBattleGroup(this);
                }

                this.restrictToPawnId = pawn.ThingID;
                this.isBeingWornById = "";
                this.isBeingWornByName = "";

                if (this.ForceSwitchBattle)
                {
                    BattleApparelGroupDTO.AddBattleGroup(this);
                }
            }
        }

        public List<Apparel> Apparel { get { return this.apparelList; } }

        public List<bool> ForcedApparel { get { return this.forcedApparel; } }

        public bool IsRestricted
        {
            get
            {
                if (this.restrictToPawnId == null)
                    return false;
                if (this.restrictToPawnId.Length == 0)
                    return false;
                return true;
            }
        }

        public string RestrictToPawnId
        {
            get { return this.restrictToPawnId; }
        }

        public string RestrictToPawnName
        {
            get { return this.restrictToPawnName; }
        }

        public bool ForceSwitchBattle
        {
            get { return this.forceSwitchBattle; }
        }

        public void SetForceSwitchBattle(bool forceSwitchBattle, Pawn pawn)
        {
            if (this.forceSwitchBattle != forceSwitchBattle)
            {
                this.forceSwitchBattle = forceSwitchBattle;
                if (pawn != null && this.forceSwitchBattle)
                {
                    if (!this.CanPawnAccess(pawn))
                    {
                        this.Unrestrict();
                    }
                    this.RestrictToPawn(pawn);
                }

                if (this.IsRestricted && this.forceSwitchBattle)
                {
                    BattleApparelGroupDTO.AddBattleGroup(this);
                }
                else
                {
                    BattleApparelGroupDTO.RemoveBattleGroup(this);
                    this.forceSwitchBattle = false;
                }
            }
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
            if (this.IsRestricted && this.ForceSwitchBattle)
            {
                BattleApparelGroupDTO.RemoveBattleGroup(this);
                this.forceSwitchBattle = false;
            }

            this.restrictToPawnId = "";
            this.restrictToPawnName = "";
        }

        public void Delete()
        {
            BattleApparelGroupDTO.RemoveBattleGroup(this);
            this.forceSwitchBattle = false;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is StorageGroupDTO)
            {
                return ((StorageGroupDTO)obj).Id == this.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("StorageGroupDTO: ");
            sb.Append("] Id: [");
            sb.Append(this.Id);
            sb.Append("name: [");
            sb.Append(this.name);
            sb.Append("] restrictToPawnId: [");
            sb.Append(this.restrictToPawnId);
            sb.Append("] restrictToPawnName: [");
            sb.Append(this.restrictToPawnName);
            sb.Append("] forceSwitchBattle: [");
            sb.Append(this.forceSwitchBattle);
            sb.Append("] isBeingWornById: [");
            sb.Append(this.isBeingWornById);
            sb.Append("] isBeingWornByName: [");
            sb.Append(this.isBeingWornByName);
            sb.Append("]");
            return sb.ToString();
        }
    }
}
