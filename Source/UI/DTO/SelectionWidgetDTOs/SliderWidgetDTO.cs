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
using ChangeDresser.UI.Util;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class SliderWidgetDTO
    {
        public readonly float OriginalValue;
        public readonly float MinValue;
        public readonly float MaxValue;

        private float selectedValue;

        public event UpdatePawnListener UpdatePawnListener;

        public SliderWidgetDTO(float value, float minValue, float maxValue)
        {
            this.OriginalValue = value;
            this.SelectedValue = value;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }
        public float SelectedValue
        {
            get { return this.selectedValue; }
            set
            {
                if (this.selectedValue != value)
                {
                    this.selectedValue = value;
                    UpdatePawnListener?.Invoke(this, value);
                }
            }
        }

        public void ResetToDefault()
        {
            this.selectedValue = this.OriginalValue;
            this.UpdatePawnListener?.Invoke(this, this.OriginalValue);
        }
    }
}
