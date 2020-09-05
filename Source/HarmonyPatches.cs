using ChangeDresser.UI;
using ChangeDresser.UI.Util;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ChangeDresser
{
    [StaticConstructorOnStartup]
    partial class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.changedresser.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static Texture2D GetIcon(ThingDef td)
        {
            Texture2D tex = null;
            if (td.uiIcon != null)
            {
                tex = td.uiIcon;
            }
            else if (td != null && td.graphicData != null && td.graphicData.texPath != null)
            {
                tex = ContentFinder<Texture2D>.Get(td.graphicData.texPath, true);
            }
            else
            {
                tex = null;
            }

            if (tex == null)
            {
                tex = WidgetUtil.noneTexture;
            }

            return tex;
        }

        #region Old SwapApparel
        /*public static void SwapApparel(Pawn pawn, Outfit toWear)
        {
#if SWAP_APPAREL
            Log.Warning(
                "Begin Main.SwapApparel Pawn: " + pawn.Name.ToStringShort + " toWear: " + toWear.label);
#endif
            if (!WorldComp.HasDressers())
            {
                Log.Warning("No Change Dressers found. Apparel will not be swapped.");
                return;
            }

#if TRACE && SWAP_APPAREL
            Log.Message("    Remove Apparel:");
#endif
            // Remove apparel from pawn
            List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);
            foreach (Apparel a in worn)
            {
                if (Settings.KeepForcedApparel && 
                    pawn.outfits.forcedHandler.ForcedApparel.Contains(a))
                {
#if TRACE && SWAP_APPAREL
                    Log.Warning("        Is Forced, Not removing: " + a.LabelShort);
#endif

                    continue;
                }
#if TRACE && SWAP_APPAREL
                Log.Warning("        Removed: " + a.LabelShort);
#endif
                pawn.apparel.Remove(a);
                /*bool handled = false;
                foreach (Building_Dresser d in WorldComp.DressersToUse)
                {
#if DEBUG
                    Log.Warning("  Dresser " + d.Label);
#endif
                    if (d.settings.filter.Allows(a))
                    {
#if DEBUG
                        Log.Warning("   Does Handle");
#endif
                        d.AddApparel(a);
                        handled = true;
                        break;
                    }
#if DEBUG
                    else
                    {
                        Log.Warning("   Does Not Handle");
                    }
#endif
                }* /
                if (!WorldComp.AddApparel(a))
                {
#if TRACE && SWAP_APPAREL
                    Log.Warning("        Apparel " + a.LabelShort + " was not added to any change dresser. Drop on floor");
#endif
                    Thing t;
                    if (!a.Spawned)
                    {
                        GenThing.TryDropAndSetForbidden(a, pawn.Position, pawn.Map, ThingPlaceMode.Near, out t, false);
                        if (!a.Spawned)
                        {
                            GenPlace.TryPlaceThing(a, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                        }
                    }
                }
            }

#if TRACE && SWAP_APPAREL
            Log.Warning("    Previous Outfit was: " + pawn.outfits.CurrentOutfit.label);
#endif
            pawn.outfits.CurrentOutfit = toWear;
#if TRACE && SWAP_APPAREL
            Log.Warning("    Current Outfit is now: " + pawn.outfits.CurrentOutfit.label);
#endif

            typeof(JobGiver_OptimizeApparel)
                .GetField("neededWarmth", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, PawnApparelGenerator.CalculateNeededWarmth(pawn, pawn.Map.Tile, GenLocalDate.Twelfth(pawn)));

            MethodInfo mi = typeof(JobGiver_OptimizeApparel).GetMethod("TryGiveJob", BindingFlags.Instance | BindingFlags.NonPublic);

            JobGiver_OptimizeApparel apparelOptimizer = new JobGiver_OptimizeApparel();
            object[] param = new object[] { pawn };
#if TRACE && SWAP_APPAREL
            Log.Warning("    Optimize Apparel:");
#endif
            for (int i = 0; i < 10; ++i)
            {
                Job job = mi.Invoke(apparelOptimizer, param) as Job;
#if TRACE && SWAP_APPAREL
                Log.Warning("        Optimize Job Loop: " + i + ". Is null: " + (string)((job == null) ? "yes" : "no"));
#endif
                if (job == null)
                    break;
#if TRACE && SWAP_APPAREL
                Log.Warning("        Job is: " + job.def.defName);
#endif
                if (job.def == JobDefOf.Wear)
                {
#if TRACE && SWAP_APPAREL
#endif
                    Apparel a = ((job.targetB != null) ? job.targetB.Thing : null) as Apparel;
                    if (a == null)
                    {
                        Log.Warning("ChangeDresser: Problem equiping pawn. Apparel is null.");
                        break;
                    }
#if TRACE && SWAP_APPAREL
                    Log.Warning("        Chosen Apparel: " + a.Label);
                    Log.Warning("        Wear from ground");
#endif
                    pawn.apparel.Wear(a);
                }
                else if (job.def == Building_Dresser.WEAR_APPAREL_FROM_DRESSER_JOB_DEF)
                {
#if TRACE && SWAP_APPAREL
                    Log.Warning("        Get from Change Dresser");
#endif
                    Building_Dresser d = ((job.targetA != null) ? job.targetA.Thing : null) as Building_Dresser;
                    Apparel a = ((job.targetB != null) ? job.targetB.Thing : null) as Apparel;

                    if (d == null || a == null)
                    {
                        Log.Warning("ChangeDresser: Problem equiping pawn. Dresser or Apparel is null.");
                        break;
                    }
#if TRACE && SWAP_APPAREL
                    Log.Warning("        Chosen Apparel: " + a.Label);
                    Log.Warning("        Wear from dresser " + d.Label);
#endif
                    d.RemoveNoDrop(a);
                    pawn.apparel.Wear(a);
                }
            }

            if (pawn.apparel.WornApparelCount == 0)
            {
#if TRACE && SWAP_APPAREL
                Log.Warning("    Pawn has no cloths. Trying a different method.");
                Log.Warning("    Trying different defs:");
#endif
                // When pawns are not on the home map they will not get dressed using the game's normal method

                // This logic works but pawns will run back to the dresser to change cloths
                foreach (ThingDef def in toWear.filter.AllowedThingDefs)
                {
#if TRACE && SWAP_APPAREL
                    Log.Warning("        Try Find Def " + def.label);
#endif
                    if (pawn.apparel.CanWearWithoutDroppingAnything(def))
                    {
#if TRACE && SWAP_APPAREL
                        Log.Warning("        Can Wear. Check Dressers for apparel:");
#endif
                        foreach (Building_Dresser d in WorldComp.DressersToUse)
                        {
#if TRACE && SWAP_APPAREL
                            Log.Warning("            " + d.Label);
#endif
                            Apparel apparel;
                            if (d.TryRemoveBestApparel(def, toWear.filter, out apparel))
                            {
#if TRACE && SWAP_APPAREL
                                Log.Warning("            Found : " + apparel.Label);
#endif
                                pawn.apparel.Wear(apparel);
                                break;
                            }
#if TRACE && SWAP_APPAREL
                            else
                                Log.Warning("            No matching apparel found");
#endif
                        }
                    }
#if TRACE && SWAP_APPAREL
                    else
                        Log.Warning("        Can't wear");
#endif
                }
            }
            foreach (Apparel a in pawn.apparel.WornApparel)
            {
                Patch_Pawn_ApparelTracker_Notify_ApparelAdded.ColorApparel(pawn, a);
            }
#if SWAP_APPAREL
            Log.Message("End Main.SwapApparel" + Environment.NewLine);
#endif
        }*/
        #endregion
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    static class Patch_Pawn_ApparelTracker_Notify_ApparelAdded
    {
        static void Prefix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            WorldComp.ApparelColorTracker.PersistColor(apparel);
            ColorApparel(__instance.pawn, apparel);
        }

        internal static void ColorApparel(Pawn pawn, Apparel apparel)
        {
            if (WorldComp.PawnOutfits.TryGetValue(pawn, out PawnOutfitTracker outfits))
            {
                outfits.ApplyApparelColor(apparel);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    static class Patch_Pawn_ApparelTracker_Notify_ApparelRemoved
    {
        static void Postfix(Apparel apparel)
        {
            WorldComp.ApparelColorTracker.ResetColor(apparel);
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class Patch_Pawn_GetGizmos
    {
#if DEBUG
        private static int i = 0;
        private static readonly int WAIT = 1000;
#endif
        static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.IsPrisoner && WorldComp.HasDressers())
            {
                __result = new List<Gizmo>(__result)
                {
                    new Command_Action
                    {
                        icon = WidgetUtil.manageapparelTexture,
                        defaultLabel = "ChangeDresser.Wearing".Translate(),
                        activateSound = SoundDef.Named("Click"),
                        action = delegate
                        {
                            Find.WindowStack.Add(new StorageUI(__instance));
                        }
                    }
                };
            }
            else if (!__instance.Drafted && WorldComp.HasDressers())
            {
#if DEBUG
                ++i;
                if (i == WAIT)
                    Log.Warning("DraftController.Postfix: Pawn is Drafted");
#endif
                if (WorldComp.PawnOutfits.TryGetValue(__instance, out PawnOutfitTracker outfits))
                {
                    List<Gizmo> l = new List<Gizmo>(__result);
#if DEBUG
                    if (i == WAIT)
                        Log.Warning("DraftController.Postfix: Sets found! Pre Gizmo Count: " + l.Count);
#endif
                    foreach (IDresserOutfit o in outfits.CivilianOutfits)
                    {
                        if (o == null || !o.IsValid())
                            continue;
#if DEBUG && DRESSER_OUTFIT
                        string msg = "Patch_Pawn_GetGizmos Outfit: " + o.Label;
                        Log.ErrorOnce(msg, msg.GetHashCode());
#endif
                        Command_Action a = new Command_Action();
                        ThingDef icon = o.Icon;
                        if (icon != null)
                        {
                            a.icon = HarmonyPatches.GetIcon(icon);
                        }
                        else
                        {
                            a.icon = WidgetUtil.noneTexture;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (!o.IsBeingWorn)
                        {
                            sb.Append("ChangeDresser.ChangeTo".Translate());
                            a.defaultDesc = "ChangeDresser.ChangeToDesc".Translate();
                        }
                        else
                        {
                            sb.Append("ChangeDresser.Wearing".Translate());
                            a.defaultDesc = "ChangeDresser.WearingDesc".Translate();
                        }
                        sb.Append(" ");
                        sb.Append(o.Label);
                        a.defaultLabel = sb.ToString();
                        a.activateSound = SoundDef.Named("Click");
                        a.action = delegate
                        {
#if DRESSER_OUTFIT
                            Log.Warning("Patch_Pawn_GetGizmos click for " + o.Label);
#endif
                            outfits.ChangeTo(o);
                            //HarmonyPatches.SwapApparel(pawn, o);
                            //outfits.ColorApparel(__instance);
                        };
                        l.Add(a);
                    }
#if DEBUG
                    if (i == WAIT)
                        Log.Warning("Post Gizmo Count: " + l.Count);
#endif
                        __result = l;
                }
            }
#if DEBUG
            else
            {
                if (i == WAIT)
                    Log.Warning("Pawn is not Drafted, could gizmo");
            }
#endif
#if DEBUG
            if (i == WAIT)
                i = 0;
#endif
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    static class Patch_Pawn_DraftController_GetGizmos
    {
#if DEBUG
        private static int i = 0;
        private static readonly int WAIT = 1000;
#endif
        static void Postfix(Pawn_DraftController __instance, ref IEnumerable<Gizmo> __result)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.Drafted && WorldComp.HasDressers())
            {
#if DEBUG
                ++i;
                if (i == WAIT)
                    Log.Warning("DraftController.Postfix: Pawn is Drafted");
#endif
                if (WorldComp.PawnOutfits.TryGetValue(pawn, out PawnOutfitTracker outfits))
                {
                    List<Gizmo> l = new List<Gizmo>(__result);
#if DEBUG
                    if (i == WAIT)
                        Log.Warning("DraftController.Postfix: Sets found! Pre Gizmo Count: " + l.Count);
#endif
                    foreach (IDresserOutfit o in outfits.BattleOutfits)
                    {
                        if (o == null || !o.IsValid())
                            continue;
#if DEBUG && DRESSER_OUTFIT
                        string msg = "Patch_Pawn_DraftController_GetGizmos Outfit: " + o.Label;
                        Log.ErrorOnce(msg, msg.GetHashCode());
#endif
#if DEBUG
                        if (i == WAIT)
                            Log.Warning("DraftController.Postfix: Set: " + o.Label + ", Current Oufit: " + pawn.outfits.CurrentOutfit.label);
#endif
                        Command_Action a = new Command_Action();
                        ThingDef icon = o.Icon;
                        if (icon != null)
                        {
                            a.icon = HarmonyPatches.GetIcon(icon);
                        }
                        else
                        {
                            a.icon = WidgetUtil.noneTexture;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (!pawn.outfits.CurrentOutfit.Equals(o))
                        {
                            sb.Append("ChangeDresser.ChangeTo".Translate());
                            a.defaultDesc = "ChangeDresser.ChangeToDesc".Translate();
                        }
                        else
                        {
                            sb.Append("ChangeDresser.Wearing".Translate());
                            a.defaultDesc = "ChangeDresser.WearingDesc".Translate();
                        }
                        sb.Append(" ");
                        sb.Append(o.Label);
                        a.defaultLabel = sb.ToString();
                        a.activateSound = SoundDef.Named("Click");
                        a.action = delegate
                        {
#if DRESSER_OUTFIT
                                Log.Warning("Patch_Pawn_DraftController_GetGizmos click for " + o.Label);
#endif
                            outfits.ChangeTo(o);
                            //HarmonyPatches.SwapApparel(pawn, o);
                            //outfits.ColorApparel(pawn);
                        };
                        l.Add(a);
                    }
#if DEBUG
                    if (i == WAIT)
                        Log.Warning("Post Gizmo Count: " + l.Count);
#endif
                    __result = l;
                }
            }
#if DEBUG
            else
            {
                if (i == WAIT)
                    Log.Warning("Pawn is not Drafted, could gizmo");
            }
#endif
#if DEBUG
            if (i == WAIT)
                i = 0;
#endif
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    static class Patch_Pawn_DraftController
    {
        static void Postfix(Pawn_DraftController __instance)
        {
            if (WorldComp.HasDressers())
            {
                Pawn pawn = __instance.pawn;
                if (WorldComp.PawnOutfits.TryGetValue(pawn, out PawnOutfitTracker outfits))
                {
                    if (pawn.Drafted)
                    {
                        outfits.ChangeToBattleOutfit();
                    }
                    else
                    {
                        outfits.ChangeToCivilianOutfit();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob", new Type[] { typeof(Pawn) })]
    static class Patch_JobGiver_OptimizeApparel
    {
        static void Postfix(Pawn pawn, ref Job __result)
        {
            if (Find.TickManager.TicksGame < pawn.mindState.nextApparelOptimizeTick)
            {
                return;
            }

#if BETTER_OUTFIT
            Log.Warning("Begin JobGiver_OptimizeApparel.Postfix(Pawn: " + pawn.Name.ToStringShort + "     Job: " + ((__result == null) ? "<null>" : __result.ToString()) + ")");
#endif
            if (!DoDressersHaveApparel() || pawn.apparel?.LockedApparel?.Count > 0)
            {
                return;
            }

            Thing thing = null;
            float baseApparelScore = 0f;
            if (__result != null && __result.targetA.Thing is Apparel)
            {
                thing = __result.targetA.Thing;
                baseApparelScore = JobGiver_OptimizeApparel.ApparelScoreGain(pawn, thing as Apparel);
                if (thing == null)
                {
                    baseApparelScore = 0f;
                }
                else
                {
#if BETTER_OUTFIT
                    Log.Message("    Game Found Better Apparel: " + ((thing == null) ? "<null>" : thing.Label) + "    Score: " + baseApparelScore);
#endif
                }
            }

            Apparel a = null;
            Building_Dresser containingDresser = null;

#if BETTER_OUTFIT
            Log.Message("    Loop Through Dressers:");
#endif
            foreach (Building_Dresser dresser in WorldComp.DressersToUse)
            {
#if TRACE && BETTER_OUTFIT
                Log.Message("        Dresser: " + dresser.Label);
#endif
                float score = baseApparelScore;
                if (dresser.FindBetterApparel(ref score, ref a, pawn, pawn.outfits.CurrentOutfit))
                {
                    thing = a;
                    baseApparelScore = score;
                    containingDresser = dresser;
#if BETTER_OUTFIT
                    Log.Message("    Dresser Found Better Apparel: " + ((a == null) ? "<null>" : a.Label) + "    Score: " + baseApparelScore);
#endif
                }
            }
#if BETTER_OUTFIT
            Log.Message("    Best Apparel: " + ((a == null) ? "<null>" : a.Label) + "    Score: " + baseApparelScore);
#endif
            if (a != null && containingDresser != null)
            {
                __result = new Job(containingDresser.wearApparelFromStorageJobDef, containingDresser, a);
            }
#if BETTER_OUTFIT
            Log.Warning("End JobGiver_OptimizeApparel.Postfix");
#endif
            countInTime.Increment();
        }

        private static bool DoDressersHaveApparel()
        {
            foreach (Building_Dresser d in WorldComp.DressersToUse)
            {
                if (d.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }

    static class TradeUtil
    {
        public static IEnumerable<T> EmptyDressers<T>(Map map) where T : Thing
        {
            List<T> a = new List<T>();
            foreach (Building_Dresser d in WorldComp.DressersToUse)
            {
                if (d.Map == map && d.Spawned && d.IncludeInTradeDeals)
                {
                    d.Empty<T>(a);
                }
            }
            return a;
        }

        public static void ReclaimApparel()
        {
            foreach (Building_Dresser d in WorldComp.DressersToUse)
            {
                if (d.Map != null && d.Spawned)
                {
                    d.ReclaimApparel();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_TraderTracker), "ColonyThingsWillingToBuy")]
    static class Patch_TradeShip_ColonyThingsWillingToBuy
    {
        // Before a caravan trade
        static void Postfix(ref IEnumerable<Thing> __result, Pawn playerNegotiator)
        {
            if (playerNegotiator != null && playerNegotiator.Map != null)
            {
                List<Thing> result = new List<Thing>(__result);
                result.AddRange(TradeUtil.EmptyDressers<Thing>(playerNegotiator.Map));
                __result = result;
            }
        }
    }

    [HarmonyPatch(typeof(TradeShip), "ColonyThingsWillingToBuy")]
    static class Patch_PassingShip_TryOpenComms
    {
        // Before an orbital trade
        static void Postfix(ref IEnumerable<Thing> __result, Pawn playerNegotiator)
        {
            if (playerNegotiator != null && playerNegotiator.Map != null)
            {
                List<Thing> result = new List<Thing>(__result);
                result.AddRange(TradeUtil.EmptyDressers<Thing>(playerNegotiator.Map));
                __result = result;
            }
        }
    }

    [HarmonyPatch(typeof(TradeDeal), "Reset")]
    static class Patch_TradeDeal_Reset
    {
        // On Reset from Trade Dialog
        static void Prefix()
        {
            TradeUtil.ReclaimApparel();
        }
    }

    [HarmonyPatch(typeof(Dialog_Trade), "Close")]
    static class Patch_Window_PreClose
    {
        static void Postfix(bool doCloseSound)
        {
            TradeUtil.ReclaimApparel();
        }
    }

    [HarmonyPatch(typeof(ReservationManager), "CanReserve")]
    static class Patch_ReservationManager_CanReserve
    {
        private static FieldInfo mapFI = null;
        static void Postfix(ref bool __result, ReservationManager __instance, Pawn claimant, LocalTargetInfo target, int maxPawns, int stackCount, ReservationLayerDef layer, bool ignoreOtherReservations)
        {
            if (mapFI == null)
            {
                mapFI = typeof(ReservationManager).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
            }

#if DEBUG
            Log.Warning("\nCanReserve original result: " + __result);
#endif
            if (!__result && mapFI != null && (target.Thing == null || target.Thing.def.defName.Equals("ChangeDresser")))
            {
                Map m = (Map)mapFI.GetValue(__instance);
                if (m != null)
                {
                    IEnumerable<Thing> things = m.thingGrid.ThingsAt(target.Cell);
                    if (things != null)
                    {
#if DEBUG
                    Log.Warning("CanReserve - Found things");
#endif
                        foreach (Thing t in things)
                        {
#if DEBUG
                        Log.Warning("CanReserve - def " + t.def.defName);
#endif
                            if (t.def.defName.Equals("ChangeDresser"))
                            {
#if DEBUG
                            Log.Warning("CanReserve is now true\n");
#endif
                                __result = true;
                            }
                        }
                    }
                }
            }
        }
    }

#region Caravan Forming
    [HarmonyPatch(typeof(Dialog_FormCaravan), "PostOpen")]
    static class Patch_Dialog_FormCaravan_PostOpen
    {
        static void Prefix(Window __instance)
        {
            Type type = __instance.GetType();
            if (type == typeof(Dialog_FormCaravan))
            {
                Map map = __instance.GetType().GetField("map", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as Map;

                foreach (Building_Dresser d in WorldComp.GetDressers(map))
                {
                    d.Empty<Thing>();
                }
            }
        }
    }

    [HarmonyPatch(typeof(CaravanFormingUtility), "StopFormingCaravan")]
    static class Patch_CaravanFormingUtility_StopFormingCaravan
    {
        [HarmonyPriority(Priority.First)]
        static void Postfix(Lord lord)
        {
            foreach (Building_Dresser d in WorldComp.DressersToUse)
            {
                d.ReclaimApparel();
            }
        }
    }

    [HarmonyPatch(
        typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan",
        new Type[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(int), typeof(int), typeof(bool) })]
    static class Patch_CaravanExitMapUtility_ExitMapAndCreateCaravan
    {
        [HarmonyPriority(Priority.First)]
        static void Prefix(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, int directionTile, int destinationTile, bool sendMessage)
        {
            if (faction == Faction.OfPlayer)
            {
                List<Pawn> p = new List<Pawn>(pawns);
                if (p.Count > 0)
                {
                    foreach (Building_Dresser d in WorldComp.DressersToUse)
                    {
                        d.ReclaimApparel();
                    }
                }
            }
        }
    }

    /*[HarmonyPatch(
        typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan",
        new Type[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int) })]
    static class Patch_CaravanExitMapUtility_ExitMapAndCreateCaravan_2
    {
        static void Prefix(IEnumerable<Pawn> pawns, Faction faction, int startingTile)
        {
            if (faction == Faction.OfPlayer)
            {
                List<Pawn> p = new List<Pawn>(pawns);
                if (p.Count > 0)
                {
                    foreach (Building_Dresser d in WorldComp.DressersToUse)
                    {
                        d.ReclaimApparel();
                    }
                }
            }
        }
    }*/
#endregion

#region Handle "Do until X" for stored weapons
    [HarmonyPatch(typeof(RecipeWorkerCounter), "CountProducts")]
    static class Patch_RecipeWorkerCounter_CountProducts
    {
        static void Postfix(ref int __result, RecipeWorkerCounter __instance, Bill_Production bill)
        {
            List<ThingDefCountClass> products = __instance.recipe.products;
            if (WorldComp.DressersToUse.Count > 0 && products != null)
            {
                foreach (ThingDefCountClass product in products)
                {
                    ThingDef def = product.thingDef;
                    foreach (Building_Dresser d in WorldComp.DressersToUse)
                    {
                        if (bill.Map == d.Map)
                        {
                            __result += d.GetApparelCount(def, bill.qualityRange, bill.hpRange, (bill.limitToAllowedStuff) ? bill.ingredientFilter : null);
                        }
                    }
                }
            }
        }
    }
#endregion

#region Pawn Death
    [HarmonyPatch(typeof(Pawn), "Kill")]
    static class Patch_Pawn_Kill
    {
        private static Map map;

        [HarmonyPriority(Priority.First)]
        static void Prefix(Pawn __instance)
        {
            map = __instance.Map;
        }

        [HarmonyPriority(Priority.First)]
        static void Postfix(Pawn __instance)
        {
            if (__instance.Dead && __instance.apparel?.LockedApparel?.Count == 0)
            {
                if (WorldComp.PawnOutfits.TryGetValue(__instance, out PawnOutfitTracker po))
                {
                    WorldComp.PawnOutfits.Remove(__instance);

                    foreach (Apparel a in po.CustomApparel)
                    {
                        if (!WorldComp.AddApparel(a))
                        {
                            BuildingUtil.DropThing(a, __instance.Position, map, true);
                        }
                    }
                }
            }
        }
    }
#endregion

    [HarmonyPatch(typeof(OutfitDatabase), "TryDelete")]
    static class Patch_OutfitDatabase_TryDelete
    {
        static void Postfix(AcceptanceReport __result, Outfit outfit)
        {
            if (__result.Accepted)
            {
                WorldComp.OutfitsForBattle.Remove(outfit);
                foreach (PawnOutfitTracker po in WorldComp.PawnOutfits.Values)
                {
                    po.Remove(outfit);
                }
            }
        }
    }

	[HarmonyPatch(typeof(ScribeSaver), "InitSaving")]
	static class Patch_ScribeSaver_InitSaving
	{
		static void Prefix()
		{
			try
			{
				foreach (Building_Dresser d in WorldComp.GetDressers(null))
				{
					try
					{
						d.ReclaimApparel(true);
					}
					catch (Exception e)
					{
						Log.Warning("Error while reclaiming apparel for change dresser\n" + e.Message);
					}
				}
			}
			catch (Exception e)
			{
				Log.Warning("Error while reclaiming apparel\n" + e.Message);
			}
		}
	}

    [HarmonyPatch(typeof(SettlementAbandonUtility), "Abandon")]
    static class Patch_SettlementAbandonUtility_Abandon
    {
        [HarmonyPriority(Priority.First)]
        static void Prefix(MapParent settlement)
        {
            WorldComp.RemoveDressers(settlement.Map);
        }
    }
    
    [HarmonyPatch(typeof(Caravan), "AddPawn")]
    static class Patch_Caravan_AddPawn
    {
        [HarmonyPriority(Priority.First)]
        static void Prefix(Pawn p, bool addCarriedPawnToWorldPawnsIfAny)
        {
            try
            {
                if (p != null && p.Drafted)
                    p.drafter.Drafted = false;
            }
            catch(Exception e)
            {
                Log.Error("Exception thrown from ChangeDresser Patch_Caravan_AddPawn - " + e.GetType().Name + " " + e.Message);
            }
        }
    }
    /*
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreRaw")]
    static class Patch_JobGiver_OptimizeApparel_ApparelScoreRaw
    {
        static SimpleCurve HitPointsPercentScoreFactorCurve = null;
        static SimpleCurve InsulationColdScoreFactorCurve_NeedWarm = null;
        static FieldInfo NeedWarmthFI = null;

        [HarmonyPriority(Priority.First)]
        static bool Prefix(ref float __result, JobGiver_OptimizeApparel __instance, Pawn pawn, Apparel ap)
        {
            Log.Message("1 pawn is " + ((pawn == null) ? "null" : "not null"));

            if (pawn != null)
                return true;
            Log.Message("2");

            if (HitPointsPercentScoreFactorCurve == null)
            {
                HitPointsPercentScoreFactorCurve = typeof(JobGiver_OptimizeApparel).GetField("HitPointsPercentScoreFactorCurve", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as SimpleCurve;
                InsulationColdScoreFactorCurve_NeedWarm = typeof(JobGiver_OptimizeApparel).GetField("InsulationColdScoreFactorCurve_NeedWarm", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as SimpleCurve;
                NeedWarmthFI = typeof(JobGiver_OptimizeApparel).GetField("neededWarmth", BindingFlags.Static | BindingFlags.NonPublic);
            }
            Log.Message("HitPointsPercentScoreFactorCurve is " + ((HitPointsPercentScoreFactorCurve == null) ? "null" : "not null"));
            Log.Message("InsulationColdScoreFactorCurve_NeedWarm is " + ((InsulationColdScoreFactorCurve_NeedWarm == null) ? "null" : "not null"));
            Log.Message("NeedWarmthFI is " + ((NeedWarmthFI == null) ? "null" : "not null"));
            Log.Message("NeedWarmth is " + NeedWarmthFI.GetValue(null));

            float result = 0.1f + ap.GetStatValue(StatDefOf.ArmorRating_Sharp) + ap.GetStatValue(StatDefOf.ArmorRating_Blunt);
            if (ap.def.useHitPoints)
            {
                float x = (float)ap.HitPoints / (float)ap.MaxHitPoints;
                result *= HitPointsPercentScoreFactorCurve.Evaluate(x);
            }
            result += ap.GetSpecialApparelScoreOffset();
            float num3 = 1f;
            if ((NeededWarmth)NeedWarmthFI.GetValue(null) == NeededWarmth.Warm)
            {
                float statValue = ap.GetStatValue(StatDefOf.Insulation_Cold);
                num3 *= InsulationColdScoreFactorCurve_NeedWarm.Evaluate(statValue);
            }
            result *= num3;
            if (ap.WornByCorpse)
            {
                result -= 0.5f;
                if (result > 0f)
                {
                    result *= 0.1f;
                }
            }
            if (ap.Stuff == ThingDefOf.Human.race.leatherDef)
            {
                result -= 0.5f;
                if (result > 0f)
                {
                    result *= 0.1f;
                }
            }
            __result = result;
            return false;
        }
    }*/
}
