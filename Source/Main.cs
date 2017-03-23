using ChangeDresser.UI.DTO.StorageDTOs;
using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
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

            BattleApparelGroupDTO.ShowForceBattleSwitch = true;
            Log.Message("ChangeDresser: Adding Harmony Postfix to Pawn_DraftController.Drafted { set }");

            Building_Dresser.DefaultStoragePriority = StoragePriority.Important;
            Log.Message("ChangeDresser: Adding Harmony Postfix to JobGiver_OptimizeApparel.TryGiveJob(Pawn)");
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    static class Patch_Pawn_DraftController
    {
        static void Postfix(Pawn_DraftController __instance)
        {
            StorageGroupDTO storageGroupDto;
            if (BattleApparelGroupDTO.TryGetBattleApparelGroupForPawn(__instance.pawn, out storageGroupDto))
            {
                if ((__instance.Drafted && !storageGroupDto.IsBeingWorn) ||
                    (!__instance.Drafted && storageGroupDto.IsBeingWorn))
                {
                    storageGroupDto.SwapWith(__instance.pawn);
                }
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
}
