using System.Collections.Generic;
using Verse;
using Verse.AI;
using ChangeDresser.UI;
using ChangeDresser.UI.Enums;
using ChangeDresser.UI.DTO;

namespace ChangeDresser.DresserJobDriver
{
    internal class JobDriver_ChangeHairStyle : JobDriver
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
                    Find.WindowStack.Add(new DresserUI(DresserDtoFactory.Create(this.GetActor(), base.job, CurrentEditorEnum.ChangeDresserHair)));
                }
            };
            yield break;
        }
    }
}
