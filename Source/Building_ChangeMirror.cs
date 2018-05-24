using ChangeDresser.UI.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace ChangeDresser
{
    class Building_ChangeMirror : Building
    {
        private JobDef changeApparelColorJobDef = DefDatabase<JobDef>.GetNamed("ChangeApparelColor", true);
        private JobDef changeHairStyleJobDef = DefDatabase<JobDef>.GetNamed("ChangeHairStyle", true);
        private JobDef changeBodyJobDef = DefDatabase<JobDef>.GetNamed("ChangeBody", true);
        public readonly JobDef changeBodyAlienColor = DefDatabase<JobDef>.GetNamed("ChangeBodyAlienColor", true);

        public static readonly List<CurrentEditorEnum> SupportedEditors = new List<CurrentEditorEnum>(3);

        static Building_ChangeMirror()
        {
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserApparelColor);
            if (Settings.IncludeColorByLayer)
                SupportedEditors.Add(CurrentEditorEnum.ChangeDresserApparelLayerColor);
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserHair);
            SupportedEditors.Add(CurrentEditorEnum.ChangeDresserBody);
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
                        Job job = new Job(this.changeApparelColorJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            if (!isAlien || AlienRaceUtil.HasHair(pawn))
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeHair".Translate(),
                    delegate
                    {
                        Job job = new Job(this.changeHairStyleJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            if (Settings.ShowBodyChange)
            {
                list.Add(new FloatMenuOption(
                    "ChangeDresser.ChangeBody".Translate(),
                    delegate
                    {
                        Job job = new Job(this.changeBodyJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));

                if (isAlien)
                {
                    list.Add(new FloatMenuOption(
                        "ChangeDresser.ChangeAlienBodyColor".Translate(),
                        delegate
                        {
                            Job job = new Job(this.changeBodyAlienColor, this);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }));
                }
            }
            return list;
        }
    }
}