using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;
using System;

namespace ChangeDresser
{
    public static class ApparelUtil
    {
        public static List<Apparel> RemoveApparel(Pawn pawn)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin ApparelUtil.RemoveApparel(Pawn: " + pawn.Name.ToStringShort + ")");
            Log.Message("    Remove Apparel:");
#endif
            List<Apparel> wornApparel = new List<Apparel>(pawn.apparel.WornApparel);
            foreach (Apparel a in wornApparel)
            {
#if DRESSER_OUTFIT
                Log.Message("        " + a.Label);
#endif
                pawn.apparel.Remove(a);
            }
            pawn.outfits.forcedHandler.ForcedApparel.Clear();
#if DRESSER_OUTFIT
            Log.Warning("End ApparelUtil.RemoveApparel Removed Count: " + wornApparel.Count);
#endif
            return wornApparel;
        }

        public static void StoreApparelInWorldDresser(List<Apparel> apparel, Pawn pawn)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin ApparelUtil.StoreApparelInWorldDresser(Pawn: " + pawn.Name.ToStringShort + ")");
            Log.Message("    Store Apparel in World Dressers:");
#endif
            foreach (Apparel a in apparel)
            {
#if DRESSER_OUTFIT
                Log.Message("        " + a.Label);
#endif
                if (!WorldComp.AddApparel(a))
                {
#if DRESSER_OUTFIT
                    Log.Warning("            Unable to place apparel in dresser, dropping to floor");
#endif
                    BuildingUtil.DropThing(a, pawn.Position, pawn.Map, false);
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End ApparelUtil.StoreApparelInWorldDresser");
#endif
        }

        public static void OptimizeApparel(Pawn pawn)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin OptimizeApparelUtil.OptimizeApparel(Pawn: " + pawn.Name + ")");
#endif
            MethodInfo mi = typeof(JobGiver_OptimizeApparel).GetMethod("TryGiveJob", BindingFlags.Instance | BindingFlags.NonPublic);
            JobGiver_OptimizeApparel apparelOptimizer = new JobGiver_OptimizeApparel();
            object[] param = new object[] { pawn };

            for (int i = 0; i < 10; ++i)
            {
#if TRACE && DRESSER_OUTFIT
                Log.Message(i + " start equip for loop");
#endif
                Job job = mi.Invoke(apparelOptimizer, param) as Job;
#if TRACE && DRESSER_OUTFIT
                Log.Message(i + " job is null: " + (string)((job == null) ? "yes" : "no"));
#endif
                if (job == null)
                    break;
#if TRACE && DRESSER_OUTFIT
                Log.Message(job.def.defName);
#endif
                if (job.def == JobDefOf.Wear)
                {
                    Apparel a = ((job.targetB != null) ? job.targetB.Thing : null) as Apparel;
                    if (a == null)
                    {
                        Log.Warning("ChangeDresser: OptimizeApparelUtil.OptimizeApparel: Problem equiping pawn. Apparel is null.");
                        break;
                    }
#if TRACE && DRESSER_OUTFIT
                    Log.Message("Wear from ground " + a.Label);
#endif
                    pawn.apparel.Wear(a);
                }
                else if (job.def == Building_Dresser.WEAR_APPAREL_FROM_DRESSER_JOB_DEF)
                {
                    Building_Dresser d = ((job.targetA != null) ? job.targetA.Thing : null) as Building_Dresser;
                    Apparel a = ((job.targetB != null) ? job.targetB.Thing : null) as Apparel;

                    if (d == null || a == null)
                    {
                        Log.Warning("ChangeDresser: OptimizeApparelUtil.OptimizeApparel: Problem equiping pawn. Dresser or Apparel is null.");
                        break;
                    }
#if TRACE && DRESSER_OUTFIT
                    Log.Message("Wear from dresser " + d.Label + " " + a.Label);
#endif
                    d.RemoveNoDrop(a);
                    pawn.apparel.Wear(a);
                }
#if TRACE && DRESSER_OUTFIT
                Log.Message(i + " end equip for loop");
#endif
            }
#if DRESSER_OUTFIT
            Log.Warning("End OptimizeApparelUtil.OptimizeApparel");
#endif
        }

        public static bool FindBetterApparel(
            ref float baseApparelScore, ref Apparel betterApparel, Pawn pawn, Outfit currentOutfit, IEnumerable<Apparel> apparelToCheck, Building dresser)
        {
            bool result = false;
            foreach (Apparel apparel in apparelToCheck)
            {
                if (!currentOutfit.filter.Allows(apparel.def))
                {
                    break;
                }
                if (!currentOutfit.filter.Allows(apparel) ||
                    apparel.IsForbidden(pawn))
                {
                    continue;
                }

                if (Settings.KeepForcedApparel)
                {
                    bool skipApparelType = false;
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    for (int i = 0; i < wornApparel.Count; i++)
                    {
                        if (!ApparelUtility.CanWearTogether(wornApparel[i].def, apparel.def, pawn.RaceProps.body) &&
                            !pawn.outfits.forcedHandler.IsForced(wornApparel[i]))
                        {
                            skipApparelType = true;
                            break;
                        }
                    }
                    if (skipApparelType)
                    {
                        break;
                    }
                }

                float gain = JobGiver_OptimizeApparel.ApparelScoreGain(pawn, apparel);
                if (gain >= 0.05f && gain > baseApparelScore)
                {
                    if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
                    {
                        if (dresser == null || 
                            ReservationUtility.CanReserveAndReach(pawn, dresser, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1))
                        {
                            betterApparel = apparel;
                            baseApparelScore = gain;
                            result = true;
                        }
                    }
                }
            }
            return result;
        }
        /*
        public static bool TryFindBestApparel(Pawn pawn, out Apparel a, out Building_Dresser dresser)
        {
#if BETTER_OUTFIT
            Log.Warning("Begin WorldComp.TryFindBestApparel(Pawn: " + pawn.Name.ToStringShort);
#endif
            a = null;
            dresser = null;
            float baseApparelScore = 0;

            PawnOutfitTracker po;
            if (PawnOutfits.TryGetValue(pawn, out po))
            {
                ApparelUtil.FindBetterApparel(ref baseApparelScore, ref a, pawn, pawn.outfits.CurrentOutfit, po.CustomApparel, null);
#if BETTER_OUTFIT
                Log.Warning("    CustomApparel Result: " + ((a == null) ? "<null>" : a.Label) + "    Score: " + baseApparelScore);
#endif
            }

            foreach (Building_Dresser d in DressersToUse)
            {
                if (d.FindBetterApparel(ref baseApparelScore, ref a, pawn, pawn.outfits.CurrentOutfit))
                {
                    dresser = d;
                }
            }
#if BETTER_OUTFIT
            Log.Warning("    Dresser Result: " + ((a == null) ? "<null>" : a.Label) + "    Score: " + baseApparelScore);
#endif

#if BETTER_OUTFIT
            Log.Warning("Begin WorldComp.TryFindBestApparel -- " + ((a == null) ? "<null>" : a.Label));
#endif
            return a != null;
        }*/
    }
}