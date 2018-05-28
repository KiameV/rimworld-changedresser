using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ChangeDresser
{
    public class SettingsController : Mod
    {
        public SettingsController(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "ChangeDresser".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }

    public class Settings : ModSettings
    {
        private static bool showGenderAgeChange = true;
        private static bool showBodyChange = true;
        private static bool includeColorByLayer = true;

        public static bool ShowGenderAgeChange { get { return showGenderAgeChange; } }
        public static bool ShowBodyChange { get { return showBodyChange; } }
        public static bool KeepForcedApparel { get { return true; } }
        public static bool IncludeColorByLayer { get { return includeColorByLayer; } }
        public static int RepairAttachmentDistance { get { return 6; } }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref showGenderAgeChange, "ChangeDresser.ShowGenderAgeChange", true, true);
            Scribe_Values.Look<bool>(ref showBodyChange, "ChangeDresser.ShowBodyChange", true, true);
            Scribe_Values.Look<bool>(ref includeColorByLayer, "ChangeDresser.IncludeColorByLayer", true, true);
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard l = new Listing_Standard(GameFont.Small);
            l.ColumnWidth = System.Math.Min(400, rect.width / 2);
            l.Begin(rect);
            l.CheckboxLabeled("ChangeDresser.IncludeColorByLayer".Translate(), ref includeColorByLayer);
            l.Gap(4);
            l.CheckboxLabeled("ChangeDresser.ShowBodyChange".Translate(), ref showBodyChange);
            if (showBodyChange)
            {
                l.Gap(4);
                l.CheckboxLabeled("ChangeDresser.ShowGenderAgeChange".Translate(), ref showGenderAgeChange);
                l.Gap(20);
            }
            else
            {
                l.Gap(48);
            }
            l.End();
        }
    }
}
