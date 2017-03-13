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
using ChangeDresser.Util;
using System;

namespace ChangeDresser.UI
{
    internal class DresserUI : Window
    {
        private DresserDTO dresserDto;

        private bool rerenderPawn = false;

        private bool saveChangedOnExit = false;

        public DresserUI(DresserDTO dresserDto)
        {
            this.closeOnEscapeKey = true;
            this.doCloseButton = false;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.dresserDto = dresserDto;
            this.dresserDto.SetUpdatePawnListeners(this.UpdatePawn);
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(650f, 600f);
            }
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

                Widgets.Label(new Rect(0f, 0f, this.InitialSize.y / 2f + 45f, 50f), "Dresser");

                float portraitBuffer = 30f;

                Rect portraitRect = WidgetUtil.AddPortraitWidget(portraitBuffer, 150f, this.dresserDto);

                float editorLeft = portraitRect.xMax + portraitBuffer;
                float editorTop = 30f + WidgetUtil.SelectionRowHeight;
                float editorWidth = 325f;

                WidgetUtil.AddSelectorWidget(portraitRect.xMax + portraitBuffer, 10f, editorWidth, null, this.dresserDto.EditorTypeSelectionDto);

                switch ((CurrentEditorEnum)this.dresserDto.EditorTypeSelectionDto.SelectedItem)
                {
                    case CurrentEditorEnum.ApparelColor:
                        WidgetUtil.AddAppararelColorSelectionWidget(editorLeft, editorTop, editorWidth, this.dresserDto.ApparelSelectionsContainer);
                        break;
                    case CurrentEditorEnum.Body:
                        WidgetUtil.AddSelectorWidget(editorLeft, editorTop, editorWidth, "Body Type:", this.dresserDto.BodyTypeSelectionDto);
                        WidgetUtil.AddSelectorWidget(editorLeft, editorTop + WidgetUtil.SelectionRowHeight + 20f, editorWidth, "Head Type:", this.dresserDto.HeadTypeSelectionDto);
                        WidgetUtil.AddSliderWidget(editorLeft, editorTop + ((WidgetUtil.SelectionRowHeight + 20f) * 2f), editorWidth, "Skin Color:", this.dresserDto.SkinColorSliderDto);
                        GUI.Label(new Rect(editorLeft, 300f, editorWidth, 40f), "Changing Gender may have adverse effects.\nUse at own risk.", WidgetUtil.MiddleCenter);
                        WidgetUtil.AddSelectorWidget(editorLeft, 340f, editorWidth, "Gender:", this.dresserDto.GenderSelectionDto);
                        break;
                    case CurrentEditorEnum.Hair:
                        WidgetUtil.AddSelectorWidget(editorLeft, editorTop, editorWidth, "Hair Style:", this.dresserDto.HairStyleSelectionDto);
                        WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + WidgetUtil.SelectionRowHeight + 10f, editorWidth, this.dresserDto.HairColorSelectionDto, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO);
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
                if (Widgets.ButtonText(new Rect(0, 0, 60, WidgetUtil.SelectionRowHeight), "Reset"))
                {
                    this.ResetToDefault();
                }
                if (Widgets.ButtonText(new Rect(90, 0, 60, WidgetUtil.SelectionRowHeight), "Save"))
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

        private void ResetToDefault()
        {
            this.dresserDto.ResetToDefault();
            this.UpdatePawn(null, null);
        }

        public override void PreClose()
        {
            try
            {
                base.PreClose();

                if (this.dresserDto.ApparelSelectionsContainer.ColorPresetsDTO.IsModified)
                {
                    Messages.Message("Apparel Presets saved.", MessageSound.Silent);
                    IOUtil.SaveColorPresets(ColorPresetType.Apparel, this.dresserDto.ApparelSelectionsContainer.ColorPresetsDTO);
                }
                else
                    Messages.Message("Apparel Presets not modified.", MessageSound.Silent);

                if (this.dresserDto.HairColorSelectionDto.ColorPresetsDTO.IsModified)
                {
                    Messages.Message("Hair Presets saved.", MessageSound.Silent);
                    IOUtil.SaveColorPresets(ColorPresetType.Hair, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO);
                }
                else
                    Messages.Message("Hair Presets not modified.", MessageSound.Silent);

                if (!this.saveChangedOnExit)
                {
                    this.ResetToDefault();
                }
            }
            catch (Exception e)
            {
                Log.Error("Error on DresserUI.PreClose: " + e.GetType().Name + " " + e.Message);
            }
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
