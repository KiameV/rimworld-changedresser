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
using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class ApparelColorSelectionDTO : SelectionColorWidgetDTO
    {
        public readonly Apparel Apparel;

        public ApparelColorSelectionDTO(Apparel apparel) : base((apparel != null) ? apparel.DrawColor : Color.white)
        {
            this.Apparel = apparel;
        }

        public override bool Equals(object o)
        {
            if (o != null &&
                o is ApparelColorSelectionDTO)
            {
                return this.Apparel.Label.Equals(((ApparelColorSelectionDTO)o).Apparel.Label);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Apparel.Label.GetHashCode();
        }
    }

    class ApparelColorSelectionsContainer
    {
        public List<ApparelColorSelectionDTO> ApparelColorSelections { get; private set; }
        public List<SelectionColorWidgetDTO> SelectedApparel { get; private set; }
        public bool CopyColorSelected { get; private set; }
        public ColorPresetsDTO ColorPresetsDTO { get; private set; }
        private Color copyColor = Color.white;

        public ApparelColorSelectionsContainer(List<Apparel> apparel, ColorPresetsDTO presetsDto)
        {
            this.ApparelColorSelections = new List<ApparelColorSelectionDTO>(apparel.Count);
            foreach (Apparel a in apparel)
            {
                this.ApparelColorSelections.Add(new ApparelColorSelectionDTO(a));
            }
            this.SelectedApparel = new List<SelectionColorWidgetDTO>();
            this.ColorPresetsDTO = presetsDto;
            this.CopyColorSelected = false;
        }

        public Color CopyColor
        {
            get { return this.copyColor; }
            set
            {
                this.copyColor = value;
                this.CopyColorSelected = true;
            }
        }

        public int Count { get { return this.ApparelColorSelections.Count; } }

        public ApparelColorSelectionDTO this[int i]
        {
            get { return this.ApparelColorSelections[i]; }
        }

        public void ResetToDefault()
        {
            foreach (ApparelColorSelectionDTO dto in this.ApparelColorSelections)
            {
                dto.ResetToDefault();
            }
            this.SelectedApparel.Clear();
            this.ColorPresetsDTO.Deselect();
        }

        public void DeselectAll()
        {
            this.SelectedApparel.Clear();
        }

        public void SelectAll()
        {
            this.DeselectAll();
            this.ColorPresetsDTO.Deselect();
            foreach (ApparelColorSelectionDTO dto in this.ApparelColorSelections)
            {
                this.SelectedApparel.Add(dto);
            }
        }

        public void Select(ApparelColorSelectionDTO dto, bool isShiftPressed)
        {
            this.ColorPresetsDTO.Deselect();
            if (!isShiftPressed)
            {
                this.DeselectAll();
                this.SelectedApparel.Add(dto);
            }
            else
            {
                bool removed = this.SelectedApparel.Remove(dto);
                if (!removed)
                {
                    this.SelectedApparel.Add(dto);
                }
            }
        }

        public bool IsSelected(ApparelColorSelectionDTO dto)
        {
            return this.SelectedApparel.Contains(dto);
        }
    }
}
