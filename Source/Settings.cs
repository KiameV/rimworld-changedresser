using System;
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
        private const float DEFAULT_MENDING_UPDATE_INTERVAL = 5f;

        private static bool showGenderAgeChange = true;
        private static bool showBodyChange = true;
        private static bool includeColorByLayer = true;
        private static bool persistApparelOriginalColor = true;
        private static bool shareHairAcrossGenders = false;
        private static int mendingAttachmentMendingSpeed = DEFAULT_MENDING_SPEED;
        private static string mendingAttachmentMendingSpeedBuffer = DEFAULT_MENDING_SPEED.ToString();
        private static float mendingAttachmentUpdateInterval = DEFAULT_MENDING_UPDATE_INTERVAL;
        private static string mendingAttachmentUpdateIntervalBuffer = DEFAULT_MENDING_UPDATE_INTERVAL.ToString();

        public static bool ShowGenderAgeChange { get { return showGenderAgeChange; } }
        public static bool ShowBodyChange { get { return showBodyChange; } }
        public static bool KeepForcedApparel { get { return true; } }
        public static bool IncludeColorByLayer { get { return includeColorByLayer; } }
        public static int RepairAttachmentDistance { get { return 6; } }
        public static bool PersistApparelOriginalColor { get { return persistApparelOriginalColor; } }
        public static bool ShareHairAcrossGenders { get { return shareHairAcrossGenders; } }
        public static int MendingAttachmentMendingSpeed { get { return mendingAttachmentMendingSpeed; } }
        public static long MendingAttachmentUpdateIntervalTicks { get { return (long)(mendingAttachmentUpdateInterval * TimeSpan.TicksPerSecond); } }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref showGenderAgeChange, "ChangeDresser.ShowGenderAgeChange", true, true);
            Scribe_Values.Look<bool>(ref showBodyChange, "ChangeDresser.ShowBodyChange", true, true);
            Scribe_Values.Look<bool>(ref includeColorByLayer, "ChangeDresser.IncludeColorByLayer", true, true);
            Scribe_Values.Look<bool>(ref persistApparelOriginalColor, "ChangeDresser.PersistApparelOriginalColor", false, true);
            Scribe_Values.Look<bool>(ref persistApparelOriginalColor, "ChangeDresser.ShareHairAcrossGenders", false, false);
            Scribe_Values.Look<int>(ref mendingAttachmentMendingSpeed, "ChangeDresser.MendingAttachmentHpPerTick", DEFAULT_MENDING_SPEED, false);
            mendingAttachmentMendingSpeedBuffer = mendingAttachmentMendingSpeed.ToString();
            Scribe_Values.Look<float>(ref mendingAttachmentUpdateInterval, "ChangeDresser.MendingAttachmentUpdateInterval", DEFAULT_MENDING_UPDATE_INTERVAL, false);
            mendingAttachmentUpdateIntervalBuffer = string.Format("{0:0.0###}", mendingAttachmentUpdateInterval);
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            bool origPersistColors = persistApparelOriginalColor;

            Listing_Standard l = new Listing_Standard(GameFont.Small);
            float width = l.ColumnWidth;
            l.ColumnWidth = Math.Min(400, rect.width / 2);
            l.Begin(rect);
            l.CheckboxLabeled("ChangeDresser.IncludeColorByLayer".Translate(), ref includeColorByLayer);
            l.Gap(4);
            l.CheckboxLabeled("ChangeDresser.PersistApparelOriginalColor".Translate(), ref persistApparelOriginalColor);
            l.Gap(4);
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
            l.Gap(10);

            l.Label("ChangeDresser.MendingAttachmentSettings".Translate());
            l.Gap(4);
            NumberInput(l, "ChangeDresser.SecondsBetweenTicks",
                ref mendingAttachmentUpdateInterval, ref mendingAttachmentUpdateIntervalBuffer,
                DEFAULT_MENDING_UPDATE_INTERVAL, 0.25f, 120f);
            l.Gap(4);

            NumberInput(l, "ChangeDresser.HPPerTick",
                ref mendingAttachmentMendingSpeed, ref mendingAttachmentMendingSpeedBuffer,
                DEFAULT_MENDING_SPEED, 1, 60);

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

        private static void NumberInput(Listing_Standard l, string label, ref float val, ref string buffer, float defaultVal, float min, float max)
        {
            try
            {
                l.TextFieldNumericLabeled<float>(label.Translate(), ref val, ref buffer, min, max);
                if (l.ButtonText("ResetButton".Translate()))
                {
                    val = defaultVal;
                    buffer = string.Format("{0:0.0###}", defaultVal);
                }
            }
            catch
            {
                val = min;
                buffer = string.Format("{0:0.0###}", min);
            }
        }

        private static void NumberInput(Listing_Standard l, string label, ref int val, ref string buffer, int defaultVal, int min, int max)
        {
            try
            {
                l.TextFieldNumericLabeled<int>(label.Translate(), ref val, ref buffer, min, max);
                if (l.ButtonText("ResetButton".Translate()))
                {
                    val = defaultVal;
                    buffer = defaultVal.ToString();
                }
            }
            catch
            {
                val = min;
                buffer = min.ToString();
            }
        }
    }
}