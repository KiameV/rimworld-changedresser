using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace ChangeDresser.DresserJobDriver
{
    internal class JobDriver_WearApparelFromStorage : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
#if DEBUG || DEBUG_TRACKER
                    Log.Message(System.Environment.NewLine + "Begin JobDriver_WearApparelFromStorage");
#endif
                    Building_Dresser dresser = (Building_Dresser)this.TargetA.Thing;
                    Pawn pawn = this.GetActor();

                    Thing t = this.TargetB.Thing;
                    if (t is Apparel a && 
                        dresser.RemoveNoDrop(a))
                    {
                        List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);
                        foreach (Apparel w in worn)
                        {
#if DEBUG || DEBUG_TRACKER
                            Log.Warning(" Remove " + w.Label);
#endif
                            if (Settings.KeepForcedApparel && pawn.outfits.forcedHandler.IsForced(w))
                            {
                                continue;
                            }
                            pawn.apparel.Remove(w);
                        }

#if DEBUG || DEBUG_TRACKER
                        Log.Warning(" Thing to wear as new: " + t.Label);
#endif
                        pawn.apparel.Wear((Apparel)t, true);

                        foreach (Apparel w in worn)
                        {
                            if (pawn.apparel.CanWearWithoutDroppingAnything(w.def))
                            {
                                pawn.apparel.Wear(w);
                            }
                            else
                            {
                                WorldComp.AddApparel(w);
                            }
                        }
                        
                        /*PawnOutfits outfits;
                        if (WorldComp.PawnOutfits.TryGetValue(pawn, out outfits))
                        {
                            outfits.ColorApparel(pawn);
                        }*/
                    }
#if DEBUG || DEBUG_TRACKER
                    Log.Message("End JobDriver_WearApparelFromStorage" + System.Environment.NewLine);
#endif
                }
            };
            yield break;
        }
    }
}
