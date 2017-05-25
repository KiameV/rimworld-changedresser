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
using ChangeDresser.DresserJobDriver;
using ChangeDresser.UI.DTO.StorageDTOs;
using ChangeDresser.UI.Enums;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Verse;
using Verse.AI;

namespace ChangeDresser
{
    public class Building_Dresser : Building_Storage, IStoreSettingsParent
    {
        public readonly JobDef changeApparelColorJobDef = DefDatabase<JobDef>.GetNamed("ChangeApparelColor", true);
        public readonly JobDef changeHairStyleJobDef = DefDatabase<JobDef>.GetNamed("ChangeHairStyle", true);
        public readonly JobDef changeBodyJobDef = DefDatabase<JobDef>.GetNamed("ChangeBody", true);
        public readonly JobDef storeApparelJobDef = DefDatabase<JobDef>.GetNamed("StoreApparel", true);
        public readonly JobDef wearApparelGroupJobDef = DefDatabase<JobDef>.GetNamed("WearApparelGroup", true);
        public readonly JobDef wearApparelFromStorageJobDef = DefDatabase<JobDef>.GetNamed("WearApparelFromStorage", true);

        public readonly List<CurrentEditorEnum> SupportedEditors = new List<CurrentEditorEnum>();

        public static StoragePriority DefaultStoragePriority = StoragePriority.Low;

        private List<Apparel> storedApparel = new List<Apparel>();
        private List<StorageGroupDTO> storageGroups = new List<StorageGroupDTO>();
        private Map CurrentMap { get; set; }
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.CurrentMap = map;

            if (settings == null)
            {
                base.settings = new StorageSettings(this);
                base.settings.CopyFrom(this.def.building.defaultStorageSettings);
                base.settings.filter.SetDisallowAll();
            }

            if (SupportedEditors.Count == 0)
            {
                SupportedEditors.Add(CurrentEditorEnum.ChangeDresserApparelColor);
                SupportedEditors.Add(CurrentEditorEnum.ChangeDresserBody);
                SupportedEditors.Add(CurrentEditorEnum.ChangeDresserHair);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            this.Dispose();
        }

        public override void Discard()
        {
            base.Discard();
            this.Dispose();
        }

        public override void DeSpawn()
        {
            base.DeSpawn();
            this.Dispose();
        }

        public override void Notify_ReceivedThing(Thing newItem)
        {
            base.Notify_ReceivedThing(newItem);
            if (!this.StoredApparel.Contains((Apparel)newItem))
            {
                if (newItem.Spawned)
                    newItem.DeSpawn();
                this.storedApparel.Add((Apparel)newItem);
            }
        }

        private void Dispose()
        {
            if (this.storedApparel != null)
            {
                DropApparel(this.storedApparel);
                this.storedApparel.Clear();
            }

            if (this.storageGroups != null)
            {
                foreach (StorageGroupDTO dto in this.storageGroups)
                {
                    DropApparel(dto.Apparel);
                    dto.Delete();
                }
                this.storageGroups.Clear();
            }
        }

        private void DropApparel(List<Apparel> apparel)
        {
            foreach (Apparel a in apparel)
            {
                this.DropApparel(a);
            }
        }

        private void DropApparel(Apparel a, bool makeForbidden = true)
        {
            try
            {
                Thing t;
                GenThing.TryDropAndSetForbidden(a, base.Position, this.CurrentMap, ThingPlaceMode.Near, out t, makeForbidden);
                if (!a.Spawned)
                {
                    GenPlace.TryPlaceThing(a, base.Position, this.CurrentMap, ThingPlaceMode.Near);
                }
                if (a.Position.Equals(base.Position))
                {
                    IntVec3 pos = a.Position;
                    pos.z = pos.z - 1;
                    a.Position = pos;
                }
            }
            catch (Exception e)
            {
                Log.Error("ChangeDresser:Building_Dresser.DropApparel error while dropping apparel. " + e.GetType() + " " + e.Message);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                BattleApparelGroupDTO.ClearBattleStorageCache();
            }

            Scribe_Collections.Look(ref this.storedApparel, "storedApparel", LookMode.Deep, new object[0]);
            Scribe_Collections.Look(ref this.storageGroups, "storageGroups", LookMode.Deep, new object[0]);
        }

        public override string GetInspectString()
        {
            this.Tick();
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.Append("ChangeDresser.StoragePriority".Translate());
            sb.Append(": ");
            sb.Append(("StoragePriority" + base.settings.Priority).Translate());
            sb.Append("\n");
            sb.Append("ChangeDresser.ApparelCount".Translate());
            sb.Append(": ");
            sb.Append(this.StoredApparel.Count);
            sb.Append("\n");
            sb.Append("ChangeDresser.ApparelGroupCount".Translate());
            sb.Append(": ");
            sb.Append(this.StorageGroups.Count);
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

        public void Remove(Apparel a, bool forbidden = true)
        {
            this.DropApparel(a, forbidden);
            this.StoredApparel.Remove(a);
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

        private Stopwatch stopWatch = new Stopwatch();
        public override void TickLong()
        {
            if (!this.stopWatch.IsRunning)
                this.stopWatch.Start();
            else
            {
                // Do this every minute
                if (this.stopWatch.ElapsedMilliseconds > 60000)
                {
                    for (int i = this.StoredApparel.Count - 1; i >= 0; --i)
                    {
                        Apparel a = this.StoredApparel[i];
                        if (!this.settings.filter.Allows(a))
                        {
                            this.DropApparel(a, false);
                            this.StoredApparel.RemoveAt(i);
                        }
                    }
                    this.stopWatch.Reset();
                }
            }
        }
        
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if (pawn.apparel.WornApparel.Count > 0)
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeApparelColors".Translate(),
                    delegate
                    {
                        Job job = new Job(this.changeApparelColorJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            list.Add(new FloatMenuOption(
                "ChangeDresser.ChangeHair".Translate(),
                delegate
                {
                    Job job = new Job(this.changeHairStyleJobDef, this);
                    pawn.jobs.TryTakeOrderedJob(job);
                }));

            list.Add(new FloatMenuOption(
                "ChangeDresser.ChangeBody".Translate(),
                delegate
                {
                    Job job = new Job(this.changeBodyJobDef, this);
                    pawn.jobs.TryTakeOrderedJob(job);
                }));

            list.Add(new FloatMenuOption(
                "ChangeDresser.StoreApparel".Translate(),
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
                    "ChangeDresser.UnwearGroup".Translate() + " \"" + dto.Name + "\"",
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
                            "ChangeDresser.WearGroup".Translate() + " \"" + dto.Name + "\"",
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