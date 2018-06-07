using ChangeDresser;
using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace MendingChangeDresserPatch
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(m => "Mending".Equals(m.Name)))
            {
                var harmony = HarmonyInstance.Create("com.mendingchangedresserpatch.rimworld.mod");

                harmony.PatchAll(Assembly.GetExecutingAssembly());

                Log.Message(
                    "MendingChangeDresserPatch Harmony Patches:" + Environment.NewLine +
                    "  Postfix:" + Environment.NewLine +
                    "    WorkGiver_DoBill.TryFindBestBillIngredients - Priority Last");
            }
        }
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(Mending.WorkGiver_DoBill), "TryFindBestBillIngredients")]
    static class Patch_WorkGiver_DoBill_TryFindBestBillIngredients
    {
        static void Postfix(ref bool __result, Bill bill, Pawn pawn, Thing billGiver, bool ignoreHitPoints, ref Thing chosen)
        {
            if (__result == false && 
                pawn != null && bill != null && bill.recipe != null && 
                bill.Map == pawn.Map &&
                bill.recipe.defName.IndexOf("Apparel") != -1)
            {
                IEnumerable<Building_Dresser> dressers = WorldComp.GetDressers(bill.Map);
                if (dressers == null)
                {
                    Log.Message("MendingChangeDresserPatch failed to retrieve ChangeDressers");
                    return;
                }

                foreach (Building_Dresser dresser in dressers)
                {
                    if ((float)(dresser.Position - billGiver.Position).LengthHorizontalSquared < bill.ingredientSearchRadius * bill.ingredientSearchRadius)
                    {
                        List<Apparel> gotten;
                        if (dresser.TryGetFilteredApparel(bill, bill.recipe.fixedIngredientFilter, out gotten))
                        {
                            Apparel a = gotten[0];
                            dresser.Remove(a, false);
                            if (a.Spawned == false)
                            {
                                Log.Error("Failed to spawn apparel-to-mend [" + a.Label + "] from dresser [" + dresser.Label + "].");
                                __result = false;
                                chosen = null;
                            }
                            else
                            {
                                __result = true;
                                chosen = a;
                            }
                            return;
                        }
                    }
                }
            }
        }
    }
}