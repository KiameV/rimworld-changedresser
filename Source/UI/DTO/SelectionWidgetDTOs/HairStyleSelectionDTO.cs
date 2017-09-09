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
using Verse;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class HairStyleSelectionDTO : ASelectionWidgetDTO
    {
        public HairDef OriginalHairDef;
        private List<HairDef> hairDefs;
        private List<HairDef> maleHairDefs = new List<HairDef>();
        private List<HairDef> femaleHairDefs = new List<HairDef>();
        private HairDef mouseOverHairDef = null;
        private int savedFemaleIndex = 0;
        private int savedMaleIndex = 0;

        public HairStyleSelectionDTO(HairDef hairDef, Gender gender) : base()
        {
            this.OriginalHairDef = hairDef;

            foreach (HairDef def in DefDatabase<HairDef>.AllDefs)
            {
                if (def.hairGender == HairGender.Male ||
                    def.hairGender == HairGender.MaleUsually ||
                    def.hairGender == HairGender.Any)
                {
                    this.maleHairDefs.Add(def);
                }

                if (def.hairGender == HairGender.Female ||
                    def.hairGender == HairGender.FemaleUsually ||
                    def.hairGender == HairGender.Any)
                {
                    this.femaleHairDefs.Add(def);
                }
            }

            this.Gender = gender;
            this.FindIndex(hairDef);
        }

        private void FindIndex(HairDef hairDef)
        {
            for (int i = 0; i < this.hairDefs.Count; ++i)
            {
                if (this.hairDefs[i].label.Equals(hairDef.label))
                {
                    base.index = i;
                    break;
                }
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
            set
            {
                this.index = value;
            }
        }

        public Gender Gender
        {
            set
            {
                if (value == Gender.Female)
                {
                    this.savedMaleIndex = base.index;
                    this.hairDefs = this.femaleHairDefs;
                    base.index = savedFemaleIndex;
                }
                else // Male
                {
                    this.savedFemaleIndex = base.index;
                    this.hairDefs = this.maleHairDefs;
                    base.index = savedMaleIndex;
                }
                base.IndexChanged();
            }
        }

        public override int Count
        {
            get
            {
                return this.hairDefs.Count;
            }
        }

        public override string SelectedItemLabel
        {
            get
            {
                return this.hairDefs[base.index].ToString();
            }
        }

        public override object SelectedItem
        {
            get
            {
                return this.hairDefs[base.index];
            }
        }

        public HairDef MouseOverSelection
        {
            get
            {
                return this.mouseOverHairDef;
            }
            set
            {
                if (value == null && this.mouseOverHairDef != null)
                {
                    this.mouseOverHairDef = null;
                    base.IndexChanged();
                }
                if (this.mouseOverHairDef != value)
                {
                    this.mouseOverHairDef = value;
                    base.UpdatePawn(value);
                }
            }
        }

        public HairDef this[int i]
        {
            get
            {
                return this.hairDefs[i];
            }
        }

        public override void ResetToDefault()
        {
            this.FindIndex(this.OriginalHairDef);
            base.IndexChanged();
        }
    }
}