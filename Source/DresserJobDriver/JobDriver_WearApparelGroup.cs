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
using ChangeDresser.UI.DTO.StorageDTOs;

namespace ChangeDresser.DresserJobDriver
{
    internal class JobDriver_WearApparelGroup : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    Building_Dresser dresser = (Building_Dresser)base.CurJob.targetA;
                    string apparelGroupName = ((SwapApparelJob)base.CurJob).ApparelGroupName;
                    Pawn pawn = this.GetActor();

                    StorageGroupDTO dto;
                    if (!dresser.TryGetStorageGroup(pawn, apparelGroupName, out dto))
                    {
                        Messages.Message(pawn.Name.ToStringShort + " " + "ChangeDresser.UnableToWearApparelGroup".Translate() + " " + apparelGroupName + ".", MessageSound.Negative);
                    }
                    else
                    {
                        dto.SwapWith(pawn);
                    }
                }
            };
            yield break;
        }
    }
}
