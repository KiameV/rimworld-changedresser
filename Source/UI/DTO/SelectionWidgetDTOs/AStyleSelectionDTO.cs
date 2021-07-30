using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    abstract class AStyleSelectionDTO : ASelectionWidgetDTO
    {
        public StyleItemDef OriginalStyleDef;
        protected List<StyleItemDef> styleDefs;
        protected List<StyleItemDef> maleDefs = new List<StyleItemDef>();
        protected List<StyleItemDef> femaleDefs = new List<StyleItemDef>();
        private StyleItemDef mouseOverHairDef = null;
        private int savedFemaleIndex = 0;
        private int savedMaleIndex = 0;

        protected void FindIndex(StyleItemDef hairDef)
        {
            for (int i = 0; i < this.styleDefs.Count; ++i)
            {
                if (this.styleDefs[i].label.Equals(hairDef.label))
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
                    this.styleDefs = this.femaleDefs;
                    base.index = savedFemaleIndex;
                }
                else // Male
                {
                    this.savedFemaleIndex = base.index;
                    this.styleDefs = this.maleDefs;
                    base.index = savedMaleIndex;
                }
                base.IndexChanged();
            }
        }

        public override int Count
        {
            get
            {
                return this.styleDefs.Count;
            }
        }

        public override string SelectedItemLabel
        {
            get
            {
                return this.styleDefs[base.index].ToString();
            }
        }

        public override object SelectedItem
        {
            get
            {
                return this.styleDefs[base.index];
            }
        }

        public StyleItemDef MouseOverSelection
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

        public StyleItemDef this[int i]
        {
            get
            {
                return this.styleDefs[i];
            }
        }

        public override void ResetToDefault()
        {
            this.FindIndex(this.OriginalStyleDef);
            base.IndexChanged();
        }
    }

    class HairStyleSelectionDTO : AStyleSelectionDTO
    {
        public HairStyleSelectionDTO(HairDef hairDef, Gender gender, bool shareAcrossGenders) : base()
        {
            this.OriginalStyleDef = hairDef;

            foreach (HairDef def in DefDatabase<HairDef>.AllDefs)
            {
                if (shareAcrossGenders)
                {
                    this.maleDefs.Add(def);
                    this.femaleDefs.Add(def);
                }
                else
                {
                    if (def.styleGender == StyleGender.Male ||
                        def.styleGender == StyleGender.MaleUsually ||
                        def.styleGender == StyleGender.Any)
                    {
                        this.maleDefs.Add(def);
                    }

                    if (def.styleGender == StyleGender.Female ||
                        def.styleGender == StyleGender.FemaleUsually ||
                        def.styleGender == StyleGender.Any)
                    {
                        this.femaleDefs.Add(def);
                    }
                }
            }

            this.Gender = gender;
            base.FindIndex(hairDef);
        }

        public HairStyleSelectionDTO(StyleItemDef styleDef, Gender gender, IEnumerable<HairDef> hairDefs)
        {
            this.OriginalStyleDef = styleDef;
            this.femaleDefs = new List<StyleItemDef>(styleDefs);
            this.maleDefs = this.femaleDefs;
            this.Gender = gender;
            this.FindIndex(styleDef);
        }
    }

    class BeardStyleSelectionDTO : AStyleSelectionDTO
    {
        public BeardStyleSelectionDTO(BeardDef beardDef, Gender gender) : base()
        {
            this.OriginalStyleDef = beardDef;

            foreach (BeardDef def in DefDatabase<BeardDef>.AllDefs)
            {
                this.maleDefs.Add(def);
                this.femaleDefs.Add(def);
            }

            this.Gender = gender;
            base.FindIndex(beardDef);
        }

        public BeardStyleSelectionDTO(BeardDef beardDef, Gender gender, IEnumerable<BeardDef> beardDefs)
        {
            this.OriginalStyleDef = beardDef;
            this.femaleDefs = new List<StyleItemDef>(styleDefs);
            this.maleDefs = this.femaleDefs;
            this.Gender = gender;
            this.FindIndex(beardDef);
        }
    }
}