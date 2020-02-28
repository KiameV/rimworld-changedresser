using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    partial class HarmonyPatches
    {
        struct StoredApparel
        {
            public readonly Building_Dresser Dresser;
            public readonly Apparel Apparel;
            public StoredApparel(Building_Dresser dresser, Apparel apparel)
            {
                this.Dresser = dresser;
                this.Apparel = apparel;
            }
        }

        struct ApparelToUse
        {
            public readonly List<StoredApparel> Apparel;
            public readonly int Count;
            public ApparelToUse(List<StoredApparel> apparel, int count)
            {
                this.Apparel = apparel;
                this.Count = count;
            }
        }

        class NeededIngrediants
        {
            public readonly ThingFilter Filter;
            public int Count;
            public readonly Dictionary<Def, List<StoredApparel>> FoundThings;

            public NeededIngrediants(ThingFilter filter, int count)
            {
                this.Filter = filter;
                this.Count = count;
                this.FoundThings = new Dictionary<Def, List<StoredApparel>>();
            }
            public void Add(StoredApparel things)
            {
                List<StoredApparel> l;
                if (!this.FoundThings.TryGetValue(things.Apparel.def, out l))
                {
                    l = new List<StoredApparel>();
                    this.FoundThings.Add(things.Apparel.def, l);
                }
                l.Add(things);
            }
            public void Clear()
            {
                this.FoundThings.Clear();
            }
            public bool CountReached()
            {
                foreach (List<StoredApparel> l in this.FoundThings.Values)
                {
                    if (this.CountReached(l))
                        return true;
                }
                return false;
            }
            private bool CountReached(List<StoredApparel> l)
            {
                int count = this.Count;
                foreach (StoredApparel st in l)
                {
                    count -= st.Apparel.stackCount;
                }
                return count <= 0;
            }
            public List<StoredApparel> GetFoundThings()
            {
                foreach (List<StoredApparel> l in this.FoundThings.Values)
                {
                    if (this.CountReached(l))
                    {
#if DEBUG
                        Log.Warning("Count [" + Count + "] reached with: " + l[0].Apparel.def.label);
#endif
                        return l;
                    }
                }
                return null;
            }
        }

        [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
        static class Patch_WorkGiver_DoBill_TryFindBestBillIngredients
        {
            static void Postfix(ref bool __result, Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen)
            {
                if (bill.Map == null)
                {
                    Log.Error("Bill's map is null");
                    return;
                }

                if (__result == true || !WorldComp.HasDressers(bill.Map) || bill.Map != pawn.Map)
                    return;

#if DEBUG && (DROP_DEBUG || BILL_DEBUG)
            Log.Warning("TryFindBestBillIngredients.Postfix __result: " + __result);
#endif
                Dictionary<ThingDef, int> chosenAmounts = new Dictionary<ThingDef, int>();
                foreach (ThingCount c in chosen)
                {
                    int count;
                    if (chosenAmounts.TryGetValue(c.Thing.def, out count))
                    {
                        count += c.Count;
                    }
                    else
                    {
                        count = c.Count;
                    }
                    chosenAmounts[c.Thing.def] = count;
                }

#if DEBUG && (DROP_DEBUG || BILL_DEBUG)
            Log.Warning("    ChosenAmounts:");
            foreach (KeyValuePair<ThingLookup, int> kv in chosenAmounts)
            {
                Log.Warning("        " + kv.Key.Def.label + " - " + kv.Value);
            }
#endif

                LinkedList<NeededIngrediants> neededIngs = new LinkedList<NeededIngrediants>();
                foreach (IngredientCount ing in bill.recipe.ingredients)
                {
                    bool found = false;
                    foreach (KeyValuePair<ThingDef, int> kv in chosenAmounts)
                    {
                        if ((int)ing.GetBaseCount() == kv.Value)
                        {
#if DEBUG && (DROP_DEBUG || BILL_DEBUG)
                        Log.Warning("    Needed Ing population count is the same");
#endif
                            if (ing.filter.Allows(kv.Key))
                            {
#if DEBUG && (DROP_DEBUG || BILL_DEBUG)
                            Log.Warning("    Needed Ing population found: " + kv.Key.Def.label + " count: " + kv.Value);
#endif
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
#if DEBUG && (DROP_DEBUG || BILL_DEBUG)
                    Log.Warning("    Needed Ing population not found");
#endif
                        neededIngs.AddLast(new NeededIngrediants(ing.filter, (int)ing.GetBaseCount()));
                    }
                }

#if DEBUG && (DROP_DEBUG || BILL_DEBUG)
            Log.Warning("    Needed Ings:");
            foreach (NeededIngrediants ings in neededIngs)
            {
                Log.Warning("        " + ings.Count);
            }
#endif

                List<ApparelToUse> apparelToUse = new List<ApparelToUse>();
                foreach (Building_Dresser dresser in WorldComp.GetDressers(bill.Map))
                {
                    if ((float)(dresser.Position - billGiver.Position).LengthHorizontalSquared < Math.Pow(bill.ingredientSearchRadius, 2))
                    {
                        LinkedListNode<NeededIngrediants> n = neededIngs.First;
                        while (n != null)
                        {
                            var next = n.Next;
                            NeededIngrediants neededIng = n.Value;

                            if (dresser.TryGetFilteredApparel(bill, neededIng.Filter, out List<Apparel> gotten))
                            {
                                foreach (Apparel got in gotten)
                                {
                                    neededIng.Add(new StoredApparel(dresser, got));
                                }
                                if (neededIng.CountReached())
                                {
                                    apparelToUse.Add(new ApparelToUse(neededIng.GetFoundThings(), neededIng.Count));
                                    neededIng.Clear();
                                    neededIngs.Remove(n);
                                }
                            }
                            n = next;
                        }
                    }
                }

#if DEBUG && (DROP_DEBUG || BILL_DEBUG)
            Log.Warning("    neededIngs.count: " + neededIngs.Count);
#endif

                if (neededIngs.Count == 0)
                {
                    __result = true;
                    foreach (ApparelToUse ttu in apparelToUse)
                    {
                        int count = ttu.Count;
                        foreach (StoredApparel sa in ttu.Apparel)
                        {
                            if (count <= 0)
                                break;
                            
                            if (sa.Dresser.TryRemove(sa.Apparel, false))
                            {
                                count -= sa.Apparel.stackCount;
                                chosen.Add(new ThingCount(sa.Apparel, sa.Apparel.stackCount));
                            }
                        }
                    }
                }

                apparelToUse.Clear();
                foreach (NeededIngrediants n in neededIngs)
                    n.Clear();
                neededIngs.Clear();
                chosenAmounts.Clear();
            }
        }
    }
}