using ChangeDresser.StoredApparel;
using ChangeDresser.UI.Util;
using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;

namespace ChangeDresser
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = HarmonyInstance.Create("com.changedresser.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            WidgetUtil.Initialize();

            IsSwapping = false;
            
            Log.Message("ChangeDresser: Adding Harmony Postfix to Pawn_DraftController.Drafted { set }");
            Log.Message("ChangeDresser: Adding Harmony Postfix to JobGiver_OptimizeApparel.TryGiveJob(Pawn)");
        }

        public static bool IsSwapping { get; private set; }
        public static void SwapApparel(StoredApparelSet toWear)
        {
            try
            {
                IsSwapping = true;
                Pawn pawn = toWear.Pawn;
                // Remove apparel from pawn
                StoredApparelSet wornSet;
                if (!StoredApparelContainer.TryGetWornApparelSet(pawn, out wornSet))
                {
#if DEBUG
                Log.Warning("Main.SwapApparel: Creating set being worn as Temp");
#endif
                    wornSet = new StoredApparelSet();
                    wornSet.Name = "TempSet";
                    wornSet.IsTemp = true;
                    wornSet.ForBattle = false;
                    wornSet.Pawn = pawn;
                    wornSet.AssignedApparel = new List<Apparel>(pawn.apparel.WornApparel);
                    wornSet.SetForcedApparel(pawn.outfits.forcedHandler.ForcedApparel);
                    StoredApparelContainer.AddApparelSet(wornSet);
                }
#if DEBUG
            else
            {
                Log.Warning("Main.SwapApparel: Found Set being Worn " + wornSet.Name);
            }
#endif

                wornSet.SwitchedFrom = true;
                wornSet.IsBeingWorn = false;

                toWear.SwitchedFrom = false;
                toWear.IsBeingWorn = true;

                foreach (Apparel a in wornSet.AssignedApparel)
                {
                    pawn.apparel.Remove(a);
                }

                // Dress the pawn
                foreach (Apparel a in toWear.AssignedApparel)
                {
                    pawn.apparel.Wear(a);
                }

                foreach (Apparel a in toWear.AssignedApparel)
                {
                    if (toWear.WasForced(a))
                    {
                        pawn.outfits.forcedHandler.ForcedApparel.Add(a);
                    }
                }
            }
            finally
            {
                IsSwapping = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class Pawn_GetGizmos
    {
        static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!__instance.Drafted)
            {
                IEnumerable<StoredApparelSet> sets;
                if (StoredApparelContainer.TryGetApparelSets(__instance, out sets))
                {
                    List<Gizmo> l = new List<Gizmo>(__result);
                    foreach (StoredApparelSet s in sets)
                    {
                        if (!s.ForBattle && !s.IsBeingWorn)
                        {
                            Command_Action a = new Command_Action();
                            string texPath = s.TexPath;
                            if (texPath != null)
                            {
                                a.icon = ContentFinder<UnityEngine.Texture2D>.Get(texPath, true);
                            }
                            StringBuilder sb = new StringBuilder("ChangeDresser.ChangeTo".Translate());
                            sb.Append(" ");
                            sb.Append(s.Name);
                            a.defaultLabel = sb.ToString();
                            a.defaultDesc = "ChangeDresser.ChangeToDesc";
                            a.activateSound = SoundDef.Named("Click");
                            a.action = delegate
                            {
                                Main.SwapApparel(s);
                            };
                            l.Add(a);
                        }
                    }
                    __result = l;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    static class Patch_Pawn_DraftController_GetGizmos
    {
#if DEBUG
        private static int i = 0;
        private static readonly int WAIT = 4000;
#endif
        static void Postfix(Pawn_DraftController __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.pawn.Drafted)
            {
#if DEBUG
                ++i;
                if (i == WAIT)
                    Log.Warning("DraftController.Postfix: Pawn is Drafted");
#endif
                IEnumerable<StoredApparelSet> sets;
                if (StoredApparelContainer.TryGetApparelSets(__instance.pawn, out sets))
                {
                    List<Gizmo> l = new List<Gizmo>(__result);
#if DEBUG
                    if (i == WAIT)
                        Log.Warning("DraftController.Postfix: Sets found! Pre Gizmo Count: " + l.Count);
#endif
                    foreach (StoredApparelSet s in sets)
                    {
#if DEBUG
                        if (i == WAIT)
                            Log.Warning("DraftController.Postfix: Set: " + s.Name + ", forBattle: " + s.ForBattle + ", isBeingWorn: " + s.IsBeingWorn);
#endif
                        if (s.ForBattle && !s.IsBeingWorn)
                        {
                            Command_Action a = new Command_Action();
                            string texPath = s.TexPath;
                            if (texPath != null)
                            {
                                a.icon = ContentFinder<UnityEngine.Texture2D>.Get(texPath, true);
                            }
                            StringBuilder sb = new StringBuilder("ChangeDresser.ChangeTo".Translate());
                            sb.Append(" ");
                            sb.Append(s.Name);
                            a.defaultLabel = sb.ToString();
                            a.defaultDesc = "ChangeDresser.ChangeToDesc";
                            a.activateSound = SoundDef.Named("Click");
                            a.action = delegate
                            {
                                Main.SwapApparel(s);
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

    /*[HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    static class Patch_Pawn_ApparelTracker_Notify_ApparelAdded
    {
        static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            StoredApparelSet set;
            if (StoredApparelContainer.TryGetWornApparelSet(__instance.pawn, out set))
            {
                set.Notify_ApparelChange(apparel);
            }
        }
    }*/

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    static class Patch_Pawn_ApparelTracker_Notify_ApparelRemoved
    {
        static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            if (!Main.IsSwapping)
            {
                StoredApparelContainer.Notify_ApparelRemoved(__instance.pawn, apparel);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    static class Patch_Pawn_DraftController
    {
        static void Postfix(Pawn_DraftController __instance)
        {
#if DEBUG
            Log.Warning("Pawn_DraftController.set_Drafted.Postfix: " + __instance.pawn.NameStringShort + " Drafted: " + __instance.pawn.Drafted);
#endif
            StoredApparelSet set;
            if (StoredApparelContainer.TryGetBestApparelSet(__instance.pawn, __instance.pawn.Drafted, out set))
            {
#if DEBUG
                Log.Warning("Pawn_DraftController.set_Drafted.Postfix: Swap To " + set.Name);
#endif
                Main.SwapApparel(set);
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob", new Type[] { typeof(Pawn) })]
    static class Patch_JobGiver_OptimizeApparel
    {
        static void Postfix(Pawn pawn, ref Job __result)
        {
            IEnumerable<Building_Dresser> dressers = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Dresser>();
            if (!DoesDressersHaveApparel(dressers))
            {
                return;
            }

            Thing thing = null;
            if (__result != null)
            {
                thing = __result.targetA.Thing;
            }

            Building_Dresser containingDresser = null;
            Outfit currentOutfit = pawn.outfits.CurrentOutfit;
            float baseApparelScore = 0f;

            foreach (Building_Dresser dresser in dressers)
            {
                foreach (Apparel apparel in dresser.StoredApparel)
                {
                    if (currentOutfit.filter.Allows(apparel))
                    {
                        if (!apparel.IsForbidden(pawn))
                        {
                            float newApparelScore = JobGiver_OptimizeApparel.ApparelScoreGain(pawn, apparel);
                            if (newApparelScore >= 0.05f && newApparelScore >= baseApparelScore)
                            {
                                if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
                                {
                                    if (ReservationUtility.CanReserveAndReach(pawn, dresser, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1))
                                    {
                                        containingDresser = dresser;
                                        thing = apparel;
                                        baseApparelScore = newApparelScore;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (thing != null && containingDresser != null)
            {
                __result = new Job(containingDresser.wearApparelFromStorageJobDef, containingDresser, thing);
            }
        }

        private static bool DoesDressersHaveApparel(IEnumerable<Building_Dresser> dressers)
        {
            foreach (Building_Dresser d in dressers)
            {
                if (d.StoredApparel.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
    /*
    [HarmonyPatch(typeof(Settlement_TraderTracker), "RegenerateStock")]
    static class Patch_Settlement_TraderTracker_RegenerateStock
    {
        static void Postfix(Settlement_TraderTracker __instance)
        {
            ThingOwner<Thing> l = (ThingOwner<Thing>)typeof(Settlement_TraderTracker).GetField("stock", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            foreach (Thing t in Current.Game.VisibleMap.spawnedThings)
            {
                if (t is Building_Dresser)
                {
                    foreach (Thing apparel in ((Building_Dresser)t).StoredApparel)
                    {
                        l.TryAdd(apparel, false);
                    }
                }
            }
        }
    }*/
}
