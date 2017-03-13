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
using UnityEngine;

namespace ChangeDresser.UI.DTO
{
    public class ColorPresetsDTO
    {
        public int Count { get { return this.ColorPresets.Length; } }

        private Color[] ColorPresets { get; set; }
        private int SelectedIndex { get; set; }
        public bool IsModified { get; set; }

        public ColorPresetsDTO()
        {
            this.ColorPresets = new Color[6];
            this.Deselect();
            this.IsModified = false;
        }

        public void Deselect()
        {
            this.SelectedIndex = -1;
        }

        public Color GetSelectedColor()
        {
            return this.ColorPresets[this.SelectedIndex];
        }

        public bool HasSelected()
        {
            return this.SelectedIndex != -1;
        }

        public bool IsSelected(int i)
        {
            return this.SelectedIndex == i;
        }

        public void SetColor(int i, Color c)
        {
            if (!this.ColorPresets[i].Equals(c))
            {
                this.ColorPresets[i] = c;
                this.IsModified = true;
            }
        }

        public void SetSelected(int i)
        {
            this.SelectedIndex = i;
        }

        internal void SetSelectedColor(Color c)
        {
            this.ColorPresets[this.SelectedIndex] = c;
            this.IsModified = true;
        }

        public Color this[int i]
        {
            get
            {
                return this.ColorPresets[i];
            }
            set
            {
                this.SetColor(i, value);
            }
        }
    }
}
