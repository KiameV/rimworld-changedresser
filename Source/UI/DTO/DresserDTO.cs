﻿/*
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
using ChangeDresser.Util;

namespace ChangeDresser.UI.DTO
{
    class DresserDTO
    {
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

        public DresserDTO(Pawn pawn, CurrentEditorEnum currentEditorEnum)
        {
            this.Pawn = pawn;
            this.CurrentEditorEnum = currentEditorEnum;

            this.EditorTypeSelectionDto = new EditorTypeSelectionDTO(this.CurrentEditorEnum);
            this.EditorTypeSelectionDto.SelectionChangeListener += delegate (object sender)
            {
                this.CurrentEditorEnum = (CurrentEditorEnum)this.EditorTypeSelectionDto.SelectedItem;
            };

            this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.Pawn.story.bodyType, this.Pawn.gender);
            this.HairStyleSelectionDto = new HairStyleSelectionDTO(this.Pawn.story.hairDef, this.Pawn.gender);
            this.HeadTypeSelectionDto = new HeadTypeSelectionDTO(this.Pawn.story.HeadGraphicPath, this.Pawn.gender);

            this.GenderSelectionDto = new GenderSelectionDTO(this.Pawn.gender);
            this.GenderSelectionDto.SelectionChangeListener += delegate (object sender)
            {
                this.BodyTypeSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                this.HairStyleSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                this.HeadTypeSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
            };

            this.HairColorSelectionDto = new HairColorSelectionDTO(this.Pawn.story.hairColor, IOUtil.LoadColorPresets(ColorPresetType.Hair));

            this.ApparelSelectionsContainer = new ApparelSelectionsContainer(this.Pawn.apparel.WornApparel, IOUtil.LoadColorPresets(ColorPresetType.Apparel));

            this.SkinColorSliderDto = new SliderWidgetDTO(this.Pawn.story.melanin, 0, 1);
        }

        public void SetUpdatePawnListeners(UpdatePawnListener updatePawn)
        {
            this.BodyTypeSelectionDto.UpdatePawnListener += updatePawn;
            this.GenderSelectionDto.UpdatePawnListener += updatePawn;
            this.HairStyleSelectionDto.UpdatePawnListener += updatePawn;
            this.HairColorSelectionDto.UpdatePawnListener += updatePawn;
            foreach (ApparelColorSelectionDTO dto in this.ApparelSelectionsContainer.ApparelColorSelections)
            {
                dto.UpdatePawnListener += updatePawn;
            }
            this.SkinColorSliderDto.UpdatePawnListener += updatePawn;
            this.HeadTypeSelectionDto.UpdatePawnListener += updatePawn;
        }

        public void ResetToDefault()
        {
            // Gender must happen first
            this.GenderSelectionDto.ResetToDefault();
            this.BodyTypeSelectionDto.ResetToDefault();
            this.HairStyleSelectionDto.ResetToDefault();
            this.HairColorSelectionDto.ResetToDefault();
            this.ApparelSelectionsContainer.ResetToDefault();
            this.SkinColorSliderDto.ResetToDefault();
            this.HeadTypeSelectionDto.ResetToDefault();
        }
    }
}
