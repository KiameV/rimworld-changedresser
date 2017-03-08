//using System.Collections.Generic;
//Only humans can be modified. using Verse;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    /* Leaving this here for possible Alien Race support class SpeciesSelectionWidgetDTO : ASelectionWidgetDTO
    {
        private List<ThingDef> species = new List<ThingDef>();
        public readonly ThingDef OriginalSpecies;

        public SpeciesSelectionWidgetDTO(ThingDef originalSpecies)
        {
            this.OriginalSpecies = originalSpecies;

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.defName.EqualsIgnoreCase("human") ||
                    def.defName.StartsWith("Alien_"))
                {
                    this.species.Add(def);
                }
            }

            this.ResetToDefault();
        }
        
        public override int Count { get { return this.species.Count; } }

        public override object SelectedItem { get { return this.species[base.index]; } }

        public override string SelectedItemLabel { get { return this.SelectedItem.ToString(); } }

        public override void ResetToDefault()
        {
            int i = this.species.IndexOf(this.OriginalSpecies);
            if (i != base.index)
            {
                base.index = i;
                this.IndexChanged();
            }
        }
    }*/
}
