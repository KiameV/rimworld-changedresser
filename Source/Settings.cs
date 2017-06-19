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

        public static bool ShowGenderAgeChange { get { return showGenderAgeChange; } }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref showGenderAgeChange, "ChangeDresser.ShowGenderAgeChange", true, false);
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard expr_06 = new Listing_Standard(GameFont.Small);
            expr_06.ColumnWidth = rect.width;
            expr_06.Begin(rect);
            expr_06.CheckboxLabeled("ChangeDresser.ShowGenderAgeChange".Translate(), ref showGenderAgeChange);
            expr_06.End();
        }
    }
}
