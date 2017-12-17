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
using ChangeDresser.UI.Util;
using UnityEngine;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class SelectionColorWidgetDTO
    {
        public event SelectionChangeListener SelectionChangeListener;
        public event UpdatePawnListener UpdatePawnListener;

        public readonly Color OriginalColor;
        private Color selectedColor;

        public Color SelectedColor
        {
            get { return this.selectedColor; }
            set
            {
                if (!this.selectedColor.Equals(value))
                {
                    this.selectedColor = value;
                    if (this.SelectionChangeListener != null)
                        this.SelectionChangeListener.Invoke(this);
                    if (this.UpdatePawnListener != null)
                        this.UpdatePawnListener.Invoke(this, this.selectedColor);
                }
            }
        }

        public SelectionColorWidgetDTO(Color color)
        {
            this.OriginalColor = color;
            this.selectedColor = color;
        }

        public void ResetToDefault()
        {
            this.selectedColor = this.OriginalColor;
            if (this.UpdatePawnListener != null)
                this.UpdatePawnListener.Invoke(this, this.OriginalColor);
        }
    }
}
