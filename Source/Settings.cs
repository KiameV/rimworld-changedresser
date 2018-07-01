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
        private const int DEFAULT_MENDING_SPEED = 1;

        private static bool showGenderAgeChange = true;
        private static bool showBodyChange = true;
        private static bool includeColorByLayer = true;
        private static bool persistApparelOriginalColor = true;
        private static bool shareHairAcrossGenders = false;
        private static int mendingAttachmentMendingSpeed = DEFAULT_MENDING_SPEED;
        private static string mendingAttachmentMendingSpeedBuffer = DEFAULT_MENDING_SPEED.ToString();

        public static bool ShowGenderAgeChange { get { return showGenderAgeChange; } }
        public static bool ShowBodyChange { get { return showBodyChange; } }
        public static bool KeepForcedApparel { get { return true; } }
        public static bool IncludeColorByLayer { get { return includeColorByLayer; } }
        public static int RepairAttachmentDistance { get { return 6; } }
        public static bool PersistApparelOriginalColor { get { return persistApparelOriginalColor; } }
        public static bool ShareHairAcrossGenders { get { return shareHairAcrossGenders; } }
        public static int MendingAttachmentMendingSpeed { get { return mendingAttachmentMendingSpeed; } }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref showGenderAgeChange, "ChangeDresser.ShowGenderAgeChange", true, true);
            Scribe_Values.Look<bool>(ref showBodyChange, "ChangeDresser.ShowBodyChange", true, true);
            Scribe_Values.Look<bool>(ref includeColorByLayer, "ChangeDresser.IncludeColorByLayer", true, true);
            Scribe_Values.Look<bool>(ref persistApparelOriginalColor, "ChangeDresser.PersistApparelOriginalColor", false, true);
            Scribe_Values.Look<bool>(ref persistApparelOriginalColor, "ChangeDresser.ShareHairAcrossGenders", false, false);
            Scribe_Values.Look<int>(ref mendingAttachmentMendingSpeed, "ChangeDresser.MendingAttachmentMendingSpeed", DEFAULT_MENDING_SPEED, false);
            mendingAttachmentMendingSpeedBuffer = mendingAttachmentMendingSpeed.ToString();
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            bool origPersistColors = persistApparelOriginalColor;

            Listing_Standard l = new Listing_Standard(GameFont.Small);
            l.ColumnWidth = System.Math.Min(400, rect.width / 2);
            l.Begin(rect);
            l.CheckboxLabeled("ChangeDresser.IncludeColorByLayer".Translate(), ref includeColorByLayer);
            l.Gap(4);
            l.CheckboxLabeled("ChangeDresser.PersistApparelOriginalColor".Translate(), ref persistApparelOriginalColor);
            l.Gap(4);
            l.TextFieldNumericLabeled<int>("ChangeDresser.MendingAttachmentMendingSpeed".Translate(), ref mendingAttachmentMendingSpeed, ref mendingAttachmentMendingSpeedBuffer, 1, 100);
            if (l.ButtonText("ResetButton".Translate()))
            {
                mendingAttachmentMendingSpeed = DEFAULT_MENDING_SPEED;
                mendingAttachmentMendingSpeedBuffer = DEFAULT_MENDING_SPEED.ToString();
            }
            l.Gap(6);
            l.CheckboxLabeled("ChangeDresser.ShareHairAcrossGenders".Translate(), ref shareHairAcrossGenders);
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

            if (origPersistColors != persistApparelOriginalColor &&
                Current.Game != null && WorldComp.ApparelColorTracker != null)
            {
                if (persistApparelOriginalColor)
                {
                    WorldComp.ApparelColorTracker.PersistWornColors();
                }
                else
                {
                    WorldComp.ApparelColorTracker.Clear();
                }
            }
        }
    }
}
