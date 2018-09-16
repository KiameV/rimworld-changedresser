using ChangeDresser.UI.Enums;
using ChangeDresser.UI.Util;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;

namespace ChangeDresser
{
    public class Building_Dresser : Building_Storage//, IStoreSettingsParent
    {
        public const long THIRTY_SECONDS = TimeSpan.TicksPerMinute / 2;

        public readonly JobDef changeApparelColorJobDef = DefDatabase<JobDef>.GetNamed("ChangeApparelColor", true);
        public readonly JobDef changeApparelColorByLayerJobDef = DefDatabase<JobDef>.GetNamed("ChangeApparelColorByLayer", true);
        public readonly JobDef changeHairStyleJobDef = DefDatabase<JobDef>.GetNamed("ChangeHairStyle", true);
        public readonly JobDef changeBodyJobDef = DefDatabase<JobDef>.GetNamed("ChangeBody", true);
        public readonly JobDef storeApparelJobDef = DefDatabase<JobDef>.GetNamed("StoreApparel", true);
        public readonly JobDef wearApparelFromStorageJobDef = DefDatabase<JobDef>.GetNamed("WearApparelFromStorage", true);
        public readonly JobDef changeBodyAlienColor = DefDatabase<JobDef>.GetNamed("ChangeBodyAlienColor", true);

        public static JobDef WEAR_APPAREL_FROM_DRESSER_JOB_DEF { get; private set; }

        public const StoragePriority DefaultStoragePriority = StoragePriority.Low;

        public bool AllowAdds { get; set; }

        internal readonly StoredApparel StoredApparel;

        private Map CurrentMap { get; set; }

        private bool includeInTradeDeals = true;
        public bool IncludeInTradeDeals { get { return this.includeInTradeDeals; } }

        public Building_Dresser()
        {
            WEAR_APPAREL_FROM_DRESSER_JOB_DEF = this.wearApparelFromStorageJobDef;

            this.StoredApparel = new StoredApparel();

            this.AllowAdds = true;
        }

        public void AddApparel(Apparel a)
        {
#if DEBUG
            Log.Warning("AddApparel " + a.Label + " Spawned: " + a.Spawned + " IsForbidden: " + a.IsForbidden(Faction.OfPlayer));
#endif
            if (a != null)
            {
                if (this.settings.AllowedToAccept(a))
                {
                    if (a.Spawned)
                    {
                        a.DeSpawn();
                    }
                    this.StoredApparel.AddApparel(a);
                }
                else // Not Allowed
                {
                    if (!WorldComp.AddApparel(a))
                    {
                        if (!a.Spawned)
                        {
                            BuildingUtil.DropThing(a, this, this.CurrentMap, false);
                        }
                    }
                }
            }
        }

        public static IEnumerable<CurrentEditorEnum> GetSupportedEditors(bool isAlien)
        {
            yield return CurrentEditorEnum.ChangeDresserApparelColor;

            if (Settings.IncludeColorByLayer)
            {
                yield return CurrentEditorEnum.ChangeDresserApparelLayerColor;
            }

            yield return CurrentEditorEnum.ChangeDresserHair;

            if (Settings.ShowBodyChange)
            {
                if (isAlien)
                {
                    yield return CurrentEditorEnum.ChangeDresserAlienSkinColor;
                }
                yield return CurrentEditorEnum.ChangeDresserBody;
            }
        }

        internal int GetApparelCount(ThingDef def, ThingFilter ingredientFilter)
        {
            return this.StoredApparel.GetApparelCount(def, ingredientFilter);
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

            foreach (Building_RepairChangeDresser r in BuildingUtil.FindThingsOfTypeNextTo<Building_RepairChangeDresser>(base.Map, base.Position, Settings.RepairAttachmentDistance))
            {
#if DEBUG_REPAIR
                Log.Warning("Adding Dresser " + this.Label + " to " + r.Label);
#endif
                r.AddDresser(this);
            }

            this.UpdatePreviousStorageFilter();
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

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            try
            {
                this.Dispose();
                base.DeSpawn(mode);
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
                this.Empty<Apparel>();
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:Building_Dresser.Dispose\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }

            WorldComp.RemoveDesser(this);
            foreach (Building_RepairChangeDresser r in BuildingUtil.FindThingsOfTypeNextTo<Building_RepairChangeDresser>(this.CurrentMap, base.Position, Settings.RepairAttachmentDistance))
            {
#if DEBUG_REPAIR
                Log.Warning("Removing Dresser " + this.Label + " to " + r.Label);
#endif
                r.RemoveDresser(this);
            }
        }

