using ChangeDresser.UI.Util;
using Harmony;
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
            var harmony = HarmonyInstance.Create("com.changedresser.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            Log.Message(
                "ChangeDresser Harmony Patches:" + Environment.NewLine +
                "  Prefix:" + Environment.NewLine +
                "    Dialog_FormCaravan.PostOpen" + Environment.NewLine +
                "    CaravanExitMapUtility.ExitMapAndCreateCaravan(IEnumerable<Pawn>, Faction, int)" + Environment.NewLine +
                "    CaravanExitMapUtility.ExitMapAndCreateCaravan(IEnumerable<Pawn>, Faction, int, int)" + Environment.NewLine + 
                "  Postfix:" + Environment.NewLine +
                "    Pawn.GetGizmos" + Environment.NewLine +
                "    Pawn_ApparelTracker.Notify_ApparelAdded" + Environment.NewLine +
                "    Pawn_DraftController.Drafted { set }" + Environment.NewLine +
                "    Pawn_DraftController.GetGizmos" + Environment.NewLine +
                "    JobGiver_OptimizeApparel.TryGiveJob" + Environment.NewLine +
                "    ReservationManager.CanReserve" + Environment.NewLine + 
                "    OutfitDatabase.TryDelete" + Environment.NewLine +
                "    CaravanFormingUtility.StopFormingCaravan");
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

        public static void SwapApparel(Pawn pawn, Outfit toWear)
        {
#if DEBUG
            Log.Message(
                Environment.NewLine + 
                "Start Main.SwapApparel Pawn: " + pawn.Name.ToStringShort + " toWear: " + toWear.label);
#endif
            if (!WorldComp.HasDressers())
            {
                Log.Warning("No Change Dressers found. Apparel will not be swapped.");
                return;
            }

            // Remove apparel from pawn
            List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);
            foreach (Apparel a in worn)
            {
                if (Settings.KeepForcedApparel && 
                    pawn.outfits.forcedHandler.ForcedApparel.Contains(a))
                {
                    continue;
                }
                    
                pawn.apparel.Remove(a);
#if DEBUG
                Log.Warning(" Apparel " + a.LabelShort + " removed");
#endif

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
                }*/
                if (!WorldComp.AddApparel(a))
                {
#if DEBUG
                    Log.Warning("  Apparel " + a.LabelShort + " was not handled");
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

            pawn.outfits.CurrentOutfit = toWear;

            typeof (JobGiver_OptimizeApparel)
                .GetField("neededWarmth", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, PawnApparelGenerator.CalculateNeededWarmth(pawn, pawn.Map.Tile, GenLocalDate.Twelfth(pawn)));

            MethodInfo mi = typeof(JobGiver_OptimizeApparel).GetMethod("TryGiveJob", BindingFlags.Instance | BindingFlags.NonPublic);

            JobGiver_OptimizeApparel apparelOptimizer = new JobGiver_OptimizeApparel();
            object[] param = new object[] { pawn };
            for (int i = 0; i < 10; ++i)
            {
#if DEBUG
                Log.Warning(i + " start equip for loop");
#endif
                Job job = mi.Invoke(apparelOptimizer, param) as Job;
#if DEBUG
                Log.Warning(i + " job is null: " + (string)((job == null) ? "yes" : "no"));
#endif
                if (job == null)
                    break;
#if DEBUG
                Log.Warning(job.def.defName);
#endif
                if (job.def == JobDefOf.Wear)
                {
                    Apparel a = ((job.targetB != null) ? job.targetB.Thing : null) as Apparel;
                    if (a == null)
                    {
                        Log.Warning("ChangeDresser: Problem equiping pawn. Apparel is null.");
                        break;
                    }
#if DEBUG
                    Log.Warning("Wear from ground " + a.Label);
#endif
                    pawn.apparel.Wear(a);
                }
                else if (job.def == Building_Dresser.WEAR_APPAREL_FROM_DRESSER_JOB_DEF)
                {
                    Building_Dresser d = ((job.targetA != null) ? job.targetA.Thing : null) as Building_Dresser;
                    Apparel a = ((job.targetB != null) ? job.targetB.Thing : null) as Apparel;

                    if (d == null || a == null)
                    {
                        Log.Warning("ChangeDresser: Problem equiping pawn. Dresser or Apparel is null.");
                        break;
                    }
#if DEBUG
                    Log.Warning("Wear from dresser " + d.Label + " " + a.Label);
#endif
                    d.RemoveNoDrop(a);
                    pawn.apparel.Wear(a);
                }
#if DEBUG
                Log.Warning(i + " end equip for loop");
#endif
            }

            if (pawn.apparel.WornApparelCount == 0)
            {
                // When pawns are not on the home map they will not get dressed using the game's normal method

                // This logic works but pawns will run back to the dresser to change cloths
                foreach (ThingDef def in toWear.filter.AllowedThingDefs)
                {
    #if DEBUG
                    Log.Warning("  Try Find Def " + def.label);
    #endif
                    if (pawn.apparel.CanWearWithoutDroppingAnything(def))
                    {
    #if DEBUG
                        Log.Warning("   Can wear");
    #endif
                        foreach (Building_Dresser d in WorldComp.DressersToUse)
                        {
    #if DEBUG
                            Log.Warning("   Check dresser " + d.Label);
    #endif
                            Apparel apparel;
                            if (d.TryRemoveBestApparel(def, toWear.filter, out apparel))
                            {
    #if DEBUG
                                Log.Warning("    Found " + apparel.LabelShort);
    #endif
                                pawn.apparel.Wear(apparel);
                                break;
                            }
    #if DEBUG
                            else
                                Log.Warning("    No matching apparel found");
    #endif
                        }
                    }
    #if DEBUG
                    else
                        Log.Warning("  Can't wear");
    #endif
                }
            }
#if DEBUG
            Log.Message("End Main.SwapApparel" + Environment.NewLine);
#endif
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    static class Patch_Pawn_ApparelTracker_Notify_ApparelAdded
    {
        static void Prefix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            PawnOutfits outfits;
            if (WorldComp.PawnOutfits.TryGetValue(__instance.pawn, out outfits))
            {
                outfits.ColorApparel(apparel);
            }
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
            if (!__instance.Drafted)
            {
#if DEBUG
                ++i;
                if (i == WAIT)
                    Log.Warning("DraftController.Postfix: Pawn is Drafted");
#endif
                PawnOutfits outfits;
                if (WorldComp.PawnOutfits.TryGetValue(__instance, out outfits))
                {
                    List<Gizmo> l = new List<Gizmo>(__result);
#if DEBUG
                    if (i == WAIT)
                        Log.Warning("DraftController.Postfix: Sets found! Pre Gizmo Count: " + l.Count);
#endif
                    foreach (Outfit o in outfits.Outfits)
                    {
                        if (o == null)
                            continue;
                        bool forBattle = WorldComp.OutfitsForBattle.Contains(o);
#if DEBUG
                        if (i == WAIT)
                            Log.Warning("DraftController.Postfix: Set: " + o.label + ", forBattle: " + forBattle + ", Cuurent Oufit: " + __instance.outfits.CurrentOutfit.label);
#endif
                        if (!forBattle)
                        {
                            Command_Action a = new Command_Action();
                            List<ThingDef> tdList = new List<ThingDef>(o.filter.AllowedThingDefs);
                            if (tdList.Count > 0)
                            {
                                a.icon = HarmonyPatches.GetIcon(tdList[0]);
                            }
                            else
                            {
                                a.icon = WidgetUtil.noneTexture;
                            }
                            StringBuilder sb = new StringBuilder();
                            if (!__instance.outfits.CurrentOutfit.Equals(o))
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
                            sb.Append(o.label);
                            a.defaultLabel = sb.ToString();
                            a.activateSound = SoundDef.Named("Click");
                            a.action = delegate
                            {
                                HarmonyPatches.SwapApparel(__instance, o);
                                //outfits.ColorApparel(__instance);
                            };
                            l.Add(a);
                        }
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
            if (pawn.Drafted)
            {
#if DEBUG
                ++i;
                if (i == WAIT)
                    Log.Warning("DraftController.Postfix: Pawn is Drafted");
#endif
                PawnOutfits outfits;
                if (WorldComp.PawnOutfits.TryGetValue(pawn, out outfits))
                {
                    List<Gizmo> l = new List<Gizmo>(__result);
#if DEBUG
                    if (i == WAIT)
                        Log.Warning("DraftController.Postfix: Sets found! Pre Gizmo Count: " + l.Count);
#endif
                    foreach (Outfit o in outfits.Outfits)
                    {
                        if (o == null)
                            continue;
                        bool forBattle = WorldComp.OutfitsForBattle.Contains(o);
#if DEBUG
                        if (i == WAIT)
                            Log.Warning("DraftController.Postfix: Set: " + o.label + ", forBattle: " + forBattle + ", Current Oufit: " + pawn.outfits.CurrentOutfit.label);
#endif
                        if (forBattle)
                        {
                            Command_Action a = new Command_Action();
                            List<ThingDef> tdList = new List<ThingDef>(o.filter.AllowedThingDefs);
                            if (tdList.Count > 0)
                            {
                                a.icon = HarmonyPatches.GetIcon(tdList[0]);
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
                            sb.Append(o.label);
                            a.defaultLabel = sb.ToString();
                            a.activateSound = SoundDef.Named("Click");
                            a.action = delegate
                            {
                                HarmonyPatches.SwapApparel(pawn, o);
                                //outfits.ColorApparel(pawn);
                            };
                            l.Add(a);
                        }
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
            Pawn pawn = __instance.pawn;
            PawnOutfits outfits;
            if (WorldComp.PawnOutfits.TryGetValue(pawn, out outfits))
            {
                Outfit outfitToWear;
                bool found = false;
                if (pawn.Drafted)
                {
                    outfits.LastCivilianOutfit = pawn.outfits.CurrentOutfit;
                    found = outfits.TryGetBattleOutfit(out outfitToWear);
                }
                else
                {
                    outfits.LastBattleOutfit = pawn.outfits.CurrentOutfit;
                    found = outfits.TryGetCivilianOutfit(out outfitToWear);
                }

                if (found)
                {
                    HarmonyPatches.SwapApparel(pawn, outfitToWear);
                    //outfits.ColorApparel(pawn);
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob", new Type[] { typeof(Pawn) })]
    static class Patch_JobGiver_OptimizeApparel
    {
        static void Postfix(Pawn pawn, ref Job __result)
        {
            if (!DoDressersHaveApparel())
            {
                return;
            }

            Thing thing = null;
            if (__result != null)
            {
                thing = __result.targetA.Thing;
            }

            Building_Dresser containingDresser = null;
            float baseApparelScore = 0f;

            foreach (Building_Dresser dresser in WorldComp.DressersToUse)
            {
                float score = baseApparelScore;
                Apparel a = dresser.FindBetterApparel(ref score, pawn, pawn.outfits.CurrentOutfit);

                if (score > baseApparelScore && a != null)
                {
                    thing = a;
                    baseApparelScore = score;
                    containingDresser = dresser;
                }
                
            }
            if (thing != null && containingDresser != null)
            {
                __result = new Job(containingDresser.wearApparelFromStorageJobDef, containingDresser, thing);
            }
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

    [HarmonyPatch(typeof(OutfitDatabase), "TryDelete")]
    static class Patch_OutfitDatabase_TryDelete
    {
        static void Postfix(ref AcceptanceReport __result, Outfit outfit)
        {
            if (__result.Accepted)
            {
                WorldComp.OutfitsForBattle.Remove(outfit);
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
            foreach (Building_Dresser d in WorldComp.GetDressers(lord.Map))
            {
                d.ReclaimApparel();
            }
        }
    }

    [HarmonyPatch(
        typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan",
        new Type[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(int) })]
    static class Patch_CaravanExitMapUtility_ExitMapAndCreateCaravan_1
    {
        [HarmonyPriority(Priority.First)]
        static void Prefix(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, int directionTile)
        {
            if (faction == Faction.OfPlayer)
            {
                List<Pawn> p = new List<Pawn>(pawns);
                if (p.Count > 0)
                {
                    foreach (Building_Dresser d in WorldComp.GetDressers(p[0].Map))
                    {
                        d.ReclaimApparel();
                    }
                }
            }
        }
    }

    [HarmonyPatch(
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
                    foreach (Building_Dresser d in WorldComp.GetDressers(p[0].Map))
                    {
                        d.ReclaimApparel();
                    }
                }
            }
        }
    }
    #endregion

    #region Handle "Do until X" for stored weapons
    [HarmonyPatch(typeof(RecipeWorkerCounter), "CountProducts")]
    static class Patch_RecipeWorkerCounter_CountProducts
    {
        static void Postfix(ref int __result, RecipeWorkerCounter __instance, Bill_Production bill)
        {
            List<ThingCountClass> products = __instance.recipe.products;
            if (WorldComp.DressersToUse.Count > 0 && products != null)
            {
                foreach (ThingCountClass product in products)
                {
                    ThingDef def = product.thingDef;
                    foreach (Building_Dresser d in WorldComp.DressersToUse)
                    {
                        if (bill.Map == d.Map)
                        {
                            __result += d.GetApparelCount(def);
                        }
                    }
                }
            }
        }
    }
    #endregion
}
