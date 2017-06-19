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
using ChangeDresser.StoredApparel;
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
    public partial class Building_Dresser : Building_Storage, IStoreSettingsParent
    {
        public readonly JobDef changeApparelColorJobDef = DefDatabase<JobDef>.GetNamed("ChangeApparelColor", true);
        public readonly JobDef changeHairStyleJobDef = DefDatabase<JobDef>.GetNamed("ChangeHairStyle", true);
        public readonly JobDef changeBodyJobDef = DefDatabase<JobDef>.GetNamed("ChangeBody", true);
        public readonly JobDef storeApparelJobDef = DefDatabase<JobDef>.GetNamed("StoreApparel", true);
        public readonly JobDef wearApparelGroupJobDef = DefDatabase<JobDef>.GetNamed("WearApparelGroup", true);
        public readonly JobDef wearApparelFromStorageJobDef = DefDatabase<JobDef>.GetNamed("WearApparelFromStorage", true);

        public static readonly List<CurrentEditorEnum> SupportedEditors = new List<CurrentEditorEnum>(3);

        static Building_Dresser()
        {
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserApparelColor);
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserBody);
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserHair);
        }

        public const StoragePriority DefaultStoragePriority = StoragePriority.Low;

        private List<Apparel> storedApparel = new List<Apparel>();
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
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            this.Dispose();
            base.Destroy(mode);
        }

        public override void Discard()
        {
            this.Dispose();
            base.Discard();
        }

        public override void DeSpawn()
        {
            this.Dispose();
            base.DeSpawn();
        }

        private void Dispose()
        {
            if (this.storedApparel != null)
            {
                DropApparel(this.storedApparel);
                this.storedApparel.Clear();
            }

            foreach (StoredApparelSet set in StoredApparelContainer.RemoveApparelSets(this))
            {
                DropApparel(set.Apparel);
            }
        }

        private void DropApparel(List<Apparel> apparel)
        {
            foreach (Apparel a in apparel)
            {
                this.DropApparel(a);
            }
        }

        private void DropApparel(Thing a, bool makeForbidden = true)
        {
            try
            {
                Thing t;
                if (!a.Spawned)
                {
                    GenThing.TryDropAndSetForbidden(a, base.Position, this.CurrentMap, ThingPlaceMode.Near, out t, makeForbidden);
                    if (!a.Spawned)
                    {
                        GenPlace.TryPlaceThing(a, base.Position, this.CurrentMap, ThingPlaceMode.Near);
                    }
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

        public override void Notify_ReceivedThing(Thing newItem)
        {
            if (!(newItem is Apparel))
            {
                DropApparel(newItem);
                return;
            }

            base.Notify_ReceivedThing(newItem);
            if (!this.StoredApparel.Contains((Apparel)newItem))
            {
                if (newItem.Spawned)
                    newItem.DeSpawn();
                this.storedApparel.Add((Apparel)newItem);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            List<StoredApparelSet> sets = null;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                sets = StoredApparelContainer.GetApparelSets(this);
            }

            Scribe_Collections.Look(ref this.storedApparel, "storedApparel", LookMode.Deep, new object[0]);
            Scribe_Collections.Look(ref sets, "storedApparelSet", LookMode.Deep, new object[0]);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (sets != null && sets.Count > 0)
                {
                    StoredApparelContainer.AddApparelSets(sets);
                }
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Collections.Look(ref groups, "storageGroups", LookMode.Deep, new object[0]);
                if (groups != null && groups.Count > 0)
                {
                    sets = this.GetStoredApparelSetsFromStorageGroupDTOs();
                    groups.Clear();
                    groups = null;
                    if (sets != null && sets.Count > 0)
                        StoredApparelContainer.AddApparelSets(sets);
                }
            }
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
            sb.Append(this.StoredApparelSets.Count);
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

        public List<StoredApparelSet> StoredApparelSets
        {
            get
            {
                return StoredApparelContainer.GetApparelSets(this);
            }
        }

        public void Remove(Apparel a, bool forbidden = true)
        {
            this.DropApparel(a, forbidden);
            this.StoredApparel.Remove(a);
        }

        public void Remove(StoredApparelSet set)
        {
            StoredApparelContainer.RemoveApparelSet(set);
        }

        private readonly Stopwatch stopWatch = new Stopwatch();
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

            StoredApparelSet wornSet;
            if (StoredApparelContainer.TryGetWornApparelSet(pawn, out wornSet))
            {
                list.Add(new FloatMenuOption(
                "ChangeDresser.UnwearGroup".Translate() + " \"" + wornSet.Name + "\"",
                delegate
                {
                    Job job = new SwapApparelJob(this.wearApparelGroupJobDef, this, wornSet.Name);
                    pawn.jobs.TryTakeOrderedJob(job);
                }));
            }
            else
            {
                foreach (StoredApparelSet set in StoredApparelContainer.GetApparelSets(this))
                {
                    if (set.IsOwnedBy(pawn))
                    {
                        list.Add(new FloatMenuOption(
                            "ChangeDresser.WearGroup".Translate() + " \"" + set.Name + "\"",
                            delegate
                            {
                                Job job = new SwapApparelJob(this.wearApparelGroupJobDef, this, set.Name);
                                pawn.jobs.TryTakeOrderedJob(job);
                            }));
                    }
                }
            }
            return list;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerable<Gizmo> enumerables = base.GetGizmos();

            List<Gizmo> l;
            if (enumerables != null)
                l = new List<Gizmo>(enumerables);
            else
                l = new List<Gizmo>(1);

            Command_Action a = new Command_Action();
            a.icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/manageapparel", true);
            a.defaultDesc = "ChangeDresser.ManageApparelDesc".Translate();
            a.defaultLabel = "ChangeDresser.ManageApparel".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action = delegate { Find.WindowStack.Add(new UI.StorageUI(this, null, true)); };
            a.groupKey = 887767542;
            l.Add(a);

            return l;
        }
    }
}