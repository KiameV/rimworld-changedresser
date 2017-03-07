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

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class ApparelSelectionsContainer
    {
        public List<ApparelColorSelectionDTO> ApparelColorSelections { get; private set; }
        public ApparelColorSelectionDTO DyeAllSelectionDto { get; private set; }
        public ApparelColorSelectionDTO SelectedApparel { get; set; }

        public ApparelSelectionsContainer(List<Apparel> apparel)
        {
            this.ApparelColorSelections = new List<ApparelColorSelectionDTO>(apparel.Count);
            foreach (Apparel a in apparel)
            {
                this.ApparelColorSelections.Add(new ApparelColorSelectionDTO(a));
            }

            this.DyeAllSelectionDto = new ApparelColorSelectionDTO(null);

            this.SelectedApparel = null;
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
        }
    }
}
