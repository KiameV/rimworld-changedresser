using ChangeDresser.UI.DTO.SelectionWidgetDTOs;
using ChangeDresser.UI.Enums;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RimWorld;
using System.Linq;

namespace ChangeDresser.UI.DTO
{
    class AlienDresserDTO : DresserDTO
    {
        private FieldInfo PrimarySkinColorFieldInfo = null;
        private FieldInfo SecondarySkinColorFieldInfo = null;
        private FieldInfo PrimaryHairColorFieldInfo = null;
        private FieldInfo SecondaryHairColorFieldInfo = null;

        private ThingComp alienComp = null;

        public AlienDresserDTO(Pawn pawn, CurrentEditorEnum currentEditorEnum, List<CurrentEditorEnum> editors) : base(pawn, currentEditorEnum, editors)
        {
            base.EditorTypeSelectionDto.SetSelectedEditor(currentEditorEnum);
        }

        protected override void Initialize()
        {
            object raceSettings = AlienRaceUtil.GetAlienRaceSettings(base.Pawn);
            object generalSettings = AlienRaceUtil.GetGeneralSettings(base.Pawn);
            object hairSettings = AlienRaceUtil.GetHairSettings(base.Pawn);

            foreach (ThingComp tc in base.Pawn.GetComps<ThingComp>())
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

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserAlienSkinColor))
            {
                if (PrimarySkinColorFieldInfo == null && SecondarySkinColorFieldInfo == null &&
                PrimaryHairColorFieldInfo == null && SecondaryHairColorFieldInfo == null)
                {
                    PrimarySkinColorFieldInfo = alienComp.GetType().GetField("skinColor");
                    SecondarySkinColorFieldInfo = alienComp.GetType().GetField("skinColorSecond");
                    SecondaryHairColorFieldInfo = alienComp.GetType().GetField("hairColorSecond");
#if ALIEN_DEBUG
                    Log.Warning("Field Info for primary skin color found: " + (PrimarySkinColorFieldInfo != null).ToString());
                    Log.Warning("Field Info for secondary skin color found: " + (SecondarySkinColorFieldInfo != null).ToString());
                    Log.Warning("Field Info for secondary hair color found: " + (SecondaryHairColorFieldInfo != null).ToString());
#endif
                }

#if ALIEN_DEBUG
                Log.Warning("AlienDresserDTO.initialize - start");
#endif
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

                    base.HairColorSelectionDto = new HairColorSelectionDTO(this.Pawn.story.hairColor, IOUtil.LoadColorPresets(ColorPresetType.Hair));
                    base.HairColorSelectionDto.SelectionChangeListener += this.PrimaryHairColorChange;

                    /*if (SecondaryHairColorFieldInfo != null)
                    {
                        base.AlienHairColorSecondary = new HairColorSelectionDTO((Color)SecondarySkinColorFieldInfo.GetValue(this.alienComp), IOUtil.LoadColorPresets(ColorPresetType.Hair));
                        base.AlienHairColorSecondary.SelectionChangeListener += this.SecondaryHairColorChange;
                    }*/
                }
            }

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserHair))
            {
                if (raceSettings != null)
                {
                    base.HasHair = AlienRaceUtil.HasHair(base.Pawn);
#if ALIEN_DEBUG
                    Log.Warning("initialize - got hair settings: HasHair = " + base.HasHair);
#endif
                    if (base.HasHair)
                    {
                        List<string> hairTags = AlienRaceUtil.GetHairTags(base.Pawn);
                        if (hairTags != null)
                        {
                            IEnumerable<HairDef> hairDefs = from hair in DefDatabase<HairDef>.AllDefs
                                                            where hair.hairTags.SharesElementWith(hairTags)
                                                            select hair;
#if ALIEN_DEBUG
                            System.Text.StringBuilder sb = new System.Text.StringBuilder("Hair Defs: ");
                            foreach (HairDef d in hairDefs)
                            {
                                sb.Append(d.defName);
                                sb.Append(", ");
                            }
                            Log.Warning("initialize - " + sb.ToString());
#endif

                            /*if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserHair))
                            {
                                if (hairSettings != null)
                                {
                                    List<string> filter = (List<string>)hairSettings.GetType().GetField("hairTags")?.GetValue(hairSettings);
                                    base.HairStyleSelectionDto = new HairStyleSelectionDTO(this.Pawn.story.hairDef, this.Pawn.gender, filter);
                                }
                            }*/
                            base.HairStyleSelectionDto = new HairStyleSelectionDTO(this.Pawn.story.hairDef, this.Pawn.gender, hairDefs);
                        }
                        else
                        {
                            base.HairStyleSelectionDto = new HairStyleSelectionDTO(this.Pawn.story.hairDef, this.Pawn.gender);
                        }
                    }
                    else
                    {
#if ALIEN_DEBUG
                        Log.Warning("initialize - remove hair editors");
#endif
                        base.EditorTypeSelectionDto.Remove(CurrentEditorEnum.ChangeDresserHair);//, CurrentEditorEnum.ChangeDresserAlienHairColor);
#if ALIEN_DEBUG
                        Log.Warning("initialize - hair editors removed");
#endif
                    }
                }
            }
            
            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserBody))
            {
                float maleGenderProbability = 0.5f;
                if (AlienRaceUtil.HasMaleGenderProbability(base.Pawn))
                {
#if ALIEN_DEBUG
                    Log.Warning("initialize - generalSettings found");
#endif
                    FieldInfo fi = generalSettings.GetType().GetField("MaleGenderProbability");
                    if (fi != null)
                    {
                        maleGenderProbability = AlienRaceUtil.GetMaleGenderProbability(base.Pawn);
#if ALIEN_DEBUG
                        Log.Warning("initialize - male gender prob = " + maleGenderProbability);
#endif
                    }

                    /* TODO
                    object alienPartGenerator = generalSettings.GetType().GetField("alienPartGenerator")?.GetValue(generalSettings);
                    if (alienPartGenerator != null)
                    {
                        List<string> crownTypes = (List<string>)alienPartGenerator.GetType().GetField("aliencrowntypes")?.GetValue(alienPartGenerator);
                        string crownType = (string)alienComp?.GetType().GetField("crownType")?.GetValue(alienComp);
                        if (crownTypes != null && crownType != null && crownTypes.Count > 1)
                        {
                            this.HeadTypeSelectionDto = new HeadTypeSelectionDTO(crownType, this.Pawn.gender, crownTypes);
                        }

                        List<BodyType> alienbodytypes = (List<BodyType>)alienPartGenerator.GetType().GetField("alienbodytypes")?.GetValue(alienPartGenerator);
                        if (alienbodytypes != null)
                        {
                            this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.Pawn.story.bodyType, this.Pawn.gender, alienbodytypes);
                        }
                    }*/
                }
                if (maleGenderProbability > 0f && maleGenderProbability < 1f)
                {
                    base.GenderSelectionDto = new GenderSelectionDTO(base.Pawn.gender);
                    base.GenderSelectionDto.SelectionChangeListener += GenderChange;
                }
#if ALIEN_DEBUG
                Log.Warning("initialize - done");
#endif
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
            this.Pawn.story.hairColor = base.HairColorSelectionDto.SelectedColor;//base.AlienHairColorPrimary.SelectedColor;
        }

        /*private void SecondaryHairColorChange(object sender)
        {
            SecondaryHairColorFieldInfo.SetValue(this.alienComp, base.AlienHairColorSecondary.SelectedColor);
        }*/

        private void GenderChange(object sender)
        {
            if (this.Pawn.story.bodyType == BodyType.Male && 
                this.Pawn.gender == Gender.Male)
            {
                this.Pawn.story.bodyType = BodyType.Female;
            }
            else if (this.Pawn.story.bodyType == BodyType.Female &&
                this.Pawn.gender == Gender.Female)
            {
                this.Pawn.story.bodyType = BodyType.Male;
            }
        }
    }
}
