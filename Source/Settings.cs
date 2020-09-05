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

        public static bool ShowDresserButtonForPawns = true;
        public static bool ShowGenderAgeChange = true;
        public static bool ShowBodyChange = true;
        public static bool IncludeColorByLayer = true;
        public static bool PersistApparelOriginalColor = true;
        public static bool ShareHairAcrossGenders = false;
        public static int MendingAttachmentMendingSpeed = DEFAULT_MENDING_SPEED;
        public static string MendingAttachmentMendingSpeedBuffer = DEFAULT_MENDING_SPEED.ToString();
        public static float MendingAttachmentUpdateInterval = DEFAULT_MENDING_UPDATE_INTERVAL;
        public static string MendingAttachmentUpdateIntervalBuffer = DEFAULT_MENDING_UPDATE_INTERVAL.ToString();

        public const bool KeepForcedApparel = true;
        public const int RepairAttachmentDistance = 6;
        public static long MendingAttachmentUpdateIntervalTicks { get { return (long)(MendingAttachmentUpdateInterval * TimeSpan.TicksPerSecond); } }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref ShowDresserButtonForPawns, "ChangeDresser.ShowDresserButtonForPawns", false);
            Scribe_Values.Look<bool>(ref ShowGenderAgeChange, "ChangeDresser.ShowGenderAgeChange", true);
            Scribe_Values.Look<bool>(ref ShowBodyChange, "ChangeDresser.ShowBodyChange", true);
            Scribe_Values.Look<bool>(ref IncludeColorByLayer, "ChangeDresser.IncludeColorByLayer", true);
            Scribe_Values.Look<bool>(ref PersistApparelOriginalColor, "ChangeDresser.PersistApparelOriginalColor", false);
            Scribe_Values.Look<bool>(ref ShareHairAcrossGenders, "ChangeDresser.ShareHairAcrossGenders", false);
            Scribe_Values.Look<int>(ref MendingAttachmentMendingSpeed, "ChangeDresser.MendingAttachmentHpPerTick", DEFAULT_MENDING_SPEED);
            MendingAttachmentMendingSpeedBuffer = MendingAttachmentMendingSpeed.ToString();
            Scribe_Values.Look<float>(ref MendingAttachmentUpdateInterval, "ChangeDresser.MendingAttachmentUpdateInterval", DEFAULT_MENDING_UPDATE_INTERVAL);
            MendingAttachmentUpdateIntervalBuffer = string.Format("{0:0.0###}", MendingAttachmentUpdateInterval);
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            bool origPersistColors = PersistApparelOriginalColor;

            Listing_Standard l = new Listing_Standard(GameFont.Small);
            float width = l.ColumnWidth;
            l.ColumnWidth = Math.Min(400, rect.width / 2);
            l.Begin(rect);
            l.CheckboxLabeled("ChangeDresser.ShowDresserButtonForPawns".Translate(), ref ShowDresserButtonForPawns);
            l.Gap(4);
            l.CheckboxLabeled("ChangeDresser.IncludeColorByLayer".Translate(), ref IncludeColorByLayer);
            l.Gap(4);
            l.CheckboxLabeled("ChangeDresser.PersistApparelOriginalColor".Translate(), ref PersistApparelOriginalColor);
            l.Gap(4);
            l.CheckboxLabeled("ChangeDresser.ShareHairAcrossGenders".Translate(), ref ShareHairAcrossGenders);
            l.Gap(4);
            l.CheckboxLabeled("ChangeDresser.ShowBodyChange".Translate(), ref ShowBodyChange);
            if (ShowBodyChange)
            {
                l.Gap(4);
                l.CheckboxLabeled("ChangeDresser.ShowGenderAgeChange".Translate(), ref ShowGenderAgeChange);
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
                ref MendingAttachmentUpdateInterval, ref MendingAttachmentUpdateIntervalBuffer,
                DEFAULT_MENDING_UPDATE_INTERVAL, 0.25f, 120f);
            l.Gap(4);

            NumberInput(l, "ChangeDresser.HPPerTick",
                ref MendingAttachmentMendingSpeed, ref MendingAttachmentMendingSpeedBuffer,
                DEFAULT_MENDING_SPEED, 1, 60);

            l.End();

            if (origPersistColors != PersistApparelOriginalColor &&
                Current.Game != null && WorldComp.ApparelColorTracker != null)
            {
                if (PersistApparelOriginalColor)
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