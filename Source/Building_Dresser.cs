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
        public readonly JobDef wearApparelFromStorageJobDef = DefDatabase<JobDef>.GetNamed("WearApparelFromStorage", true);

        public static readonly List<CurrentEditorEnum> SupportedEditors = new List<CurrentEditorEnum>(3);

        static Building_Dresser()
        {
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserApparelColor);
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserHair);
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserBody);
        }

        public const StoragePriority DefaultStoragePriority = StoragePriority.Low;

        private List<Apparel> toredApparel = new List<Apparel>();
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
            try
            {
                this.Dispose();
                base.Destroy(mode);
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.Destroy\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }
        }

        public override void Discard()
        {
            try
            {
                this.Dispose();
                base.Discard();
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.Discard\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }
        }

        public override void DeSpawn()
        {
            try
            {
                this.Dispose();
                base.DeSpawn();
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.DeSpawn\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }
        }

        private void Dispose()
        {
            try
            {
                if (this.toredApparel != null)
                {
                    DropApparel(this.toredApparel);
                    this.toredApparel.Clear();
                }
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.Dispose\n" + 
                    e.GetType().Name + " " + e.Message + "\n" + 
                    e.StackTrace);
            }
}

        private void DropApparel(List<Apparel> apparel, bool makeForbidden = true)
        {
            try
            {
                if (apparel != null)
                {
                    foreach (Apparel a in apparel)
                    {
                        this.DropThing(a, makeForbidden);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.DropApparel\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }
        }

        private System.Random random = null;
        private void DropThing(Thing a, bool makeForbidden = true)
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
                    if (this.random == null)
                        this.random = new System.Random();
                    int dir = this.random.Next(2);
                    int amount = this.random.Next(2);
                    if (amount == 0)
                        amount = -1;
                    if (dir == 0)
                        pos.x = pos.x + amount;
                    else
                        pos.z = pos.z + amount;
                    a.Position = pos;
                }
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.DropApparel\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }
        }

        public override void Notify_ReceivedThing(Thing newItem)
        {
            if (!(newItem is Apparel))
            {
                DropThing(newItem);
                return;
            }

            Apparel a = (Apparel)newItem;
            //if (!WorldComp.IsAssignedApparel(a))
            //{
                base.Notify_ReceivedThing(newItem);
                if (!this.StoredApparel.Contains(a))
                {
                    if (newItem.Spawned)
                        newItem.DeSpawn();
                    this.StoredApparel.Add((Apparel)newItem);
                }
            //}
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref this.toredApparel, "storedApparel", LookMode.Deep, new object[0]);

            /*if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (sets != null && sets.Count > 0)
                {
                    foreach (StoredApparelSet set in sets)
                    {
                        set.SetParentDresser(this);
                        StoredApparelContainer.AddApparelSet(set);
                    }
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
                    {
                        foreach (StoredApparelSet set in sets)
                        {
                            set.SetParentDresser(this);
                            StoredApparelContainer.AddApparelSet(set);
                        }
                    }
                }
            }*/
        }

        public override string GetInspectString()
        {
            this.Tick();
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.Append("\n");
            sb.Append("ChangeDresser.StoragePriority".Translate());
            sb.Append(": ");
            sb.Append(("StoragePriority" + base.settings.Priority).Translate());
            sb.Append("\n");
            sb.Append("ChangeDresser.ApparelCount".Translate());
            sb.Append(": ");
            sb.Append(this.StoredApparel.Count);
            return sb.ToString();
        }

        public List<Apparel> StoredApparel
        {
            get
            {
                if (this.toredApparel == null)
                    this.toredApparel = new List<Apparel>();
                return this.toredApparel;
            }
            set
            {
                this.toredApparel = value;
                if (this.toredApparel == null)
                    this.toredApparel = new List<Apparel>();
            }
        }

        public void Remove(Apparel a, bool forbidden = true)
        {
            try
            {
                this.DropThing(a, forbidden);
                this.StoredApparel.Remove(a);
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.Remove\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }
        }

        public void RemoveNoDrop(Apparel a)
        {
            this.StoredApparel.Remove(a);
        }

        private readonly Stopwatch stopWatch = new Stopwatch();
        public override void TickLong()
        {
            try
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
                                this.DropThing(a, false);
                                this.StoredApparel.RemoveAt(i);
                            }
                        }
                        this.stopWatch.Reset();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.TickLong\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
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
            if (Settings.ShowBodyChange)
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeBody".Translate(),
                    delegate
                    {
                        Job job = new Job(this.changeBodyJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }
            list.Add(new FloatMenuOption(
                "ChangeDresser.StoreApparel".Translate(),
                delegate
                {
                    Job job = new Job(this.storeApparelJobDef, this);
                    pawn.jobs.TryTakeOrderedJob(job);
                }));
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

            int groupKey = 987767542;

            Command_Action a = new Command_Action();
            a.icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/manageapparel", true);
            a.defaultDesc = "ChangeDresser.AssignApparelDesc".Translate();
            a.defaultLabel = "ChangeDresser.AssignApparel".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action = delegate { Find.WindowStack.Add(new UI.StorageGroupUI(this)); };
            a.groupKey = groupKey;
            l.Add(a);

            a = new Command_Action();
            a.icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/empty", true);
            a.defaultDesc = "ChangeDresser.EmptyDesc".Translate();
            a.defaultLabel = "ChangeDresser.Empty".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action = 
                delegate
                {
                    this.DropApparel(this.StoredApparel, false);
                    this.StoredApparel.Clear();
                };
            a.groupKey = groupKey + 1;
            l.Add(a);

            return SaveStorageSettingsUtil.SaveStorageSettingsGizmoUtil.AddSaveLoadGizmos(l, SaveStorageSettingsUtil.SaveTypeEnum.Apparel_Management, this.settings.filter);
        }
    }
}