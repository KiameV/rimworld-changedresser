using System.Collections.Generic;
using Verse;
using Verse.AI;
using ChangeDresser.UI;
using ChangeDresser.UI.Enums;
using ChangeDresser.UI.DTO;

namespace ChangeDresser.DresserJobDriver
{
    internal class JobDriver_ChangeSkinColor : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    Find.WindowStack.Add(new DresserUI(new DresserDTO(this.GetActor(), CurrentEditorEnum.SkinColor)));
                }
            };
            yield break;
        }
    }
}
