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
            if (this.rerenderPawn)
            {
                this.dresserDto.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(this.dresserDto.Pawn);
                this.rerenderPawn = false;
            }

            Text.Font = GameFont.Medium;
            // Top Left 0 - 50
            Widgets.Label(new Rect(0f, 0f, this.InitialSize.y / 2f + 45f, 50f), "Dresser");

            float portraitBuffer = 30f;

            Rect portraitRect = WidgetUtil.AddPortraitWidget(portraitBuffer, 150f, this.dresserDto);

            float editorLeft = portraitRect.xMax + portraitBuffer;
            float editorTop = 30f + WidgetUtil.NavButtonSize.y;
            float editorWidth = 325f;

            WidgetUtil.AddSelectorWidget(portraitRect.xMax + portraitBuffer, 10f, editorWidth, this.dresserDto.EditorTypeSelectionDto);

            switch ((CurrentEditorEnum)this.dresserDto.EditorTypeSelectionDto.SelectedItem)
            {
                case CurrentEditorEnum.ApparelColor:
                    WidgetUtil.AddAppararelColorSelectionWidget(editorLeft, editorTop, editorWidth, this.dresserDto.ApparelSelectionsContainer);
                    break;
                case CurrentEditorEnum.BodyType:
                    WidgetUtil.AddSelectorWidget(editorLeft, editorTop, editorWidth, this.dresserDto.BodyTypeSelectionDto);
                    break;
                case CurrentEditorEnum.HairColor:
                    WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop, editorWidth, this.dresserDto.HairColorSelectionDto);
                    break;
                case CurrentEditorEnum.HairStyle:
                    WidgetUtil.AddSelectorWidget(editorLeft, editorTop, editorWidth, this.dresserDto.HairStyleSelectionDto);
                    break;
                case CurrentEditorEnum.HeadType:
                    WidgetUtil.AddSelectorWidget(editorLeft, editorTop, editorWidth, this.dresserDto.HeadTypeSelectionDto);
                    break;
                case CurrentEditorEnum.SkinColor:
                    WidgetUtil.AddSliderWidget(editorLeft, editorTop, editorWidth, this.dresserDto.SkinColorSliderDto);
                    break;
                case CurrentEditorEnum.Gender:
                    WidgetUtil.AddSelectorWidget(editorLeft, editorTop, editorWidth, this.dresserDto.GenderSelectionDto);
                    break;
            }

            float xWidth = 150;
            float xBuffer = (this.InitialSize.x - xWidth) / 2;
            Rect bottomButtonsRect = new Rect(editorLeft, this.InitialSize.y - WidgetUtil.NavButtonSize.y - 36, xWidth, WidgetUtil.NavButtonSize.y);
            GUI.BeginGroup(bottomButtonsRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            if (Widgets.ButtonText(new Rect(0, 0, 60, WidgetUtil.NavButtonSize.y), "Reset"))
            {
                this.ResetToDefault();
            }
            if (Widgets.ButtonText(new Rect(90, 0, 60, WidgetUtil.NavButtonSize.y), "Save"))
            {
                this.saveChangedOnExit = true;
                this.Close();
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        private void ResetToDefault()
        {
            this.dresserDto.ResetToDefault();
            this.UpdatePawn(null, null);
        }

        public override void PreClose()
        {
            base.PreClose();
            if (!this.saveChangedOnExit)
            {
                this.ResetToDefault();
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
