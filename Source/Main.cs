using ChangeDresser.UI.Util;
using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
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
            
            Log.Message("ChangeDresser: Adding Harmony Postfix to Pawn.GetGizmos");
            Log.Message("ChangeDresser: Adding Harmony Postfix to Pawn_ApparelTracker.Notify_ApparelAdded");
            Log.Message("ChangeDresser: Adding Harmony Postfix to Pawn_DraftController.Drafted { set }");
            Log.Message("ChangeDresser: Adding Harmony Postfix to Pawn_DraftController.GetGizmos");
            Log.Message("ChangeDresser: Adding Harmony Postfix to JobGiver_OptimizeApparel.TryGiveJob(Pawn)");
        }

        public static void SwapApparel(Pawn pawn, Outfit toWear)
        {
#if DEBUG
            Log.Message(
                Environment.NewLine + 
                "Start Main.SwapApparel Pawn: " + pawn.Name.ToStringShort + " toWear: " + toWear.label);
#endif
            // Remove apparel from pawn
            List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);
            foreach (Apparel a in worn)
            {
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

            pawn.outfits.CurrentOutfit = toWear;
#if DEBUG
            Log.Message("End Main.SwapApparel" + Environment.NewLine);
#endif
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class Pawn_GetGizmos
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
                            /*string texPath = "";
                            if (texPath != null)
                            {
                                a.icon = ContentFinder<UnityEngine.Texture2D>.Get(texPath, true);
                            }*/
                            List<ThingDef> tdList = new List<ThingDef>(o.filter.AllowedThingDefs);
                            Texture2D tex = null;
                            if (tdList.Count > 0)
                            {
                                tex = ContentFinder<Texture2D>.Get(tdList[0].graphicData.texPath, true);
                            }
                            if (tex == null)
                            {
                                tex = WidgetUtil.noneTexture;
                            }
                            a.icon = tex;
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
                                Main.SwapApparel(__instance, o);
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
                            /*string texPath = "";
                            if (texPath != null)
                            {
                                a.icon = ContentFinder<UnityEngine.Texture2D>.Get(texPath, true);
                            }*/
                            List<ThingDef> tdList = new List<ThingDef>(o.filter.AllowedThingDefs);
                            Texture2D tex = null;
                            if (tdList.Count > 0)
                            {
                                tex = ContentFinder<Texture2D>.Get(tdList[0].graphicData.texPath, true);
                            }
                            if (tex == null)
                            {
                                tex = WidgetUtil.noneTexture;
                            }
                            a.icon = tex;
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
                                Main.SwapApparel(pawn, o);
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

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    static class Patch_Pawn_ApparelTracker_Notify_ApparelAdded
    {
        struct LastTimeAndTries
        {
            public int Tries;
            public long LastTime;
            public LastTimeAndTries(int tries, long lastTime)
            {
                this.Tries = tries;
                this.LastTime = lastTime;
            }
        }
        static Dictionary<Pawn, LastTimeAndTries> lastTimeAndTries = new Dictionary<Pawn, LastTimeAndTries>();
        static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
#if DEBUG || DEBUG_TRACKER
            Log.Message(Environment.NewLine + "Start Pawn_ApparelTracker.Notify_ApparelAdded");
#endif
            long now = DateTime.Now.Ticks;
            LastTimeAndTries i;
            if (lastTimeAndTries.TryGetValue(__instance.pawn, out i))
            {
                long delta = now - i.LastTime;
                if (delta < TimeSpan.TicksPerMinute)
                {
                    if (i.Tries >= 8)
                    {
#if DEBUG || DEBUG_TRACKER
                        Log.Warning(__instance.pawn.Name.ToStringShort + " reached the maximum number of tried in a minute");
#endif
                        return;
                    }
                    else // i.Tries < 8
                    {
#if DEBUG || DEBUG_TRACKER
                        Log.Warning(__instance.pawn.Name.ToStringShort + " try count: " + i);
#endif
                        ++i.Tries;
                    }
                }
                else
                {
#if DEBUG || DEBUG_TRACKER
                    Log.Warning(__instance.pawn.Name.ToStringShort + " try reset");
#endif
                    i.Tries = 1;
                    i.LastTime = now;
                }
            }
            else
            {
                i = new LastTimeAndTries(1, now);
            }

            PawnOutfits po;
            if (WorldComp.PawnOutfits.TryGetValue(__instance.pawn, out po))
            {
#if DEBUG
                Log.Warning(" po found");
#endif
                Color c;
                if (po.TryGetColorFor(apparel.def.apparel.LastLayer, out c))
                {
#if DEBUG
                    Log.Warning(" assigned color for layer " + apparel.def.apparel.LastLayer);
#endif
                    CompColorableUtility.SetColor(apparel, c, true);
                    __instance.pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    PortraitsCache.SetDirty(__instance.pawn);
                }
#if DEBUG
                else
                {
                    Log.Warning(" no assigned color for layer " + apparel.def.apparel.LastLayer);
                }
#endif
            }
#if DEBUG || DEBUG_TRACKER
            Log.Message("End Pawn_ApparelTracker.Notify_ApparelAdded" + Environment.NewLine);
#endif
        }
    }

    /*[HarmonyPatch(typeof(Pawn_TraderTracker), "ColonyThingsWillingToBuy")]
    static class Patch_Pawn_TraderTracker_ColonyThingsWillingToBuy
    {
        static void Postfix(IEnumerable<Thing> __result)
        {
            Log.Error("POSTFIX WILLING TO BUY START");
            Map map = Current.Game.VisibleMap;
            if (map != null)
            {
                Log.Error("Map found");
                List<Thing> l = new List<Thing>(__result);
                foreach (Building b in map.listerBuildings.allBuildingsColonist)
                {
                    Building_Dresser d = b as Building_Dresser;
                    if (d != null)
                    {
                        Log.Error("Dresser found " + d.Count);
                        l.AddRange(d.Apparel as List<Thing>);
                    }
                }
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