        private bool DropThing(Thing t, bool makeForbidden = true)
        {
            WorldComp.ApparelColorTracker.RemoveApparel(t as Apparel);
            return BuildingUtil.DropThing(t, this, this.CurrentMap, makeForbidden);
        }

        private void DropApparel<T>(IEnumerable<T> things, bool makeForbidden = true) where T : Thing
        {
            try
            {
                if (things != null)
                {
                    foreach (T t in things)
                    {
                        WorldComp.ApparelColorTracker.RemoveApparel(t as Apparel);
                        this.DropThing(t, makeForbidden);
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

        public void Empty<T>(List<T> removed = null) where T : Thing
        {
            try
            {
                this.AllowAdds = false;

                foreach (LinkedList<Apparel> ll in this.StoredApparel.StoredApparelLookup.Values)
                {
                    foreach (Apparel a in ll)
                    {
                        BuildingUtil.DropThing(a, this, this.CurrentMap, false);
                        if (removed != null)
                        {
                            removed.Add(a as T);
                        }
                    }
                    ll.Clear();
                }
                this.StoredApparel.StoredApparelLookup.Clear();
                WorldComp.ApparelColorTracker.Clear();
            }
            finally
            {
                this.AllowAdds = true;
            }
        }

        internal void ReclaimApparel()
        {
#if DEBUG
            List<Apparel> ll = new List<Apparel>(BuildingUtil.FindThingsOfTypeNextTo<Apparel>(base.Map, base.Position, 1));
            Log.Warning("Apparel found: " + ll.Count);
#endif
            List <Thing> l = BuildingUtil.FindThingsNextTo(base.Map, base.Position, 1);
            if (l.Count > 0)
            {
                foreach (Thing t in l)
                {
                    if (t is Apparel)
                    {
                        WorldComp.AddApparel((Apparel)t);
                    }
                }
                l.Clear();
            }
        }

        public bool TryGetFilteredApparel(Bill bill, ThingFilter filter, out List<Apparel> gotten, bool getOne = false, bool isForMending = false)
        {
            gotten = null;
            foreach (KeyValuePair<ThingDef, LinkedList<Apparel>> kv in this.StoredApparel.StoredApparelLookup)
            {
                if (bill.IsFixedOrAllowedIngredient(kv.Key) && filter.Allows(kv.Key))
                {
                    foreach (Apparel t in kv.Value)
                    {
                        if (bill.IsFixedOrAllowedIngredient(t) && filter.Allows(t))
                        {
                            if (isForMending && t.HitPoints == t.MaxHitPoints)
                            {
                                continue;
                            }

                            if (gotten == null)
                            {
                                gotten = new List<Apparel>();
                            }
                            gotten.Add(t);

                            if (getOne)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return gotten != null;
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
#if DEBUG
                    Log.Warning("ChangeDresser.HandleThingsOnTop - Thing " + t.Label + " Type: " + t.GetType().Name);
#endif
                    if (t != null && t != this && !(t is Blueprint) && !(t is Building))
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

        public override void Notify_ReceivedThing(Thing newItem)
        {
            if (!this.AllowAdds || 
                !(newItem is Apparel))
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
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            if (sb.Length > 0)
                sb.Append(Environment.NewLine);
            sb.Append("ChangeDresser.StoragePriority".Translate());
            sb.Append(": ");
            sb.Append(("StoragePriority" + base.settings.Priority).Translate());
            sb.Append(Environment.NewLine);
            sb.Append("ChangeDresser.ApparelCount".Translate());
            sb.Append(": ");
            sb.Append(this.Count);
            sb.Append(Environment.NewLine);
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

        /// <summary>
        /// DO NOT CHANGE THIS METHOD'S SIGNATURE. IT WILL BREAK MENDING PATCH MOD
        /// </summary>
        public void Remove(Apparel a, bool forbidden = true)
        {
            this.TryRemove(a, forbidden);
        }

        public bool TryRemove(Apparel a, bool forbidden = true)
        {
            try
            {
                this.AllowAdds = false;
                if (this.StoredApparel.RemoveApparel(a))
                {
                    return this.DropThing(a, forbidden);
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
            finally
            {
                this.AllowAdds = true;
            }
            return false;
        }

        public bool RemoveNoDrop(Apparel a)
        {
            WorldComp.ApparelColorTracker.RemoveApparel(a);
            return this.StoredApparel.RemoveApparel(a);
        }

        //private long lastAutoCollect = 0;
        public override void TickLong()
        {
            if (this.Spawned && base.Map != null)
            {
                // Fix for an issue where apparel will appear on top of the dresser even though it's already stored inside
                this.HandleThingsOnTop();
            }

            if (!this.AreStorageSettingsEqual())
            {
                try
                {
                    this.AllowAdds = false;

                    WorldComp.SortDressersToUse();
                    this.UpdatePreviousStorageFilter();

                    List<Apparel> removed = this.StoredApparel.RemoveFilteredApparel(this.settings);
                    foreach (Apparel a in removed)
                    {
                        if (!WorldComp.AddApparel(a))
                        {
                            this.DropThing(a, false);
                        }
                    }
                }
                finally
                {
                    this.AllowAdds = true;
                }
            }

            /*long now = DateTime.Now.Millisecond;
            if (now - this.lastAutoCollect > THIRTY_SECONDS)
            {
                this.lastAutoCollect = now;
                this.ReclaimApparel();
            }*/
        }

#region Float Menu Options
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            bool isAlien = AlienRaceUtil.IsAlien(pawn);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if (pawn.apparel.WornApparel.Count > 0)
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeApparelColors".Translate(),
                    delegate
                    {
                        Job job = new Job(changeApparelColorJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
                if (Settings.IncludeColorByLayer)
                {
                    list.Add(new FloatMenuOption(
                        "ChangeDresser.ChangeApparelColorsByLayer".Translate(),
                        delegate
                        {
                            Job job = new Job(changeApparelColorByLayerJobDef, this);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }));
                }
            }
            if (!isAlien || AlienRaceUtil.HasHair(pawn))
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeHair".Translate(),
                    delegate
                    {
                        Job job = new Job(changeHairStyleJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }
            if (Settings.ShowBodyChange)
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeBody".Translate(),
                    delegate
                    {
                        Job job = new Job(changeBodyJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));

                if (isAlien)
                {
                    list.Add(new FloatMenuOption(
                        "ChangeDresser.ChangeAlienBodyColor".Translate(),
                        delegate
                        {
                            Job job = new Job(changeBodyAlienColor, this);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }));
                }
            }
            list.Add(new FloatMenuOption(
                "ChangeDresser.StoreApparel".Translate(),
                delegate
                {
                    Job job = new Job(storeApparelJobDef, this);
                    pawn.jobs.TryTakeOrderedJob(job);
                }));
            return list;
        }
#endregion

        public bool FindBetterApparel(ref float baseApparelScore, ref Apparel betterApparel, Pawn pawn, Outfit currentOutfit)
        {
            return this.StoredApparel.FindBetterApparel(ref baseApparelScore, ref betterApparel, pawn, currentOutfit, this);
        }

#region Gizmos
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerable<Gizmo> enumerables = base.GetGizmos();

            List<Gizmo> l;
            if (enumerables != null)
                l = new List<Gizmo>(enumerables);
            else
                l = new List<Gizmo>(1);

            int groupKey = this.GetType().Name.GetHashCode();

            Command_Action a = new Command_Action();
            a.icon = WidgetUtil.manageapparelTexture;
            a.defaultDesc = "ChangeDresser.ManageApparelDesc".Translate();
            a.defaultLabel = "ChangeDresser.ManageApparel".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action = delegate { Find.WindowStack.Add(new UI.StorageUI(this, null)); };
            a.groupKey = groupKey;
            ++groupKey;
            l.Add(a);

            a = new Command_Action();
            a.icon = WidgetUtil.assignweaponsTexture;
            a.defaultDesc = "ChangeDresser.AssignOutfitsDesc".Translate();
            a.defaultLabel = "ChangeDresser.AssignOutfits".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action = delegate { Find.WindowStack.Add(new UI.AssignOutfitUI(this)); };
            a.groupKey = groupKey;
            ++groupKey;
            l.Add(a);

            a = new Command_Action();
            a.icon = WidgetUtil.customapparelTexture;
            a.defaultDesc = "ChangeDresser.CustomOutfitsDesc".Translate();
            a.defaultLabel = "ChangeDresser.CustomOutfits".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action = delegate { Find.WindowStack.Add(new UI.CustomOutfitUI(this)); };
            a.groupKey = groupKey;
            ++groupKey;
            l.Add(a);

            a = new Command_Action();
            a.icon = WidgetUtil.emptyTexture;
            a.defaultDesc = "ChangeDresser.EmptyDesc".Translate();
            a.defaultLabel = "ChangeDresser.Empty".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action =
                delegate
                {
                    this.Empty<Apparel>();
                };
            a.groupKey = groupKey;
            ++groupKey;
            l.Add(a);

            a = new Command_Action();
            a.icon = WidgetUtil.collectTexture;
            a.defaultDesc = "ChangeDresser.CollectDesc".Translate();
            a.defaultLabel = "ChangeDresser.Collect".Translate();
            a.activateSound = SoundDef.Named("Click");
            a.action =
                delegate
                {
                    this.ReclaimApparel();
                };
            a.groupKey = groupKey;
            ++groupKey;
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
            a.groupKey = groupKey;
            ++groupKey;
            l.Add(a);

            return SaveStorageSettingsUtil.AddSaveLoadGizmos(l, SaveTypeEnum.Apparel_Management, this.settings.filter);
        }
#endregion

#region ThingFilters
        private ThingFilter previousStorageFilters = new ThingFilter();
        private FieldInfo AllowedDefsFI = typeof(ThingFilter).GetField("allowedDefs", BindingFlags.Instance | BindingFlags.NonPublic);
        protected bool AreStorageSettingsEqual()
        {
            ThingFilter currentFilters = base.settings.filter;
            if (currentFilters.AllowedDefCount != this.previousStorageFilters.AllowedDefCount ||
                currentFilters.AllowedQualityLevels != this.previousStorageFilters.AllowedQualityLevels ||
                currentFilters.AllowedHitPointsPercents != this.previousStorageFilters.AllowedHitPointsPercents)
            {
                return false;
            }

            HashSet<ThingDef> currentAllowed = AllowedDefsFI.GetValue(currentFilters) as HashSet<ThingDef>;
            foreach (ThingDef previousAllowed in AllowedDefsFI.GetValue(this.previousStorageFilters) as HashSet<ThingDef>)
            {
                if (!currentAllowed.Contains(previousAllowed))
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdatePreviousStorageFilter()
        {
            ThingFilter currentFilters = base.settings.filter;

            this.previousStorageFilters.AllowedHitPointsPercents = currentFilters.AllowedHitPointsPercents;
            this.previousStorageFilters.AllowedQualityLevels = currentFilters.AllowedQualityLevels;

            HashSet<ThingDef> previousAllowed = AllowedDefsFI.GetValue(this.previousStorageFilters) as HashSet<ThingDef>;
            previousAllowed.Clear();
            previousAllowed.AddRange(AllowedDefsFI.GetValue(currentFilters) as HashSet<ThingDef>);
        }
        #endregion
    }
}