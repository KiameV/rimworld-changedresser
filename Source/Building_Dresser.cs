using ChangeDresser.UI.Enums;
using ChangeDresser.UI.Util;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Verse;
using Verse.AI;

namespace ChangeDresser
{
    public class Building_Dresser : Building_Storage//, IStoreSettingsParent
    {
        public const long THIRTY_SECONDS = TimeSpan.TicksPerMinute / 2;

        public readonly JobDef changeApparelColorJobDef = DefDatabase<JobDef>.GetNamed("ChangeApparelColor", true);
        public readonly JobDef changeHairStyleJobDef = DefDatabase<JobDef>.GetNamed("ChangeHairStyle", true);
        public readonly JobDef changeBodyJobDef = DefDatabase<JobDef>.GetNamed("ChangeBody", true);
        public readonly JobDef storeApparelJobDef = DefDatabase<JobDef>.GetNamed("StoreApparel", true);
        public readonly JobDef wearApparelFromStorageJobDef = DefDatabase<JobDef>.GetNamed("WearApparelFromStorage", true);

        public static JobDef WEAR_APPAREL_FROM_DRESSER_JOB_DEF { get; private set; }

        public static readonly List<CurrentEditorEnum> SupportedEditors = new List<CurrentEditorEnum>(3);

        public const StoragePriority DefaultStoragePriority = StoragePriority.Low;

        private readonly StoredApparel StoredApparel;
        private readonly Stopwatch stopWatch = new Stopwatch();

        private Map CurrentMap { get; set; }

        private bool includeInTradeDeals = true;
        public bool IncludeInTradeDeals { get { return this.includeInTradeDeals; } }

