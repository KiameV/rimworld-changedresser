/*
 * MIT License
 * 
 * Copyright (c) [2017] [Travis Offtermatt]
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace ChangeDresser.DresserJobDriver
{
    internal class JobDriver_WearApparelFromStorage : JobDriver
    {
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
                    if (t is Apparel && 
                        dresser.RemoveNoDrop((Apparel)t))
                    {
                        List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);
                        foreach (Apparel w in worn)
                        {
#if DEBUG || DEBUG_TRACKER
                            Log.Warning(" Remove " + w.Label);
#endif
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
                                dresser.AddApparel(w);
                            }
                        }
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
