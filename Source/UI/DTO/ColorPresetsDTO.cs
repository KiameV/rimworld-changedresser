using System;
using UnityEngine;

namespace ChangeDresser.UI.DTO
{
    public class ColorPresetsDTO
    {
        public int Count { get { return this.ColorPresets.Length; } }

        private Color[] ColorPresets { get; set; }
        private int SelectedIndex { get; set; }

        public ColorPresetsDTO()
        {
            this.ColorPresets = new Color[6];
            this.Deselect();
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
            this.ColorPresets[i] = c;
        }

        public void SetSelected(int i)
        {
            this.SelectedIndex = i;
        }

        internal void SetSelectedColor(Color c)
        {
            this.ColorPresets[this.SelectedIndex] = c;
        }

        public Color this[int i]
        {
            get
            {
                return this.ColorPresets[i];
            }
            set
            {
                this.ColorPresets[i] = value;
            }
        }
    }
}
