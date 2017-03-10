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
}
