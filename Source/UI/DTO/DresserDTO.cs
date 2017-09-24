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
using ChangeDresser.UI.DTO.SelectionWidgetDTOs;
using ChangeDresser.UI.Enums;
using Verse;
using ChangeDresser.UI.Util;
using System.Collections.Generic;

namespace ChangeDresser.UI.DTO
{
    class DresserDTO
    {
        private long originalAgeBioTicks = long.MinValue;
        private long originalAgeChronTicks = long.MinValue;

        public Pawn Pawn { get; private set; }
        public CurrentEditorEnum CurrentEditorEnum { get; set; }

        public EditorTypeSelectionDTO EditorTypeSelectionDto { get; private set; }
        public BodyTypeSelectionDTO BodyTypeSelectionDto { get; private set; }
        public GenderSelectionDTO GenderSelectionDto { get; private set; }
        public HairStyleSelectionDTO HairStyleSelectionDto { get; private set; }
        public HairColorSelectionDTO HairColorSelectionDto { get; private set; }
        public ApparelSelectionsContainer ApparelSelectionsContainer { get; private set; }
        public SliderWidgetDTO SkinColorSliderDto { get; private set; }
        public HeadTypeSelectionDTO HeadTypeSelectionDto { get; private set; }

        public SelectionColorWidgetDTO AlienSkinColorPrimary { get; protected set; }
        public SelectionColorWidgetDTO AlienSkinColorSecondary { get; protected set; }
        public HairColorSelectionDTO AlienHairColorPrimary { get; protected set; }
        public HairColorSelectionDTO AlienHairColorSecondary { get; protected set; }

        public DresserDTO(Pawn pawn, CurrentEditorEnum currentEditorEnum, List<CurrentEditorEnum> editors)
        {
            this.Pawn = pawn;

            this.CurrentEditorEnum = currentEditorEnum;

            this.EditorTypeSelectionDto = new EditorTypeSelectionDTO(this.CurrentEditorEnum, editors);
            this.EditorTypeSelectionDto.SelectionChangeListener += delegate (object sender)
            {
                this.CurrentEditorEnum = (CurrentEditorEnum)this.EditorTypeSelectionDto.SelectedItem;
                if (this.CurrentEditorEnum == CurrentEditorEnum.ChangeDresserHair)
                {
                    Prefs.HatsOnlyOnMap = true;
                }
                else
                {
                    Prefs.HatsOnlyOnMap = false;
                }
            };

            if (editors.Contains(CurrentEditorEnum.ChangeDresserApparelColor))
            {
                this.ApparelSelectionsContainer = new ApparelSelectionsContainer(this.Pawn.apparel.WornApparel, IOUtil.LoadColorPresets(ColorPresetType.Apparel));
            }

            if (editors.Contains(CurrentEditorEnum.ChangeDresserBody))
            {
                this.originalAgeBioTicks = pawn.ageTracker.AgeBiologicalTicks;
                this.originalAgeChronTicks = pawn.ageTracker.AgeChronologicalTicks;

                this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.Pawn.story.bodyType, this.Pawn.gender);

                this.HeadTypeSelectionDto = new HeadTypeSelectionDTO(this.Pawn.story.HeadGraphicPath, this.Pawn.gender);

                this.SkinColorSliderDto = new SliderWidgetDTO(this.Pawn.story.melanin, 0, 1);

                this.GenderSelectionDto = new GenderSelectionDTO(this.Pawn.gender);
                this.GenderSelectionDto.SelectionChangeListener += delegate (object sender)
                {
                    this.BodyTypeSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                    this.HairStyleSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                    this.HeadTypeSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                };
            }

            if (editors.Contains(CurrentEditorEnum.ChangeDresserHair))
            {
                this.HairStyleSelectionDto = new HairStyleSelectionDTO(this.Pawn.story.hairDef, this.Pawn.gender);
                this.HairColorSelectionDto = new HairColorSelectionDTO(this.Pawn.story.hairColor, IOUtil.LoadColorPresets(ColorPresetType.Hair));
            }

            this.AlienSkinColorPrimary = null;
            this.AlienSkinColorSecondary = null;
            this.AlienHairColorPrimary = null;
            this.AlienHairColorSecondary = null;
        }

        public void SetUpdatePawnListeners(UpdatePawnListener updatePawn)
        {
            if (this.ApparelSelectionsContainer != null)
            {
                foreach (ApparelColorSelectionDTO dto in this.ApparelSelectionsContainer.ApparelColorSelections)
                {
                    dto.UpdatePawnListener += updatePawn;
                }
            }

            if (this.BodyTypeSelectionDto != null)
                this.BodyTypeSelectionDto.UpdatePawnListener += updatePawn;
            if (this.GenderSelectionDto != null)
                this.GenderSelectionDto.UpdatePawnListener += updatePawn;
            if (this.HairStyleSelectionDto != null)
                this.HairStyleSelectionDto.UpdatePawnListener += updatePawn;
            if (this.HairColorSelectionDto != null)
                this.HairColorSelectionDto.UpdatePawnListener += updatePawn;
            if (this.SkinColorSliderDto != null)
                this.SkinColorSliderDto.UpdatePawnListener += updatePawn;
            if (this.HeadTypeSelectionDto != null)
                this.HeadTypeSelectionDto.UpdatePawnListener += updatePawn;

            if (this.AlienSkinColorPrimary != null)
                this.AlienSkinColorPrimary.UpdatePawnListener += updatePawn;
            if (this.AlienSkinColorSecondary != null)
                this.AlienSkinColorSecondary.UpdatePawnListener += updatePawn;
            if (this.AlienHairColorPrimary != null)
                this.AlienHairColorPrimary.UpdatePawnListener += updatePawn;
            if (this.AlienHairColorSecondary != null)
                this.AlienHairColorSecondary.UpdatePawnListener += updatePawn;
        }

        public void ResetToDefault()
        {
#if TRACE
            Log.Warning(System.Environment.NewLine + "DresserDTO.Begin ResetToDefault");
#endif
            // Gender must happen first
            this.GenderSelectionDto?.ResetToDefault();
            this.BodyTypeSelectionDto?.ResetToDefault();
            this.HairStyleSelectionDto?.ResetToDefault();
            this.HairColorSelectionDto?.ResetToDefault();
            this.ApparelSelectionsContainer?.ResetToDefault();
            this.SkinColorSliderDto?.ResetToDefault();
            this.HeadTypeSelectionDto?.ResetToDefault();
            if (this.originalAgeBioTicks != long.MinValue)
                this.Pawn.ageTracker.AgeBiologicalTicks = this.originalAgeBioTicks;
            if (this.originalAgeChronTicks != long.MinValue)
                this.Pawn.ageTracker.AgeChronologicalTicks = this.originalAgeChronTicks;

            
            this.AlienSkinColorPrimary?.ResetToDefault();
            this.AlienSkinColorSecondary?.ResetToDefault();
            this.AlienHairColorPrimary?.ResetToDefault();
            this.AlienHairColorSecondary?.ResetToDefault();
#if TRACE
            Log.Warning("End DresserDTO.ResetToDefault" + System.Environment.NewLine);
#endif
        }
    }
}