        static Building_Dresser()
        {
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserApparelColor);
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserHair);
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserBody);
        }

        public Building_Dresser()
        {
            this.StoredApparel = new StoredApparel(this);
            this.stopWatch.Start();

            WEAR_APPAREL_FROM_DRESSER_JOB_DEF = this.wearApparelFromStorageJobDef;
        }

        public void AddApparel(Apparel a)
        {
            if (a != null && a.Spawned)
            {
                a.DeSpawn();
            }
            this.StoredApparel.AddApparel(a);
        }

        internal bool TryRemoveApparel(ThingDef def, out Apparel apparel)
        {
            return this.StoredApparel.TryRemoveApparel(def, out apparel);
        }

        public bool TryRemoveBestApparel(ThingDef def, ThingFilter filter, out Apparel apparel)
        {
            return this.StoredApparel.TryRemoveBestApparel(def, filter, out apparel);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.CurrentMap = map;
            WorldComp.AddDresser(this);
            
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

        public override void DeSpawn()
        {
            try
            {
                WorldComp.RemoveDesser(this);
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
                if (this.StoredApparel != null)
                {
                    this.DropApparel(this.StoredApparel.Apparel);
                    this.StoredApparel.Clear();
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

        private void DropApparel(IEnumerable<Apparel> apparel, bool makeForbidden = true)
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

        public IEnumerable<Thing> EmptyOnTop()
        {
            IEnumerable<Thing> things = this.StoredApparel.Empty<Thing>();
            foreach (Thing t in things)
            {
                if (!t.Spawned)
                {
                    Thing ouThing;
                    if (!t.Spawned)
                    {
                        GenThing.TryDropAndSetForbidden(t, base.Position, this.CurrentMap, ThingPlaceMode.Near, out ouThing, false);
                        if (!t.Spawned)
                        {
                            GenPlace.TryPlaceThing(t, base.Position, this.CurrentMap, ThingPlaceMode.Near);
                        }
                    }

                    if (!t.Spawned)
                    {
                        this.AddApparel(t as Apparel);
                    }
                    else
                    {
                        this.Map.thingGrid.Deregister(t);
                        t.Position = this.Position;
                        this.Map.thingGrid.Register(t);
                    }
                }
            }
            return things;
        }

        public void HandleThingsOnTop()
        {
#if TRADE_DEBUG
            Log.Warning("Start ChangeDresser.HandleThingsOnTop for " + this.Label + " Spawned: " + this.Spawned);
#endif
            if (this.Spawned)
            {
                foreach (Thing t in base.Map.thingGrid.ThingsAt(this.Position))
                {
#if TRADE_DEBUG
                    Log.Warning("ChangeDresser.HandleThingsOnTop - Thing " + t?.Label);
#endif
                    if (t != null && t != this)
                    {
                        if (t is Apparel)
                        {
                            this.AddApparel((Apparel)t);
                        }
                        else
                        {
                            IntVec3 p = t.Position;
                            p.x = p.x + 1;
                            t.Position = p;
                            Log.Warning("Moving " + t.Label);
                        }
                    }
                }
            }
#if TRADE_DEBUG
            Log.Warning("End ChangeDresser.HandleThingsOnTop");
#endif
        }

        private Random random = null;
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
            base.Notify_ReceivedThing(a);
            if (!this.StoredApparel.Contains(a))
            {
                if (newItem.Spawned)
                {
                    newItem.DeSpawn();
                }
                this.StoredApparel.AddApparel(a);
            }
        }

        private List<Apparel> tempApparelList = null;
        public override void ExposeData()
        {
#if DEBUG
            Log.Warning(Environment.NewLine + "Start Building_Dresser.ExposeData mode: " + Scribe.mode);
#endif
            base.ExposeData();

            //bool useInLookup = this.UseInApparelLookup;
            //Scribe_Values.Look(ref useInLookup, "useInApparelLookup", false, false);
            //this.UseInApparelLookup = useInLookup;

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.tempApparelList = new List<Apparel>(this.StoredApparel.Apparel);
            }

#if DEBUG
            Log.Warning(" Scribe_Collections.Look tempApparelList");
#endif
            Scribe_Collections.Look(ref this.tempApparelList, "apparel", LookMode.Deep, new object[0]);
            Scribe_Values.Look(ref this.includeInTradeDeals, "includeInTradeDeals", true);
#if DEBUG
            if (this.tempApparelList != null)
                Log.Warning(" tempApparelList Count: " + this.tempApparelList.Count);
            else
                Log.Warning(" StempApparelList is null");
#endif
            if (this.tempApparelList != null &&
                Scribe.mode == LoadSaveMode.PostLoadInit)
            {
#if DEBUG
                Log.Warning(" tempApparelList != null && PostLoadInit");
#endif
                foreach (Apparel apparel in this.tempApparelList)
                {
                    if (apparel != null)
                    {
                        this.StoredApparel.AddApparel(apparel);
                    }
                }
            }

            if (this.tempApparelList != null &&
                (Scribe.mode == LoadSaveMode.Saving ||
                 Scribe.mode == LoadSaveMode.PostLoadInit))
            {
#if DEBUG
                StringBuilder sb = new StringBuilder(" Saving or PostLoadInit - Count: " + this.StoredApparel.Count);
                foreach (Apparel a in this.StoredApparel.Apparel)
                {
                    sb.Append(", ");
                    sb.Append(a.LabelShort);
                }
                Log.Warning(sb.ToString());
#endif
                this.tempApparelList.Clear();
                this.tempApparelList = null;
            }

#if DEBUG
            Log.Message("End Building_Dresser.ExposeData" + Environment.NewLine);
#endif
        }

        public override string GetInspectString()
        {
            //this.Tick();
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.Append("\n");
            sb.Append("ChangeDresser.StoragePriority".Translate());
            sb.Append(": ");
            sb.Append(("StoragePriority" + base.settings.Priority).Translate());
            sb.Append("\n");
            sb.Append("ChangeDresser.ApparelCount".Translate());
            sb.Append(": ");
            sb.Append(this.Count);
            sb.Append("\n");
            sb.Append("ChangeDresser.IncludeInTradeDeals".Translate());
            sb.Append(": ");
            sb.Append(this.includeInTradeDeals.ToString());
            return sb.ToString();
        }

        public IEnumerable<Apparel> Apparel
        {
            get
            {
                return this.StoredApparel.Apparel;
            }
        }

        public int Count { get { return this.StoredApparel.Count; } }

        public void Remove(Apparel a, bool forbidden = true)
        {
            try
            {
                if (this.StoredApparel.Contains(a))
                {
                    this.DropThing(a, forbidden);
                    this.StoredApparel.RemoveApparel(a);
                }
#if DEBUG
                else
                {
                    Log.Error("Request to Remove " + a.Label + " failed. " + this.Label + " did not contain that apparel.");
                }
#endif
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.Remove\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }
        }

        public bool RemoveNoDrop(Apparel a)
        {
            return this.StoredApparel.RemoveApparel(a);
        }

        public override void TickLong()
        {
            if (this.Spawned)
            {
                if (base.Map != null)
                {
                    // Fix for an issue where apparel will appear on top of the dresser even though it's already stored inside
                    this.HandleThingsOnTop();
                }
                if (stopWatch.ElapsedTicks > THIRTY_SECONDS)
                {
                    WorldComp.SortDressersToUse();
                    /*if (this.StoredApparel.FilterApparel)
                    {*/
                    //this.StoredApparel.FilterApparel = false;
                    try
                    {
                        List<Apparel> removed = this.StoredApparel.GetFilteredApparel(this.settings.filter);
                        foreach (Apparel a in removed)
                        {
                            if (this.StoredApparel.RemoveApparel(a))
                            {
                                if (!WorldComp.AddApparel(a))
                                {
                                    this.DropThing(a, false);
                                }
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
                    //}
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
            if (!AlienRaceUtil.IsAlien(pawn))
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeHair".Translate(),
                    delegate
                    {
                        Job job = new Job(this.changeHairStyleJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }
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

        public Apparel FindBetterApparel(ref float baseApparelScore, Pawn pawn, Outfit currentOutfit)
        {
            return this.StoredApparel.FindBetterApparel(ref baseApparelScore, pawn, currentOutfit, this);
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
            a.icon = WidgetUtil.manageapparelTexture;
            a.defaultDesc = "ChangeDresser.ManageApparelDesc".Translate();
            a.defaultLabel = "ChangeDresser.ManageApparel".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action = delegate { Find.WindowStack.Add(new UI.StorageUI(this, null)); };
            a.groupKey = groupKey;
            l.Add(a);

            a = new Command_Action();
            a.icon = WidgetUtil.assignweaponsTexture;
            a.defaultDesc = "ChangeDresser.AssignOutfitsDesc".Translate();
            a.defaultLabel = "ChangeDresser.AssignOutfits".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action = delegate { Find.WindowStack.Add(new UI.AssignOutfitUI(this)); };
            a.groupKey = groupKey + 1;
            l.Add(a);

            a = new Command_Action();
            a.icon = WidgetUtil.emptyTexture;
            a.defaultDesc = "ChangeDresser.EmptyDesc".Translate();
            a.defaultLabel = "ChangeDresser.Empty".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action = 
                delegate
                {
                    this.DropApparel(this.StoredApparel.Empty<Apparel>(), false);
                    this.StoredApparel.Clear();
                };
            a.groupKey = groupKey + 2;
            l.Add(a);

            a = new Command_Action();
            if (this.includeInTradeDeals)
            {
                a.icon = WidgetUtil.yesSellTexture;
            }
            else
            {
                a.icon = WidgetUtil.noSellTexture;
            }
            a.defaultDesc = "ChangeDresser.IncludeInTradeDealsDesc".Translate();
            a.defaultLabel = "ChangeDresser.IncludeInTradeDeals".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action =
                delegate
                {
                    this.includeInTradeDeals = !this.includeInTradeDeals;
                };
            a.groupKey = groupKey + 3;
            l.Add(a);

            return SaveStorageSettingsUtil.SaveStorageSettingsGizmoUtil.AddSaveLoadGizmos(l, SaveStorageSettingsUtil.SaveTypeEnum.Apparel_Management, this.settings.filter);
        }
    }
}