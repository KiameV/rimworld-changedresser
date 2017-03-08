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
using UnityEngine;
using Verse;

namespace ChangeDresser.UI.Util
{
    delegate void SelectionChangeListener(object sender);
    delegate void UpdatePawnListener(object sender, object value);

    class WidgetUtil
    {
        private static readonly Texture2D nextTexture = ContentFinder<Texture2D>.Get("UI/next", true);
        private static readonly Texture2D previousTexture = ContentFinder<Texture2D>.Get("UI/previous", true);
        private static readonly Texture2D colorPickerTexture = ContentFinder<Texture2D>.Get("UI/colorpicker", true);
        private static readonly Texture2D editIconTexture = ContentFinder<Texture2D>.Get("UI/edit", true);

        public static readonly Vector2 NavButtonSize = new Vector2(30f, 30f);
        public static readonly Vector2 ButtonSize = new Vector2(150f, 30f);
        public static readonly Vector2 PortraitSize = new Vector2(192f, 192f);
        private static Vector2 scrollPos = new Vector2(0, 0);

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
            return rect;
        }

        public static void AddColorSelectorWidget(float left, float top, float width, SelectionColorWidgetDTO dto)
        {
            Text.Font = GameFont.Medium;

            Rect colorPickerRect = new Rect(0, 25f, width, colorPickerTexture.height * width / colorPickerTexture.width);
            GUI.BeginGroup(new Rect(left, top, width, colorPickerRect.height + 60f));

            GUI.color = Color.white;
            if (GUI.RepeatButton(colorPickerRect, colorPickerTexture, GUI.skin.label))
            {
                dto.SelectedColor = GetColorFromTexture(Event.current.mousePosition, colorPickerRect, colorPickerTexture);
            }

            GUI.BeginGroup(new Rect(0, colorPickerRect.height + 30f, width, 20f));
            GUIStyle centeredStyle = GUI.skin.label;
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0f, 0f, 10f, 20f), "R", centeredStyle);
            string rText = GUI.TextField(new Rect(12f, 0f, 30f, 20f), ColorConvert(dto.SelectedColor.r), 3);

            GUI.Label(new Rect(52f, 0f, 10f, 20f), "G", centeredStyle);
            string gText = GUI.TextField(new Rect(64f, 0f, 30f, 20f), ColorConvert(dto.SelectedColor.g), 3);

            GUI.Label(new Rect(104f, 0f, 10f, 20f), "B", centeredStyle);
            string bText = GUI.TextField(new Rect(116f, 0f, 30f, 20f), ColorConvert(dto.SelectedColor.b), 3);
            GUI.EndGroup();
            GUI.EndGroup();

            Color c = dto.SelectedColor;
            c.r = ColorConvert(rText);
            c.g = ColorConvert(gText);
            c.b = ColorConvert(bText);
            dto.SelectedColor = c;

            Text.Anchor = TextAnchor.UpperLeft;
        }
        
        public static void AddSliderWidget(float left, float top, float width, SliderWidgetDTO sliderWidgetDto)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;

            Rect rect = new Rect(left, top + 5f, width, NavButtonSize.y);
            GUI.BeginGroup(rect);

            GUI.color = Color.white;
            sliderWidgetDto.SelectedValue = GUI.HorizontalSlider(
                new Rect(20, 10f, width - 20, NavButtonSize.y), 
                sliderWidgetDto.SelectedValue, sliderWidgetDto.MinValue, sliderWidgetDto.MaxValue);

            GUI.EndGroup();

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void AddSelectorWidget(float left, float top, float width, ASelectionWidgetDTO selectionWidgetDto)
        {
            const float buffer = 5f;
            Text.Anchor = TextAnchor.MiddleCenter;

            Rect rect = new Rect(left + 50, top, width - 50, NavButtonSize.y);
            GUI.BeginGroup(rect);
            {
                GUI.color = Color.white;
                Text.Font = GameFont.Medium;
                Rect previousButtonRect = new Rect(0, 0, NavButtonSize.x, NavButtonSize.y);
                if (GUI.Button(previousButtonRect, previousTexture))
                {
                    selectionWidgetDto.DecreaseIndex();
                }

                Rect labelRect = new Rect(NavButtonSize.x + buffer, 0, rect.width - (2 * NavButtonSize.x) - (2 * buffer), NavButtonSize.y);
                var centeredStyle = GUI.skin.label;
                centeredStyle.alignment = TextAnchor.MiddleCenter;
                GUI.Label(labelRect, selectionWidgetDto.SelectedItemLabel, centeredStyle);

                GUI.color = Color.grey;
                Widgets.DrawBox(labelRect, 1);

                Rect nextButtonRect = new Rect(rect.width - NavButtonSize.x, 0, NavButtonSize.x, NavButtonSize.y);
                if (GUI.Button(nextButtonRect, nextTexture))
                {
                    selectionWidgetDto.IncreaseIndex();
                }
            }
            GUI.EndGroup();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void AddAppararelColorSelectionWidget(float left, float top, float width, ApparelSelectionsContainer apparelSelectionsContainer)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            if (apparelSelectionsContainer.Count == 0)
            {
                var centeredStyle = GUI.skin.label;
                centeredStyle.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(left, top, width, NavButtonSize.y), "No clothing is being worn.", centeredStyle);
            }
            else
            {
                const float cellHeight = 40f;
                Rect apparelListRect = new Rect(left, top, width, 150f);
                Rect apparealScrollRect = new Rect(0f, 0f, width - 16f, apparelSelectionsContainer.Count * cellHeight + NavButtonSize.y);

                GUI.BeginGroup(apparelListRect);
                scrollPos = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), scrollPos, apparealScrollRect);

                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(new Rect(40, 0, width - 56, NavButtonSize.y), "Dye All"))
                {
                    apparelSelectionsContainer.SelectedApparel = apparelSelectionsContainer.DyeAllSelectionDto;
                }
                Text.Font = GameFont.Medium;

                for (int i = 0; i < apparelSelectionsContainer.Count; ++i)
                {
                    Apparel apparel = apparelSelectionsContainer[i].Apparel;
                    GUI.BeginGroup(new Rect(0, NavButtonSize.y + 5f + i * cellHeight, apparelListRect.width, cellHeight));
                    Widgets.ThingIcon(new Rect(0f, 0f, 40f, cellHeight), apparel);
                    Text.Font = GameFont.Small;
                    Widgets.Label(new Rect(45f, 0f, apparealScrollRect.width - 90f, cellHeight), apparel.Label);
                    if (Widgets.ButtonImage(new Rect(apparealScrollRect.width - 40f, 0 + 4f, 32f, 32f), editIconTexture))
                    {
                        apparelSelectionsContainer.SelectedApparel = apparelSelectionsContainer[i];
                    }
                    GUI.EndGroup();
                }
                GUI.EndScrollView();
                GUI.EndGroup();

                if (apparelSelectionsContainer.SelectedApparel != null)
                {
                    AddColorSelectorWidget(left, top + apparelListRect.height + 10f, width, apparelSelectionsContainer.SelectedApparel);
                }
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