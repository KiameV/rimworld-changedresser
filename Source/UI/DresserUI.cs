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
            this.closeOnClickedOutside = true;
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
                if (this.dresserDto.GradientHairColorSelectionDto != null)
                    return new Vector2(750f, 600f);
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
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(this.dresserDto.Pawn);
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

                float x = 30f;
                float y = 150f;
                if (this.dresserDto.GradientHairColorSelectionDto != null)
                {
                    x += 100f;
                    y = 90f;
                }
                //Rect portraitRect = WidgetUtil.AddPortraitWidget(x, 150f, this.dresserDto);
                Rect portraitRect = WidgetUtil.AddPortraitWidget(x, y, this.dresserDto);
                y += portraitRect.height;

                float editorLeft = portraitRect.xMax + 30f;
                float editorTop = 30f + WidgetUtil.SelectionRowHeight;
                float editorWidth = 325f;

                WidgetUtil.AddSelectorWidget(portraitRect.xMax + 30, 10f, editorWidth, null, this.dresserDto.EditorTypeSelectionDto);

                switch ((CurrentEditorEnum)this.dresserDto.EditorTypeSelectionDto.SelectedItem)
                {
                    case CurrentEditorEnum.ChangeDresserApparelColor:
                        WidgetUtil.AddAppararelColorSelectionWidget(editorLeft, editorTop, editorWidth, this.dresserDto.ApparelSelectionsContainer, this.GetClearColorCallback(), this.dresserDto.Pawn.story.favoriteColor);
                        break;
                    case CurrentEditorEnum.ChangeDresserApparelLayerColor:
                        WidgetUtil.AddAppararelColorByLayerSelectionWidget(editorLeft, editorTop, editorWidth, this.dresserDto.ApparelLayerSelectionsContainer, this.GetClearColorCallback(), this.dresserDto.Pawn.story.favoriteColor);
                        break;
                    case CurrentEditorEnum.ChangeDresserBody:
                        bool isShowing = false;
                        float top = editorTop;
                        if (this.dresserDto.BodyTypeSelectionDto != null && this.dresserDto.BodyTypeSelectionDto.Count > 1)
                        {
                            WidgetUtil.AddSelectorWidget(editorLeft, top, editorWidth, "ChangeDresser.BodyType".Translate() + ":", this.dresserDto.BodyTypeSelectionDto);
                            top += WidgetUtil.SelectionRowHeight + 20f;
                            isShowing = true;
                        }
                        if (this.dresserDto.HeadTypeSelectionDto != null && this.dresserDto.HeadTypeSelectionDto.Count > 1)
                        {
                            WidgetUtil.AddSelectorWidget(editorLeft, top, editorWidth, "ChangeDresser.HeadType".Translate() + ":", this.dresserDto.HeadTypeSelectionDto);
                            top += WidgetUtil.SelectionRowHeight + 20f;
                            isShowing = true;
                        }
                        if (this.dresserDto.SkinColorSliderDto != null)
                        {
                            WidgetUtil.AddSliderWidget(editorLeft, top, editorWidth, "ChangeDresser.SkinColor".Translate() + ":", this.dresserDto.SkinColorSliderDto);
                            isShowing = true;
                        }

                        if (!isShowing)
                        {
                            GUI.Label(new Rect(editorLeft, top, editorWidth, 40), "ChangeDresser.NoEditableAttributes".Translate());
                        }

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
                        const float listboxHeight = 250f;
                        if (this.dresserDto.HairStyleSelectionDto != null)
                        {
                            //bool showHairColor = this.dresserDto.HairColorSelectionDto != null;

                            float height = listboxHeight;
                            /*if (!showHairColor)
                            {
                                height += 250;
                            }*/

                            WidgetUtil.AddListBoxWidget(editorLeft, editorTop, editorWidth, height, 
                                ref this.dresserDto.SelectedStyle,
                                "ChangeDresser.HairStyle".Translate(), this.dresserDto.HairStyleSelectionDto,
                                "ChangeDresser.BeardStyle".Translate(), this.dresserDto.BeardStyleSelectionDto);

                            //if (showHairColor)
                            //{
                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + listboxHeight + 10f, editorWidth, this.dresserDto.HairColorSelectionDto, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO, this.dresserDto.Pawn.story.favoriteColor);
                            //}

                            if (this.dresserDto.GradientHairColorSelectionDto != null)
                            {
                                Text.Font = GameFont.Tiny;
                                Widgets.CheckboxLabeled(new Rect(15f, y + 5f, 120f, 24f), "GradientHairTitle".Translate(), ref this.dresserDto.GradientHairColorSelectionDto.IsGradientEnabled);
                                if (this.dresserDto.GradientHairColorSelectionDto.IsGradientEnabled)
                                {
                                    WidgetUtil.AddColorSelectorWidget(15f, editorTop + listboxHeight + 10f, editorWidth, this.dresserDto.GradientHairColorSelectionDto, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO, this.dresserDto.Pawn.story.favoriteColor);
                                }
                            }
                        }
                        break;
                    case CurrentEditorEnum.ChangeDresserFavoriteColor:
                        if (this.dresserDto.FavoriteColorDTO != null)
                        {
                            Text.Font = GameFont.Tiny;
                            WidgetUtil.AddFavoriteColorSelectorWidget(editorLeft, editorTop, editorWidth, this.dresserDto.FavoriteColorDTO, this.dresserDto.FavoriteColorDTO.ColorPresetsDTO);
                        }
                        break;
                    case CurrentEditorEnum.ChangeDresserAlienSkinColor:
                        if (this.dresserDto.AlienSkinColorPrimary != null)
                        {
                            GUI.color = Color.white;
                            Text.Font = GameFont.Medium;
                            GUI.Label(new Rect(editorLeft, editorTop, editorWidth, 30), "ChangeDresser.AlienPrimarySkinColor".Translate());
                            Text.Font = GameFont.Small;

                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + 40, editorWidth, this.dresserDto.AlienSkinColorPrimary, null, this.dresserDto.Pawn.story.favoriteColor);
                        }
                        if (this.dresserDto.AlienSkinColorSecondary != null)
                        {
                            GUI.color = Color.white;
                            Text.Font = GameFont.Medium;
                            GUI.Label(new Rect(editorLeft, editorTop + 260, editorWidth, 30), "ChangeDresser.AlienSecondarySkinColor".Translate());
                            Text.Font = GameFont.Small;

                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + 300, editorWidth, this.dresserDto.AlienSkinColorSecondary, null, this.dresserDto.Pawn.story.favoriteColor);
                        }
                        break;

                    /*case CurrentEditorEnum.ChangeDresserAlienHairColor:
                        if (this.dresserDto.AlienHairColorPrimary != null)
                        {
                            GUI.color = Color.white;
                            Text.Font = GameFont.Medium;
                            GUI.Label(new Rect(editorLeft, editorTop, editorWidth, 30), "ChangeDresser.AlienPrimaryHairColor".Translate());
                            Text.Font = GameFont.Small;

                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + 40, editorWidth, this.dresserDto.AlienHairColorPrimary, null);
                        }

                        if (this.dresserDto.AlienHairColorSecondary != null)
                        {
                            GUI.color = Color.white;
                            Text.Font = GameFont.Medium;
                            GUI.Label(new Rect(editorLeft, editorTop + 260, editorWidth, 30), "ChangeDresser.AlienSecondaryHairColor".Translate());
                            Text.Font = GameFont.Small;

                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + 300, editorWidth, this.dresserDto.AlienHairColorSecondary, null);
                        }
                        break;*/
                }

                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Tiny;
                GUI.Label(new Rect(0, 400, 250, 100f), GUI.tooltip);
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.UpperLeft;

                float xWidth = 150;
                float xBuffer = (this.InitialSize.x - xWidth) / 2;
                Rect bottomButtonsRect = new Rect(editorLeft, this.InitialSize.y - WidgetUtil.SelectionRowHeight - 36, xWidth, WidgetUtil.SelectionRowHeight);
                GUI.BeginGroup(bottomButtonsRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                if (Widgets.ButtonText(new Rect(0, 0, 60, WidgetUtil.SelectionRowHeight), "Reset".Translate()))
                {
                    this.ResetToDefault();
                    this.ApparelWithColorChange.Clear();
                }
                if (Widgets.ButtonText(new Rect(90, 0, 60, WidgetUtil.SelectionRowHeight), "Save".Translate()))
                {
                    this.saveChangedOnExit = true;
                    this.Close();
                }
                GUI.EndGroup();
            }
            catch (Exception e)
            {
                Log.Error("[Change Dresser] " + this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message);
                Messages.Message(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message, MessageTypeDefOf.NegativeEvent);
                base.Close();
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        private ClearColorLayers GetClearColorCallback()
        {
            ClearColorLayers callback = null;
            PawnOutfitTracker t;
            if (WorldComp.PawnOutfits.TryGetValue(this.dresserDto.Pawn, out t) && 
                t.HasApparelColors())
            {
                callback = delegate ()
                {
                    if (this.dresserDto.ApparelSelectionsContainer != null &&
                        this.dresserDto.ApparelSelectionsContainer.ColorPresetsDTO != null)
                    {
                        this.dresserDto.ApparelSelectionsContainer.ColorPresetsDTO.ClearModified();
                    }
                    t.ClearApparelColors();
                };
            }
            return callback;
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
#if TRACE
            Log.Warning(Environment.NewLine + "Begin DresserUI.ResetToDefault");
#endif

            this.dresserDto.ResetToDefault();
            this.UpdatePawn(null, null);
            
#if TRACE
            Log.Warning("End DresserUI.ResetToDefault" + Environment.NewLine);
#endif
        }

        public override void PreClose()
        {
#if TRACE
            Log.Message(Environment.NewLine + "Start DresserUI.PreClose");
#endif
            try
            {
                base.PreClose();

                if (this.dresserDto != null)
                {
                    if (this.dresserDto.ApparelSelectionsContainer?.ColorPresetsDTO?.IsModified == true)
                    {
                        IOUtil.SaveColorPresets(ColorPresetType.Apparel, this.dresserDto.ApparelSelectionsContainer.ColorPresetsDTO);
                    }
                    if (this.dresserDto.HairColorSelectionDto?.ColorPresetsDTO?.IsModified == true)
                    {
                        IOUtil.SaveColorPresets(ColorPresetType.Hair, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO);
                    }
                    if (this.dresserDto.FavoriteColorDTO?.ColorPresetsDTO?.IsModified == true)
                    {
                        IOUtil.SaveColorPresets(ColorPresetType.FavoriteColor, this.dresserDto.FavoriteColorDTO.ColorPresetsDTO);
                    }
                }

                if (this.ApparelWithColorChange != null)
                {
#if DEBUG
                    Log.Warning(" this.ApparelWithColorChange.Count: " + this.ApparelWithColorChange.Count);
#endif
                    foreach(Apparel a in this.ApparelWithColorChange)
                    {
                        PawnOutfitTracker po;
                        if (WorldComp.PawnOutfits.TryGetValue(this.dresserDto.Pawn, out po))
                        {
#if DEBUG
                            Log.Warning("  po found ");
#endif
                            //foreach (ApparelLayerDef l in a.def.apparel.layers)
                            //{
#if DEBUG
                                Log.Warning("  color change for " + a.Label);
#endif
                                po.SetApparelColor(a, a.DrawColor);
                            //}
                        }
                    }

                    this.ApparelWithColorChange.Clear();
                    this.ApparelWithColorChange = null;
                }

                if (!this.saveChangedOnExit)
                {
                    this.ResetToDefault();
                }
                PortraitsCache.SetDirty(this.dresserDto.Pawn);
                GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(this.dresserDto.Pawn);
            }
            catch (Exception e)
            {
                Log.Error("[Change Dresser] Error on DresserUI.PreClose: " + e.GetType().Name + " " + e.Message + Environment.NewLine + e.StackTrace);
            }
#if TRACE
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
                    if (this.ApparelWithColorChange == null)
                        this.ApparelWithColorChange = new List<Apparel>();

                    ApparelColorSelectionDTO dto = (ApparelColorSelectionDTO)sender;
                    CompColorableUtility.SetColor(dto.Apparel, dto.SelectedColor, true);

                    if (!this.ApparelWithColorChange.Contains(dto.Apparel))
                    {
                        this.ApparelWithColorChange.Add(dto.Apparel);
                    }
                }
                else if (sender is ApparelLayerColorSelectionDTO)
                {
                    if (this.ApparelWithColorChange == null)
                        this.ApparelWithColorChange = new List<Apparel>();

                    ApparelLayerColorSelectionDTO dto = (ApparelLayerColorSelectionDTO)sender;
                    dto.PawnOutfitTracker.SetLayerColor(dto.ApparelLayerDef, dto.SelectedColor);

                    foreach (Apparel a in this.dresserDto.Pawn.apparel.WornApparel)
                    {
                        Color c = a.DrawColor;
                        dto.PawnOutfitTracker.ApplyApparelColor(a);
                        if (a.DrawColor != c &&
                            !this.ApparelWithColorChange.Contains(a))
                        {
                            this.ApparelWithColorChange.Add(a);
                        }
                    }
                }
                else if (sender is BodyTypeSelectionDTO)
                {
                    pawn.story.bodyType = value as BodyTypeDef;
                }
                else if (sender is GenderSelectionDTO)
                {
                    pawn.gender = (Gender)value;
                }
                else if (sender is HairColorSelectionDTO)
                {
                    if ((sender as HairColorSelectionDTO).IsGradientEnabled)
                    {
                        GradientHairColorUtil.SetGradientHair(pawn, true, (Color)value);
                    }
                    else
                    {
                        pawn.story.hairColor = (Color)value;
                    }
                }
                else if (sender is HairStyleSelectionDTO)
                {
                    pawn.story.hairDef = value as HairDef;
                }
                else if (sender is BeardStyleSelectionDTO)
                {
                    pawn.style.beardDef = value as BeardDef;
                }
                else if (sender is HeadTypeSelectionDTO)
                {
                    dresserDto.SetCrownType(value);
                }
                else if (sender is SliderWidgetDTO)
                {
                    pawn.story.melanin = (float)value;
                }
                else if (sender is FavoriteColorSelectionDTO)
                {
                    pawn.story.favoriteColor = (Color)value;
                }
            }
            rerenderPawn = true;
        }
    }
}
