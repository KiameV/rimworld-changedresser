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
using ChangeDresser.UI.DTO.SelectionWidgetDTOs;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System;
using System.Text.RegularExpressions;

namespace ChangeDresser.UI.Util
{
    delegate void SelectionChangeListener(object sender);
    delegate void UpdatePawnListener(object sender, object value);

    [StaticConstructorOnStartup]
    static class WidgetUtil
    {
        public static readonly Texture2D nextTexture;
        public static readonly Texture2D previousTexture;
        public static readonly Texture2D cantTexture;
        public static readonly Texture2D colorPickerTexture;
        public static readonly Texture2D copyIconTexture;
        public static readonly Texture2D pasteIconTexture;
        public static readonly Texture2D dropTexture;

        static WidgetUtil()
        {
            nextTexture = ContentFinder<Texture2D>.Get("UI/next", true);
            previousTexture = ContentFinder<Texture2D>.Get("UI/previous", true);
            cantTexture = ContentFinder<Texture2D>.Get("UI/x", true);
            colorPickerTexture = ContentFinder<Texture2D>.Get("UI/colorpicker", true);
            copyIconTexture = ContentFinder<Texture2D>.Get("UI/copy", true);
            pasteIconTexture = ContentFinder<Texture2D>.Get("UI/paste", true);
            dropTexture = ContentFinder<Texture2D>.Get("UI/drop", true);
        }

        public static readonly Vector2 NavButtonSize = new Vector2(30f, 30f);
        public static readonly Vector2 ButtonSize = new Vector2(150f, 30f);
        public static readonly Vector2 PortraitSize = new Vector2(192f, 192f);
        private static Vector2 scrollPos = new Vector2(0, 0);
        private static readonly Texture2D ColorPreset = new Texture2D(20, 20);

        public static float SelectionRowHeight { get { return NavButtonSize.y; } }

        private static GUIStyle middleCenterGuiStyle = null;

        public static GUIStyle MiddleCenter
        {
            get
            {
                if (middleCenterGuiStyle == null)
                {
                    middleCenterGuiStyle = GUI.skin.label;
                    middleCenterGuiStyle.alignment = TextAnchor.MiddleCenter;
                }
                return middleCenterGuiStyle;
            }
        }

        public static Rect AddPortraitWidget(float left, float top, DresserDTO dresserDto)
        {
            // Portrait
            Rect rect = new Rect(left, top, PortraitSize.x, PortraitSize.y);

            // Draw the pawn's portrait
            GUI.BeginGroup(rect);
            Vector2 size = new Vector2(128f, 180f);
            Rect position = new Rect(rect.width * 0.5f - size.x * 0.5f, 10f + rect.height * 0.5f - size.y * 0.5f, size.x, size.y);
            RenderTexture image = PortraitsCache.Get(dresserDto.Pawn, size, new Vector3(0f, 0f, 0f), 1f);
            GUI.DrawTexture(position, image);
            GUI.EndGroup();

            GUI.color = Color.white;
            Widgets.DrawBox(rect, 1);

            for (int x = 0; x < ColorPreset.width; ++x)
                for (int y = 0; y < ColorPreset.height; ++y)
                    ColorPreset.SetPixel(x, y, Color.white);

            return rect;
        }

        public static void AddColorSelectorWidget(float left, float top, float width, SelectionColorWidgetDTO selectionDto, ColorPresetsDTO presetsDto)
        {
            List<SelectionColorWidgetDTO> l = new List<SelectionColorWidgetDTO>(1);
            l.Add(selectionDto);
            AddColorSelectorWidget(left, top, width, l, presetsDto);
        }

