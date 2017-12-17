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
        public bool HasHair { get; protected set; }

        protected long originalAgeBioTicks = long.MinValue;
        protected long originalAgeChronTicks = long.MinValue;

        public readonly Pawn Pawn;
        public CurrentEditorEnum CurrentEditorEnum { get; set; }

        public EditorTypeSelectionDTO EditorTypeSelectionDto { get; protected set; }

        public BodyTypeSelectionDTO BodyTypeSelectionDto { get; protected set; }
        public GenderSelectionDTO GenderSelectionDto { get; protected set; }
        public HairStyleSelectionDTO HairStyleSelectionDto { get; protected set; }
        public HairColorSelectionDTO HairColorSelectionDto { get; protected set; }
        public ApparelSelectionsContainer ApparelSelectionsContainer { get; protected set; }
        public SliderWidgetDTO SkinColorSliderDto { get; protected set; }
        public HeadTypeSelectionDTO HeadTypeSelectionDto { get; protected set; }

        public SelectionColorWidgetDTO AlienSkinColorPrimary { get; protected set; }
        public SelectionColorWidgetDTO AlienSkinColorSecondary { get; protected set; }
        //public HairColorSelectionDTO AlienHairColorPrimary { get; protected set; }
        //public HairColorSelectionDTO AlienHairColorSecondary { get; protected set; }

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
            
            this.HasHair = true;

            this.BodyTypeSelectionDto = null;
            this.GenderSelectionDto = null;
            this.HairStyleSelectionDto = null;
            this.HairColorSelectionDto = null;
            this.SkinColorSliderDto = null;
            this.HeadTypeSelectionDto = null;

            this.AlienSkinColorPrimary = null;
            this.AlienSkinColorSecondary = null;
            //this.AlienHairColorPrimary = null;
            //this.AlienHairColorSecondary = null;

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserApparelColor))
            {
                this.ApparelSelectionsContainer = new ApparelSelectionsContainer(this.Pawn.apparel.WornApparel, IOUtil.LoadColorPresets(ColorPresetType.Apparel));
            }

            this.Initialize();
        }

        protected virtual void Initialize()
        {
#if ALIEN_DEBUG
            Log.Warning("DresserDTO.initialize - start");
#endif
            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserBody))
            {
                this.originalAgeBioTicks = this.Pawn.ageTracker.AgeBiologicalTicks;
                this.originalAgeChronTicks = this.Pawn.ageTracker.AgeChronologicalTicks;

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

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserHair))
            {
                this.HairStyleSelectionDto = new HairStyleSelectionDTO(this.Pawn.story.hairDef, this.Pawn.gender);
                this.HairColorSelectionDto = new HairColorSelectionDTO(this.Pawn.story.hairColor, IOUtil.LoadColorPresets(ColorPresetType.Hair));
            }
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
            //if (this.AlienHairColorPrimary != null)
            //    this.AlienHairColorPrimary.UpdatePawnListener += updatePawn;
            //if (this.AlienHairColorSecondary != null)
            //    this.AlienHairColorSecondary.UpdatePawnListener += updatePawn;
        }

        public void ResetToDefault()
        {
#if TRACE
            Log.Warning(System.Environment.NewLine + "DresserDTO.Begin ResetToDefault");
#endif
            // Gender must happen first
            if (this.GenderSelectionDto != null)
                this.GenderSelectionDto.ResetToDefault();
            if (this.BodyTypeSelectionDto != null)
                this.BodyTypeSelectionDto.ResetToDefault();
            if (this.HairStyleSelectionDto != null)
                this.HairStyleSelectionDto.ResetToDefault();
            if (this.HairColorSelectionDto != null)
                this.HairColorSelectionDto.ResetToDefault();
            if (this.ApparelSelectionsContainer != null)
                this.ApparelSelectionsContainer.ResetToDefault();
            if (this.SkinColorSliderDto != null)
                this.SkinColorSliderDto.ResetToDefault();
            if (this.HeadTypeSelectionDto != null)
                this.HeadTypeSelectionDto.ResetToDefault();

            if (this.originalAgeBioTicks != long.MinValue)
                this.Pawn.ageTracker.AgeBiologicalTicks = this.originalAgeBioTicks;
            if (this.originalAgeChronTicks != long.MinValue)
                this.Pawn.ageTracker.AgeChronologicalTicks = this.originalAgeChronTicks;
            
            if (this.AlienSkinColorPrimary != null)
                this.AlienSkinColorPrimary.ResetToDefault();
            if (this.AlienSkinColorSecondary != null)
                this.AlienSkinColorSecondary.ResetToDefault();
            //this.AlienHairColorPrimary?.ResetToDefault();
            //this.AlienHairColorSecondary?.ResetToDefault();
#if TRACE
            Log.Warning("End DresserDTO.ResetToDefault" + System.Environment.NewLine);
#endif
        }
    }
}
