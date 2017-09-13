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
            
            Log.Message("ChangeDresser: Adding Harmony Postfix to Pawn_DraftController.Drafted { set }");
            Log.Message("ChangeDresser: Adding Harmony Postfix to JobGiver_OptimizeApparel.TryGiveJob(Pawn)");
        }

        public static void SwapApparel(Pawn pawn, Outfit toWear)
        {
            // Remove apparel from pawn
            List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);
            foreach (Apparel a in worn)
            {
                pawn.apparel.Remove(a);

                bool handled = false;
                foreach (Building_Dresser d in WorldComp.DressersToUse)
                {
                    if (d.settings.filter.Allows(a))
                    {
                        d.AddApparel(a);
                        handled = true;
                        break;
                    }
                }
                if (!handled)
                {
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

            foreach (ThingDef def in toWear.filter.AllowedThingDefs)
            {
                if (pawn.apparel.CanWearWithoutDroppingAnything(def))
                {
                    foreach (Building_Dresser d in WorldComp.DressersToUse)
                    {
                        Apparel apparel;
                        if (d.TryRemoveBestApparel(def, toWear.filter, out apparel))
                        {
                            pawn.apparel.Wear(apparel);
                            break;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class Pawn_GetGizmos
    {
#if DEBUG
        private static int i = 0;
        private static readonly int WAIT = 4000;
#endif
        static void Postfix(Pawn_DraftController __instance, ref IEnumerable<Gizmo> __result)
        {
            Pawn pawn = __instance.pawn;
            if (!pawn.Drafted)
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
                    foreach (OutfitType o in outfits.OutfitTypes)
                    {
#if DEBUG
                        if (i == WAIT)
                            Log.Warning("DraftController.Postfix: Set: " + s.Name + ", forBattle: " + s.ForBattle + ", isBeingWorn: " + s.IsBeingWorn);
#endif
                        if (!o.ForBattle && !pawn.outfits.CurrentOutfit.Equals(o.Outfit))
                        {
                            Command_Action a = new Command_Action();
                            /*string texPath = "";
                            if (texPath != null)
                            {
                                a.icon = ContentFinder<UnityEngine.Texture2D>.Get(texPath, true);
                            }*/
                            a.icon = ContentFinder<UnityEngine.Texture2D>.Get(new List<ThingDef>(o.Outfit.filter.AllowedThingDefs)[0].graphicData.texPath, true);
                            StringBuilder sb = new StringBuilder("ChangeDresser.ChangeTo".Translate());
                            sb.Append(" ");
                            sb.Append(o.Outfit.label);
                            a.defaultLabel = sb.ToString();
                            a.defaultDesc = "ChangeDresser.ChangeToDesc";
                            a.activateSound = SoundDef.Named("Click");
                            a.action = delegate
                            {
                                Main.SwapApparel(pawn, o.Outfit);
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
        private static readonly int WAIT = 4000;
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
                    foreach (OutfitType o in outfits.OutfitTypes)
                    {
#if DEBUG
                        if (i == WAIT)
                            Log.Warning("DraftController.Postfix: Set: " + s.Name + ", forBattle: " + s.ForBattle + ", isBeingWorn: " + s.IsBeingWorn);
#endif
                        if (o.ForBattle && !pawn.outfits.CurrentOutfit.Equals(o.Outfit))
                        {
                            Command_Action a = new Command_Action();
                            /*string texPath = "";
                            if (texPath != null)
                            {
                                a.icon = ContentFinder<UnityEngine.Texture2D>.Get(texPath, true);
                            }*/
                            a.icon = ContentFinder<UnityEngine.Texture2D>.Get(new List<ThingDef>(o.Outfit.filter.AllowedThingDefs)[0].graphicData.texPath, true);
                            StringBuilder sb = new StringBuilder("ChangeDresser.ChangeTo".Translate());
                            sb.Append(" ");
                            sb.Append(o.Outfit.label);
                            a.defaultLabel = sb.ToString();
                            a.defaultDesc = "ChangeDresser.ChangeToDesc";
                            a.activateSound = SoundDef.Named("Click");
                            a.action = delegate
                            {
                                Main.SwapApparel(pawn, o.Outfit);
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
                    if (outfits.TryGetBattleOutfit(out outfitToWear))
                    {
                        outfits.LastCivilianOutfit = pawn.outfits.CurrentOutfit;
                        found = true;
                    }
                }
                else
                {
                    if (outfits.TryGetCivilianOutfit(out outfitToWear))
                    {
                        outfits.LastBattleOutfit = pawn.outfits.CurrentOutfit;
                        found = true;
                    }
                }

                if (found)
                {
                    Main.SwapApparel(pawn, outfitToWear);
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

                if (score > baseApparelScore)
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
    }

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
