using ChangeDresser.UI.DTO.SelectionWidgetDTOs;
using ChangeDresser.UI.Enums;
using Verse;
using ChangeDresser.UI.Util;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

namespace ChangeDresser.UI.DTO
{
    class AlienDresserDTO : DresserDTO
    {
        private static FieldInfo PrimarySkinColorFieldInfo = null;
        private static FieldInfo SecondarySkinColorFieldInfo = null;
        private static FieldInfo PrimaryHairColorFieldInfo = null;
        private static FieldInfo SecondaryHairColorFieldInfo = null;

        private readonly ThingComp alienComp = null;

        public AlienDresserDTO(Pawn pawn, CurrentEditorEnum currentEditorEnum, List<CurrentEditorEnum> editors) : base(pawn, currentEditorEnum, editors)
        {
            foreach (ThingComp tc in this.Pawn.GetComps<ThingComp>())
            {
#if ALIEN_DEBUG
                Log.Warning(" comp: " + tc.GetType().Namespace + "." + tc.GetType().Name);
#endif
                if (tc.GetType().Namespace.EqualsIgnoreCase("AlienRace") &&
                    tc.GetType().Name.EqualsIgnoreCase("AlienComp"))
                {
                    this.alienComp = tc;
#if ALIEN_DEBUG
                    Log.Warning("Alien Comp found!");
#endif
                    break;
                }
            }

            if (PrimarySkinColorFieldInfo == null && SecondarySkinColorFieldInfo == null && 
                PrimaryHairColorFieldInfo == null && SecondaryHairColorFieldInfo == null)
            {
                PrimarySkinColorFieldInfo = alienComp.GetType().GetField("skinColor");
                SecondarySkinColorFieldInfo = alienComp.GetType().GetField("skinColorSecond");
                SecondaryHairColorFieldInfo = alienComp.GetType().GetField("hairColorSecond");
#if ALIEN_DEBUG
                Log.Warning("Field Info for primary skin color found: " + (PrimarySkinColorFieldInfo != null).ToString());
                Log.Warning("Field Info for secondary skin color found: " + (SecondarySkinColorFieldInfo != null).ToString());
#endif
            }

            if (this.alienComp != null)
            {
                if (PrimarySkinColorFieldInfo != null)
                {
                    base.AlienSkinColorPrimary = new SelectionColorWidgetDTO((Color)PrimarySkinColorFieldInfo.GetValue(this.alienComp));
                    base.AlienSkinColorPrimary.SelectionChangeListener += this.PrimarySkinColorChange;
                }

                if (SecondarySkinColorFieldInfo != null)
                {
                    base.AlienSkinColorSecondary = new SelectionColorWidgetDTO((Color)SecondarySkinColorFieldInfo.GetValue(this.alienComp));
                    base.AlienSkinColorPrimary.SelectionChangeListener += this.SecondarySkinColorChange;
                }
                
                base.AlienHairColorPrimary = new HairColorSelectionDTO(this.Pawn.story.hairColor, IOUtil.LoadColorPresets(ColorPresetType.Hair));
                base.AlienHairColorPrimary.SelectionChangeListener += this.PrimaryHairColorChange;

                if (SecondaryHairColorFieldInfo != null)
                {
                    base.AlienHairColorSecondary = new HairColorSelectionDTO((Color)SecondarySkinColorFieldInfo.GetValue(this.alienComp), IOUtil.LoadColorPresets(ColorPresetType.Hair));
                    base.AlienHairColorSecondary.SelectionChangeListener += this.SecondaryHairColorChange;
                }
            }
        }

        private void PrimarySkinColorChange(object sender)
        {
            PrimarySkinColorFieldInfo.SetValue(this.alienComp, base.AlienSkinColorPrimary.SelectedColor);
        }

        private void SecondarySkinColorChange(object sender)
        {
            SecondarySkinColorFieldInfo.SetValue(this.alienComp, base.AlienSkinColorSecondary.SelectedColor);
        }

        private void PrimaryHairColorChange(object sender)
        {
            this.Pawn.story.hairColor = base.AlienHairColorPrimary.SelectedColor;
        }

        private void SecondaryHairColorChange(object sender)
        {
            SecondaryHairColorFieldInfo.SetValue(this.alienComp, base.AlienHairColorSecondary.SelectedColor);
        }
    }
}