        public static void AddColorSelectorWidget(float left, float top, float width, List<SelectionColorWidgetDTO> selectionDtos, ColorPresetsDTO presetsDto)
        {
            Text.Font = GameFont.Medium;

            Rect colorPickerRect = new Rect(0, 25f, width, colorPickerTexture.height * width / colorPickerTexture.width);
            GUI.BeginGroup(new Rect(left, top, width, colorPickerRect.height + 60f));

            GUI.color = Color.white;
            if (GUI.RepeatButton(colorPickerRect, colorPickerTexture, GUI.skin.label))
            {
                SetColorToSelected(selectionDtos, presetsDto, GetColorFromTexture(Event.current.mousePosition, colorPickerRect, colorPickerTexture));
            }

            GUI.BeginGroup(new Rect(0, colorPickerRect.height + 30f, width, 20f));
            Color rgbColor = Color.white;
            if (presetsDto.HasSelected())
            {
                rgbColor = presetsDto.GetSelectedColor();
            }
            else if (selectionDtos.Count > 0)
            {
                rgbColor = selectionDtos[0].SelectedColor;
            }
            GUI.Label(new Rect(0f, 0f, 10f, 20f), "ChangeDresser.R".Translate(), MiddleCenter);
            string rText = GUI.TextField(new Rect(12f, 0f, 30f, 20f), ColorConvert(rgbColor.r), 3);

            GUI.Label(new Rect(52f, 0f, 10f, 20f), "ChangeDresser.G".Translate(), MiddleCenter);
            string gText = GUI.TextField(new Rect(64f, 0f, 30f, 20f), ColorConvert(rgbColor.g), 3);

            GUI.Label(new Rect(104f, 0f, 10f, 20f), "ChangeDresser.B".Translate(), MiddleCenter);
            string bText = GUI.TextField(new Rect(116f, 0f, 30f, 20f), ColorConvert(rgbColor.b), 3);

            bool skipRGB = false;
            float l = 156f;
            for (int i = 0; i < presetsDto.Count; ++i)
            {
                GUI.color = presetsDto[i];

                l += ColorPreset.width + 4;
                Rect presetRect = new Rect(l, 0f, ColorPreset.width, ColorPreset.height);
                GUI.Label(presetRect, new GUIContent(ColorPreset,
                    "ChangeDresser.ColorPresetHelp".Translate()));
                if (Widgets.ButtonInvisible(presetRect, false))
                {
                    if (Event.current.shift)
                    {
                        if (presetsDto.IsSelected(i))
                        {
                            presetsDto.Deselect();
                        }
                        else
                        {
                            if (selectionDtos.Count > 0 &&
                                !presetsDto.HasSelected())
                            {
                                presetsDto.SetColor(i, selectionDtos[0].SelectedColor);
                            }
                            presetsDto.SetSelected(i);
                        }
                    }
                    else
                    {
                        SetColorToSelected(selectionDtos, null, presetsDto[i]);
                    }
                    skipRGB = true;
                }
                GUI.color = Color.white;
                if (presetsDto.IsSelected(i))
                {
                    Widgets.DrawBox(presetRect, 1);
                }
            }
            GUI.EndGroup();
            GUI.EndGroup();

            if (!skipRGB &&
                (selectionDtos.Count > 0 || presetsDto.HasSelected()))
            {
                Color c = Color.white;
                c.r = ColorConvert(rText);
                c.g = ColorConvert(gText);
                c.b = ColorConvert(bText);

                SetColorToSelected(selectionDtos, presetsDto, c);
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void SetColorToSelected(List<SelectionColorWidgetDTO> dtos, ColorPresetsDTO presetsDto, Color color)
        {
            if (presetsDto != null && presetsDto.HasSelected())
            {
                presetsDto.SetSelectedColor(color);
            }
            else if (dtos.Count > 0)
            {
                foreach (SelectionColorWidgetDTO dto in dtos)
                {
                    dto.SelectedColor = color;
                }
            }
        }

        public static string AddNumberTextInput(float labelLeft, float top, float inputLeft, float inputWidth, string label, string value)
        {
            GUI.color = Color.white;
            GUI.BeginGroup(new Rect(labelLeft, top, inputLeft + inputWidth, SelectionRowHeight));
            GUI.Label(new Rect(0, 0, inputLeft - 5, SelectionRowHeight), label, MiddleCenter);
            Rect inputRect = new Rect(inputLeft, 0, inputWidth, SelectionRowHeight);
            string result = GUI.TextField(inputRect, value, MiddleCenter).Trim();
            GUI.color = Color.grey;
            Widgets.DrawBox(inputRect, 1);
            GUI.EndGroup();

            if (result.Length > 0 && !Regex.IsMatch(result, "^[0-9]*$"))
            {
                result = value;
            }
            return result;
        }

        public static void AddSliderWidget(float left, float top, float width, string label, SliderWidgetDTO sliderWidgetDto)
        {
            Rect rect = new Rect(left, top + 5f, width, SelectionRowHeight);
            GUI.BeginGroup(rect);

            GUI.color = Color.white;
            GUI.Label(new Rect(0, 0, 75, SelectionRowHeight), label, MiddleCenter);
            sliderWidgetDto.SelectedValue = GUI.HorizontalSlider(
                new Rect(80, 10f, width - 100, SelectionRowHeight), 
                sliderWidgetDto.SelectedValue, sliderWidgetDto.MinValue, sliderWidgetDto.MaxValue);

            GUI.EndGroup();
        }

        public static void AddSelectorWidget(float left, float top, float width, string label, ASelectionWidgetDTO selectionWidgetDto)
        {
            const float buffer = 5f;

            Rect rect = new Rect(left, top, width, SelectionRowHeight);
            GUI.BeginGroup(rect);
            GUI.color = Color.white;
            Text.Font = GameFont.Medium;
            left = 0;
            if (label != null)
            {
                //Text.Anchor = TextAnchor.MiddleLeft;
                GUI.Label(new Rect(0, 0, 75, SelectionRowHeight), label, MiddleCenter);
                left = 80;
            }
            Text.Anchor = TextAnchor.MiddleCenter;

            Rect previousButtonRect = new Rect(left, 0, NavButtonSize.x, NavButtonSize.y);
            if (GUI.Button(previousButtonRect, previousTexture))
            {
                selectionWidgetDto.DecreaseIndex();
            }

            Rect labelRect = new Rect(NavButtonSize.x + buffer + left, 0, rect.width - (2 * NavButtonSize.x) - (2 * buffer) - left, NavButtonSize.y);
            GUI.Label(labelRect, selectionWidgetDto.SelectedItemLabel, MiddleCenter);

            GUI.color = Color.grey;
            Widgets.DrawBox(labelRect, 1);

            Rect nextButtonRect = new Rect(rect.width - NavButtonSize.x, 0, NavButtonSize.x, NavButtonSize.y);
            if (GUI.Button(nextButtonRect, nextTexture))
            {
                selectionWidgetDto.IncreaseIndex();
            }
            GUI.EndGroup();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void AddAppararelColorSelectionWidget(float left, float top, float width, ApparelSelectionsContainer apparelSelectionsContainer)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            if (apparelSelectionsContainer.Count == 0)
            {
                GUI.Label(new Rect(left, top, width, SelectionRowHeight), "ChangeDresser.NoClothingIsWorn".Translate(), MiddleCenter);
            }
            else
            {
                const float cellHeight = 40f;
                Rect apparelListRect = new Rect(left, top, width, 150f);
                Rect apparelScrollRect = new Rect(0f, 0f, width - 16f, apparelSelectionsContainer.Count * cellHeight + SelectionRowHeight);

                GUI.BeginGroup(apparelListRect);
                scrollPos = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), scrollPos, apparelScrollRect);

                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(new Rect(20, 0, 100, SelectionRowHeight), "ChangeDresser.SelectAll".Translate()))
                {
                    apparelSelectionsContainer.SelectAll();
                }
                if (Widgets.ButtonText(new Rect(apparelScrollRect.width - 120, 0, 100, SelectionRowHeight), "ChangeDresser.DeselectAll".Translate()))
                {
                    apparelSelectionsContainer.DeselectAll();
                }
                Text.Font = GameFont.Medium;

                for (int i = 0; i < apparelSelectionsContainer.Count; ++i)
                {
                    ApparelColorSelectionDTO dto = apparelSelectionsContainer[i];
                    Apparel apparel = dto.Apparel;
                    GUI.BeginGroup(new Rect(0, SelectionRowHeight + 5f + i * cellHeight, apparelListRect.width, cellHeight));

                    Widgets.ThingIcon(new Rect(0f, 0f, 40f, cellHeight), apparel);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Rect textRect = new Rect(45f, 0f, apparelScrollRect.width - 90f, cellHeight);
                    if (apparelSelectionsContainer.IsSelected(dto))
                    {
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.gray;
                    }
                    Widgets.Label(textRect, new GUIContent(apparel.Label, "ChangeDresser.SelectMultipleApparel".Translate()));
                    if (Widgets.ButtonInvisible(textRect, false))
                    {
                        apparelSelectionsContainer.Select(dto, Event.current.shift);
                    }
                    GUI.color = Color.white;
                    if (Widgets.ButtonImage(new Rect(apparelScrollRect.width - 40f, 0, 32f, 16f), copyIconTexture))
                    {
                        apparelSelectionsContainer.CopyColor = apparel.DrawColor;
                    }
                    if (apparelSelectionsContainer.CopyColorSelected)
                    {
                        if (Widgets.ButtonImage(new Rect(apparelScrollRect.width - 40f, 16f, 32f, 16f), pasteIconTexture))
                        {
                            dto.SelectedColor = apparelSelectionsContainer.CopyColor;
                        }
                    }
                    GUI.EndGroup();
                }
                GUI.EndScrollView();
                GUI.EndGroup();
                
                AddColorSelectorWidget(left, top + apparelListRect.height + 10f, width, apparelSelectionsContainer.SelectedApparel, apparelSelectionsContainer.ColorPresetsDTO);
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static Color GetColorFromTexture(Vector2 mousePosition, Rect rect, Texture2D texture)
        {
            float localMouseX = mousePosition.x - rect.x;
            float localMouseY = mousePosition.y - rect.y;
            int imageX = (int)(localMouseX * ((float)colorPickerTexture.width / (rect.width + 0f)));
            int imageY = (int)((rect.height - localMouseY) * ((float)colorPickerTexture.height / (rect.height + 0f)));
            Color pixel = texture.GetPixel(imageX, imageY);
            return pixel;
        }

        private static string ColorConvert(float f)
        {
            try
            {
                int i = (int)(f * 255);
                if (i > 255)
                {
                    i = 255;
                }
                else if (i < 0)
                {
                    i = 0;
                }
                return i.ToString();
            }
            catch
            {
                return "0";
            }
        }

        private static float ColorConvert(string intText)
        {
            try
            {
                float f = int.Parse(intText) / 255f;
                if (f > 1)
                {
                    f = 1;
                }
                else if (f < 0)
                {
                    f = 0;
                }
                return f;
            }
            catch
            {
                return 0;
            }

        }
    }
}