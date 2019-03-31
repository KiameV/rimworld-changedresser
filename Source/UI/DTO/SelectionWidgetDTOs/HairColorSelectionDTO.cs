using UnityEngine;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class HairColorSelectionDTO : SelectionColorWidgetDTO
    {
        public ColorPresetsDTO ColorPresetsDTO { get; private set; }
        public bool IsGradientEnabled = false;

        public HairColorSelectionDTO(Color originalColor, ColorPresetsDTO presetsDto) : base(originalColor)
        {
            this.ColorPresetsDTO = presetsDto;
        }

        public HairColorSelectionDTO(Color originalColor, ColorPresetsDTO presetsDto, bool isGradientEnabled) : base(originalColor)
        {
            this.ColorPresetsDTO = presetsDto;
            this.IsGradientEnabled = isGradientEnabled;
        }

        public new void ResetToDefault()
        {
            base.ResetToDefault();
            this.ColorPresetsDTO.Deselect();
        }
    }
}
