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
using ChangeDresser.UI.DTO;
using ChangeDresser.UI.Util;
using RimWorld;
using UnityEngine;
using Verse;
using ChangeDresser.UI.Enums;
using ChangeDresser.UI.DTO.SelectionWidgetDTOs;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace ChangeDresser.UI
{
    internal class DresserUI : Window
    {
        private static readonly long TICKS_PER_YEAR = 3600000L;
        private static readonly long MAX_AGE = 1000000000 * TICKS_PER_YEAR;
        private DresserDTO dresserDto;

        private bool rerenderPawn = false;

        private bool saveChangedOnExit = false;

        private bool originalHatsHideSetting;

        private List<Apparel> ApparelWithColorChange = new List<Apparel>();

        public DresserUI(DresserDTO dresserDto)
        {
            this.closeOnEscapeKey = true;
            this.doCloseButton = false;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.dresserDto = dresserDto;
            this.dresserDto.SetUpdatePawnListeners(this.UpdatePawn);
            this.dresserDto.EditorTypeSelectionDto.SelectionChangeListener += delegate (object sender)
            {
                this.rerenderPawn = true;
            };
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(650f, 600f);
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            this.originalHatsHideSetting = Prefs.HatsOnlyOnMap;
        }

        public override void PostClose()
        {
            base.PostClose();
            Prefs.HatsOnlyOnMap = this.originalHatsHideSetting;
            this.dresserDto.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            PortraitsCache.SetDirty(this.dresserDto.Pawn);
        }

        public override void DoWindowContents(Rect inRect)
        {
            try
            {
                if (this.rerenderPawn)
                {
                    this.dresserDto.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    PortraitsCache.SetDirty(this.dresserDto.Pawn);
                    this.rerenderPawn = false;
                }

                Text.Font = GameFont.Medium;

                Widgets.Label(new Rect(0f, 0f, this.InitialSize.y / 2f + 45f, 50f), "ChangeDresser.DresserLabel".Translate());

                float portraitBuffer = 30f;

                Rect portraitRect = WidgetUtil.AddPortraitWidget(portraitBuffer, 150f, this.dresserDto);

                float editorLeft = portraitRect.xMax + portraitBuffer;
                float editorTop = 30f + WidgetUtil.SelectionRowHeight;
                float editorWidth = 325f;

                WidgetUtil.AddSelectorWidget(portraitRect.xMax + portraitBuffer, 10f, editorWidth, null, this.dresserDto.EditorTypeSelectionDto);

                switch ((CurrentEditorEnum)this.dresserDto.EditorTypeSelectionDto.SelectedItem)
                {
                    case CurrentEditorEnum.ChangeDresserApparelColor:
                        WidgetUtil.AddAppararelColorSelectionWidget(editorLeft, editorTop, editorWidth, this.dresserDto.ApparelSelectionsContainer);
                        break;
                    case CurrentEditorEnum.ChangeDresserBody:
                        float top = editorTop;
                        WidgetUtil.AddSelectorWidget(editorLeft, top, editorWidth, "ChangeDresser.BodyType".Translate() + ":", this.dresserDto.BodyTypeSelectionDto);
                        top += WidgetUtil.SelectionRowHeight + 20f;
                        WidgetUtil.AddSelectorWidget(editorLeft, top, editorWidth, "ChangeDresser.HeadType".Translate() + ":", this.dresserDto.HeadTypeSelectionDto);
                        top += WidgetUtil.SelectionRowHeight + 20f;
                        WidgetUtil.AddSliderWidget(editorLeft, top, editorWidth, "ChangeDresser.SkinColor".Translate() + ":", this.dresserDto.SkinColorSliderDto);
                        if (Settings.ShowGenderAgeChange)
                        {
                            GUI.Label(new Rect(editorLeft, 300f, editorWidth, 40f), "ChangeDresser.GenderChangeWarning".Translate(), WidgetUtil.MiddleCenter);
                            top = 340f;
                            WidgetUtil.AddSelectorWidget(editorLeft, top, editorWidth, "ChangeDresser.Gender".Translate() + ":", this.dresserDto.GenderSelectionDto);

                            top += WidgetUtil.SelectionRowHeight + 5;
                            long ageBio = this.dresserDto.Pawn.ageTracker.AgeBiologicalTicks;
                            if (AddLongInput(editorLeft, top, 120, 80, "ChangeDresser.AgeBiological".Translate() + ":", ref ageBio, MAX_AGE, TICKS_PER_YEAR))
                            {
                                this.dresserDto.Pawn.ageTracker.AgeBiologicalTicks = ageBio;
                                rerenderPawn = true;
                                if (ageBio > this.dresserDto.Pawn.ageTracker.AgeChronologicalTicks)
                                {
                                    this.dresserDto.Pawn.ageTracker.AgeChronologicalTicks = ageBio;
                                }
                            }

                            top += WidgetUtil.SelectionRowHeight + 5;
                            long ageChron = this.dresserDto.Pawn.ageTracker.AgeChronologicalTicks;
                            if (AddLongInput(editorLeft, top, 120, 80, "ChangeDresser.AgeChronological".Translate() + ":", ref ageChron, MAX_AGE, TICKS_PER_YEAR))
                            {
                                this.dresserDto.Pawn.ageTracker.AgeChronologicalTicks = ageChron;
                            }
                        }
                        break;
                    case CurrentEditorEnum.ChangeDresserHair:
                        /*if (Settings.UseLeftRightHairSelect)
                        {
                            WidgetUtil.AddSelectorWidget(editorLeft, editorTop, editorWidth, "ChangeDresser.HairStyle".Translate() + ":", this.dresserDto.HairStyleSelectionDto);
                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + WidgetUtil.SelectionRowHeight + 10f, editorWidth, this.dresserDto.HairColorSelectionDto, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO);
                        }
                        else
                        {*/
                        const float listboxHeight = 250f;
                        WidgetUtil.AddListBoxWidget(editorLeft, editorTop, editorWidth, listboxHeight, "ChangeDresser.HairStyle".Translate() + ":", this.dresserDto.HairStyleSelectionDto);
                        WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + listboxHeight + 10f, editorWidth, this.dresserDto.HairColorSelectionDto, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO);
                        //}
                        break;
                }

                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                GUI.Label(new Rect(0, 75, this.InitialSize.y / 2f, 50f), GUI.tooltip);
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.UpperLeft;

                float xWidth = 150;
                float xBuffer = (this.InitialSize.x - xWidth) / 2;
                Rect bottomButtonsRect = new Rect(editorLeft, this.InitialSize.y - WidgetUtil.SelectionRowHeight - 36, xWidth, WidgetUtil.SelectionRowHeight);
                GUI.BeginGroup(bottomButtonsRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                if (Widgets.ButtonText(new Rect(0, 0, 60, WidgetUtil.SelectionRowHeight), "ChangeDresser.Reset".Translate()))
                {
                    this.ResetToDefault();
                    this.ApparelWithColorChange.Clear();
                }
                if (Widgets.ButtonText(new Rect(90, 0, 60, WidgetUtil.SelectionRowHeight), "ChangeDresser.Save".Translate()))
                {
                    this.saveChangedOnExit = true;
                    this.Close();
                }
                GUI.EndGroup();
            }
            catch (Exception e)
            {
                Log.Error(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message);
                Messages.Message(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message, MessageSound.Negative);
                base.Close();
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        private bool AddLongInput(float labelLeft, float top, float inputLeft, float inputWidth, string label, ref long value, long maxValue, long factor = 1)
        {
            string stringValue;
            if (value == -1)
            {
                stringValue = "";
            }
            else
            {
                stringValue = (value / factor).ToString();
            }
            string result = WidgetUtil.AddNumberTextInput(labelLeft, top, inputLeft, inputWidth, label, stringValue);
            try
            {
                if (result.Length == 0)
                {
                    value = -1;
                    return true;
                }
                else if (result.Length > 0 && !result.Equals(stringValue))
                {
                    value = long.Parse(result);
                    if (value < 0)
                    {
                        value = 0;
                    }
                    else
                    {
                        value *= factor;
                        if (value > maxValue || value < 0)
                            value = maxValue;
                    }
                    return true;
                }
            }
            catch { }
            return false;
        }

        private void ResetToDefault()
        {
            this.dresserDto.ResetToDefault();
            this.UpdatePawn(null, null);
        }

        public override void PreClose()
        {
#if DEBUG
            Log.Message(Environment.NewLine + "Start DresserUI.PreClose");
#endif
            try
            {
                base.PreClose();

                if (this.dresserDto?.ApparelSelectionsContainer?.ColorPresetsDTO?.IsModified == true)
                {
                    IOUtil.SaveColorPresets(ColorPresetType.Apparel, this.dresserDto.ApparelSelectionsContainer.ColorPresetsDTO);
                }

                if (this.dresserDto?.HairColorSelectionDto?.ColorPresetsDTO?.IsModified == true)
                {
                    IOUtil.SaveColorPresets(ColorPresetType.Hair, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO);
                }

                if (this.ApparelWithColorChange != null)
                {
#if DEBUG
                    Log.Warning(" this.ApparelWithColorChange.Count: " + this.ApparelWithColorChange.Count);
#endif
                    foreach(Apparel a in this.ApparelWithColorChange)
                    {
                        PawnOutfits po;
                        if (WorldComp.PawnOutfits.TryGetValue(this.dresserDto.Pawn, out po))
                        {
#if DEBUG
                            Log.Warning("  po found ");
#endif
                            foreach (ApparelLayer l in a.def.apparel.layers)
                            {
#if DEBUG
                                Log.Warning("  color change for " + a.Label);
#endif
                                po.SetColorFor(l, a.DrawColor);
                            }
                        }
                    }

                    this.ApparelWithColorChange.Clear();
                    this.ApparelWithColorChange = null;
                }

                if (!this.saveChangedOnExit)
                {
                    this.ResetToDefault();
                }
            }
            catch (Exception e)
            {
                Log.Error("Error on DresserUI.PreClose: " + e.GetType().Name + " " + e.Message);
            }
#if DEBUG
            Log.Message("End DresserUI.PreClose" + Environment.NewLine);
#endif
        }

        private void UpdatePawn(object sender, object value)
        {
            if (sender != null)
            {
                Pawn pawn = this.dresserDto.Pawn;

                if (sender is ApparelColorSelectionDTO)
                {
                    ApparelColorSelectionDTO dto = (ApparelColorSelectionDTO)sender;
                    CompColorableUtility.SetColor(dto.Apparel, dto.SelectedColor, true);

                    if (!this.ApparelWithColorChange.Contains(dto.Apparel))
                    {
                        this.ApparelWithColorChange.Add(dto.Apparel);
                    }
                }
                if (sender is BodyTypeSelectionDTO)
                {
                    pawn.story.bodyType = (BodyType)value;
                }
                else if (sender is GenderSelectionDTO)
                {
                    pawn.gender = (Gender)value;
                }
                else if (sender is HairColorSelectionDTO)
                {
                    pawn.story.hairColor = (Color)value;
                }
                else if (sender is HairStyleSelectionDTO)
                {
                    pawn.story.hairDef = (HairDef)value;
                }
                else if (sender is HeadTypeSelectionDTO)
                {
                    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dresserDto.Pawn.story, value);
                }
                else if (sender is SliderWidgetDTO)
                {
                    pawn.story.melanin = (float)value;
                }
            }
            rerenderPawn = true;
        }
    }
}
