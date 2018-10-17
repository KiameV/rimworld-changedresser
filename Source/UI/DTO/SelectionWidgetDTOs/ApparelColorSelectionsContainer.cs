using RimWorld;
using System.Collections.Generic;
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

    class ApparelColorSelectionsContainer
    {
        public List<ApparelColorSelectionDTO> ApparelColorSelections { get; private set; }
        public List<SelectionColorWidgetDTO> SelectedApparel { get; private set; }
        public bool CopyColorSelected { get; private set; }
        public ColorPresetsDTO ColorPresetsDTO { get; private set; }
        private Color copyColor = Color.white;

        public ApparelColorSelectionsContainer(List<Apparel> apparel, ColorPresetsDTO presetsDto)
        {
            this.ApparelColorSelections = new List<ApparelColorSelectionDTO>(apparel.Count);
            foreach (Apparel a in apparel)
            {
                this.ApparelColorSelections.Add(new ApparelColorSelectionDTO(a));
            }
            this.SelectedApparel = new List<SelectionColorWidgetDTO>();
            this.ColorPresetsDTO = presetsDto;
            this.CopyColorSelected = false;
        }

        public Color CopyColor
        {
            get { return this.copyColor; }
            set
            {
                this.copyColor = value;
                this.CopyColorSelected = true;
            }
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
            this.SelectedApparel.Clear();
            this.ColorPresetsDTO.Deselect();
        }

        public void DeselectAll()
        {
            this.SelectedApparel.Clear();
        }

        public void SelectAll()
        {
            this.DeselectAll();
            this.ColorPresetsDTO.Deselect();
            foreach (ApparelColorSelectionDTO dto in this.ApparelColorSelections)
            {
                this.SelectedApparel.Add(dto);
            }
        }

        public void Select(ApparelColorSelectionDTO dto, bool isShiftPressed)
        {
            this.ColorPresetsDTO.Deselect();
            if (!isShiftPressed)
            {
                this.DeselectAll();
                this.SelectedApparel.Add(dto);
            }
            else
            {
                bool removed = this.SelectedApparel.Remove(dto);
                if (!removed)
                {
                    this.SelectedApparel.Add(dto);
                }
            }
        }

        public bool IsSelected(ApparelColorSelectionDTO dto)
        {
            return this.SelectedApparel.Contains(dto);
        }
    }
}
