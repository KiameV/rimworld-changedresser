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
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ChangeDresser.UI.DTO.StorageDTOs
{
    class StorageGroupDTO
    {
        public Pawn Pawn { get; private set; }

        public readonly Building_Dresser Dresser;

        public StorageGroupDTO(Building_Dresser dresser, Pawn pawn)
        {
            this.Dresser = dresser;
            this.Pawn = pawn;

            Log.Warning("StorageGroupDTO: Dresser" + (string)((dresser == null) ? "null" : "instance"));
            Log.Warning("Pawn" + (string)((Pawn == null) ? "null" : "instance"));
            Log.Warning("IsPawnRestricted " + this.IsPawnRestricted);
            Log.Warning("GroupName " + this.GroupName);
            Log.Warning("ForceSwitchBattle " + this.ForceSwitchBattle);
            Log.Warning("StoredApparel Count " + this.StoredApparel.Count);
        }

        public bool IsPawnRestricted
        {
            get
            {
                return this.Pawn.ThingID.Equals(this.Dresser.RestrictToPawnId);
            }
            set
            {
                if (value)
                {
                    this.Dresser.RestrictToPawnId = this.Pawn.ThingID;
                    this.Dresser.RestrictToPawnName = this.Pawn.Name.ToStringShort;
                }
                else
                {
                    this.Dresser.RestrictToPawnId = "";
                }
            }
        }

        public string GroupName
        {
            get { return this.Dresser.StorageGroupName; }
            set { this.Dresser.StorageGroupName = value; }
        }

        public bool ForceSwitchBattle
        {
            get { return this.Dresser.ForceSwitchBattle; }
            set { this.Dresser.ForceSwitchBattle = value; }
        }

        public List<Apparel> StoredApparel
        {
            get { return this.Dresser.StoredApparel; }
            set { this.Dresser.StoredApparel = value; }
        }

        public string RestrictToPawnName
        {
            get { return this.Dresser.RestrictToPawnName; }
        }
    }
}
