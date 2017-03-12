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
using ChangeDresser.UI.DTO.StorageDTOs;
using ChangeDresser.UI.Enums;
using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Verse;
using Verse.AI;

namespace ChangeDresser
{
    public class Building_Dresser : Building
    {
        private JobDef changeApparelColorJobDef = DefDatabase<JobDef>.GetNamed("ChangeApparelColor", true);
        private JobDef changeHairStyleJobDef = DefDatabase<JobDef>.GetNamed("ChangeHairStyle", true);
        private JobDef changeBodyJobDef = DefDatabase<JobDef>.GetNamed("ChangeBody", true);
        private JobDef storeApparelJobDef = DefDatabase<JobDef>.GetNamed("StoreApparel", true);
        private JobDef wearApparelGroupJobDef = DefDatabase<JobDef>.GetNamed("WearApparelGroup", true);

        public readonly List<CurrentEditorEnum> SupportedEditors = new List<CurrentEditorEnum>();

        private List<Apparel> storedApparel = new List<Apparel>();
        private List<StorageGroupDTO> storageGroups = new List<StorageGroupDTO>();

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);

            SupportedEditors.Add(CurrentEditorEnum.ApparelColor);
            SupportedEditors.Add(CurrentEditorEnum.Body);
            SupportedEditors.Add(CurrentEditorEnum.Hair);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Collections.LookList(ref this.storedApparel, "storedApparel", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList(ref this.storageGroups, "storageGroups", LookMode.Deep, new object[0]);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            //sb.AppendLine("Stored Items: ".Translate() + ": " + -10f.ToStringTemperature("F0"));
            return sb.ToString();
        }

        public List<Apparel> StoredApparel
        {
            get
            {
                if (this.storedApparel == null)
                    this.storedApparel = new List<Apparel>();
                return this.storedApparel;
            }
            set
            {
                this.storedApparel = value;
                if (this.storedApparel == null)
                    this.storedApparel = new List<Apparel>();
            }
        }

        public List<StorageGroupDTO> StorageGroups
        {
            get
            {
                if (this.storageGroups == null)
                    this.storageGroups = new List<StorageGroupDTO>();
                return this.storageGroups;
            }
        }

        public void Remove(StorageGroupDTO storageGroupDto)
        {
            for (int i = 0; i < this.storageGroups.Count; ++i)
            {
                if (this.storageGroups[i] == storageGroupDto)
                {
                    this.storageGroups.RemoveAt(i);
                    break;
                }
            }
        }

        public bool TryGetStorageGroup(Pawn pawn, string apparelGroupName, out StorageGroupDTO storageGroupDTO)
        {
            foreach (StorageGroupDTO dto in this.storageGroups)
            {
                if (dto.Name.Equals(apparelGroupName))
                {
                    if (dto.CanPawnAccess(pawn))
                    {
                        storageGroupDTO = dto;
                        return true;
                    }
                }
            }
            storageGroupDTO = null;
            return false;
        }

        [DebuggerHidden]
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if (pawn.apparel.WornApparel.Count > 0)
            {
                list.Add(new FloatMenuOption(
                    "Change outfit's colors",
                    delegate
                    {
                        Job job = new Job(this.changeApparelColorJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            list.Add(new FloatMenuOption(
                "Change hair style",
                delegate
                {
                    Job job = new Job(this.changeHairStyleJobDef, this);
                    pawn.jobs.TryTakeOrderedJob(job);
                }));

            list.Add(new FloatMenuOption(
                "Change body attributes",
                delegate
                {
                    Job job = new Job(this.changeBodyJobDef, this);
                    pawn.jobs.TryTakeOrderedJob(job);
                }));

            list.Add(new FloatMenuOption(
                "Store Apparel",
                delegate
                {
                    Job job = new Job(this.storeApparelJobDef, this);
                    pawn.jobs.TryTakeOrderedJob(job);
                }));

            bool isWearingSet = false;
            foreach (StorageGroupDTO dto in this.StorageGroups)
            {
                isWearingSet = dto.IsPawnWearing(pawn);
                if (isWearingSet)
                {
                    list.Add(new FloatMenuOption(
                    "Unwear Group \"" + dto.Name + "\"",
                    delegate
                    {
                        Job job = new SwapApparelJob(this.wearApparelGroupJobDef, this, dto.Name);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
                    break;
                }
            }

            if (!isWearingSet)
            {
                foreach (StorageGroupDTO dto in this.StorageGroups)
                {
                    if (dto.CanPawnAccess(pawn))
                    {
                        list.Add(new FloatMenuOption(
                            "Wear Group \"" + dto.Name + "\"",
                            delegate
                            {
                                Job job = new SwapApparelJob(this.wearApparelGroupJobDef, this, dto.Name);
                                pawn.jobs.TryTakeOrderedJob(job);
                            }));
                    }
                }
            }
            return list;
        }
    }
}