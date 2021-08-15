using System.Collections.Generic;
using Verse;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class PawnSelectionWidgetDTO : ASelectionWidgetDTO
    {
        private readonly List<Pawn> Pawns;

        public PawnSelectionWidgetDTO(List<Pawn> pawns)
        {
            this.Pawns = pawns;
        }

        public override int Count => Pawns.Count;

        public override string SelectedItemLabel
        {
            get
            {
                return this.Pawns[base.index].ToString();
            }
        }

        public override object SelectedItem
        {
            get
            {
                return this.Pawns[base.index];
            }
        }

        public bool SetSelectedPawn(Pawn pawn)
        {
            int i = 0;
            while (this.SelectedItem != pawn && i < this.Count)
            {
                this.IncreaseIndex();
                ++i;
            }
            return this.SelectedItem == pawn;
        }

        public override void ResetToDefault()
        {
            // Nothing to do
        }
    }
}
