using ChangeDresser.UI.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace ChangeDresser
{
    class Building_ChangeMirror : Building
    {

        public static IEnumerable<CurrentEditorEnum> GetSupportedEditors(bool isAlien)
        {
            yield return CurrentEditorEnum.ChangeDresserApparelColor;

            if (Settings.IncludeColorByLayer)
            {
                yield return CurrentEditorEnum.ChangeDresserApparelLayerColor;
            }

            yield return CurrentEditorEnum.ChangeDresserHair;

            if (Settings.ShowBodyChange)
            {
                if (isAlien)
                {
                    yield return CurrentEditorEnum.ChangeDresserAlienSkinColor;
                }
                yield return CurrentEditorEnum.ChangeDresserBody;
            }

            if (ModsConfig.IdeologyActive)
                yield return CurrentEditorEnum.ChangeDresserFavoriteColor;
        }

        [DebuggerHidden]
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            bool isAlien = AlienRaceUtil.IsAlien(pawn);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if (pawn.apparel.WornApparel.Count > 0)
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeApparelColors".Translate(),
                    delegate
                    {
                        Job job = new Job(JobDefOfCD.ChangeApparelColor, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            if (!isAlien || AlienRaceUtil.HasHair(pawn))
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeHair".Translate(),
                    delegate
                    {
                        Job job = new Job(JobDefOfCD.ChangeHairStyle, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            if (Settings.ShowBodyChange)
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeBody".Translate(),
                    delegate
                    {
                        Job job = new Job(JobDefOfCD.ChangeBody, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));

                if (isAlien)
                {
                    list.Add(new FloatMenuOption(
                        "ChangeDresser.ChangeAlienBodyColor".Translate(),
                        delegate
                        {
                            Job job = new Job(JobDefOfCD.ChangeBodyAlienColor, this);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }));
                }
                if (ModsConfig.IdeologyActive)
                {
                    list.Add(new FloatMenuOption(
                        "ChangeDresser.ChangeFavoriteColor".Translate(),
                        delegate
                        {
                            Job job = new Job(JobDefOfCD.ChangeFavoriteColor, this);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }));
                }
            }

            if (ModsConfig.IdeologyActive)
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeFavoriteColor".Translate(),
                    delegate
                    {
                        Job job = new Job(JobDefOfCD.ChangeFavoriteColor, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));

            }
            return list;
        }
    }
}