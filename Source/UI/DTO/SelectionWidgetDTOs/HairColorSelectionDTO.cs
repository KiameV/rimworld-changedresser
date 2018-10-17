using UnityEngine;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class HairColorSelectionDTO : SelectionColorWidgetDTO
    {
        public ColorPresetsDTO ColorPresetsDTO { get; private set; }

        public HairColorSelectionDTO(Color originalColor, ColorPresetsDTO presetsDto) : base(originalColor)
        {
            this.ColorPresetsDTO = presetsDto;
        }

        public new void ResetToDefault()
        {
            base.ResetToDefault();
            this.ColorPresetsDTO.Deselect();
        }
    }
}
