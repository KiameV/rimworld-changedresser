using ChangeDresser.UI.DTO.SelectionWidgetDTOs;
using ChangeDresser.UI.Enums;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RimWorld;
using System.Linq;
using static AlienRace.AlienPartGenerator;
using AlienRace;

namespace ChangeDresser.UI.DTO
{
    class AlienDresserDTO : DresserDTO
    {
        private ThingComp alienComp = null;

        public AlienDresserDTO(Pawn pawn, CurrentEditorEnum currentEditorEnum, IEnumerable<CurrentEditorEnum> editors) : base(pawn, currentEditorEnum, editors)
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
                if (tc is AlienComp ac)
                {
                    this.alienComp = ac;
#if ALIEN_DEBUG
                    Log.Warning("Alien Comp found!");
#endif
                    break;
                }
            }

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserAlienSkinColor))
            {
#if ALIEN_DEBUG
                Log.Warning("AlienDresserDTO.initialize - start");
#endif
                if (this.alienComp is AlienComp ac)
                {
                    var c = ac.GetChannel("skin");
                    if (c != null)
                    {
                        base.AlienSkinColorPrimary = new SelectionColorWidgetDTO(c.first);
                        base.AlienSkinColorPrimary.SelectionChangeListener += this.PrimarySkinColorChange;

                        base.AlienSkinColorSecondary = new SelectionColorWidgetDTO(c.second);
                        base.AlienSkinColorPrimary.SelectionChangeListener += this.SecondarySkinColorChange;
                    }

                    if (base.Pawn.def is ThingDef_AlienRace ar && 
                        ar.alienRace.hairSettings.hasHair)
                    {
                        base.HairColorSelectionDto = new HairColorSelectionDTO(this.Pawn.story.hairColor, IOUtil.LoadColorPresets(ColorPresetType.Hair));
                        base.HairColorSelectionDto.SelectionChangeListener += this.PrimaryHairColorChange;

                        ColorPresetsDTO hairColorPresets = IOUtil.LoadColorPresets(ColorPresetType.Hair);
                        if (GradientHairColorUtil.IsGradientHairAvailable)
                        {
                            if (!GradientHairColorUtil.GetGradientHair(this.Pawn, out bool enabled, out Color color))
                            {
                                enabled = false;
                                color = Color.white;
                            }
                            base.GradientHairColorSelectionDto = new HairColorSelectionDTO(color, hairColorPresets, enabled);
                            base.GradientHairColorSelectionDto.SelectionChangeListener += this.GradientHairColorChange;
                        }
                    }
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

                    fi = generalSettings.GetType().GetField("alienPartGenerator");
                    if (fi != null)
                    {
                        object alienPartGenerator = fi.GetValue(generalSettings);
                        if (alienPartGenerator != null)
                        {
                            Log.Warning($"{this.Pawn.def.defName} - {this.Pawn.story.crownType.ToString()}");
                            fi = alienPartGenerator.GetType().GetField("aliencrowntypes");
                            Log.Warning("Crown Types:");
                            foreach (var ct in (List<string>)fi.GetValue(alienPartGenerator))
                                Log.Warning($"{ct}");
                            if (fi != null && alienComp != null)
                            {
                                List<string> crownTypes = (List<string>)fi.GetValue(alienPartGenerator);
                                fi = alienComp.GetType().GetField("crownType");
                                if (fi != null)
                                {
                                    string crownType = (string)fi.GetValue(alienComp);
                                    if (crownTypes != null && crownType != null && crownTypes.Count > 1)
                                    {
                                        this.HeadTypeSelectionDto = new HeadTypeSelectionDTO(crownType, this.Pawn.gender, crownTypes);
                                    }
                                }
                            }

                            try
                            {
                                fi = alienPartGenerator.GetType().GetField("alienbodytypes");
                                if (fi != null)
                                {
                                    //Log.Warning("Get story");
                                    //Log.Warning(this.Pawn.story.bodyType.ToString());
                                    List<BodyTypeDef> alienbodytypes = (List<BodyTypeDef>)fi.GetValue(alienPartGenerator);
                                    if (alienbodytypes != null && alienbodytypes.Count > 0)
                                    {
                                        //Log.Warning("Found body types");
                                        this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.Pawn.story.bodyType, this.Pawn.gender, alienbodytypes);
                                        //Log.Warning("Body Types loaded");
                                    }
                                    else
                                    {
                                        Log.Warning("No alien body types found. Defaulting to human.");
                                        this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.Pawn.story.bodyType, this.Pawn.gender);
                                    }
                                }
                            }
                            catch
                            {
                                Log.Warning("Problem getting alien body types. Defaulting to human.");
                                this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.Pawn.story.bodyType, this.Pawn.gender);
                            }
                        }
                    }
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
            var c = (this.alienComp as AlienComp)?.GetChannel("skin");
            if (c != null)
                c.first = base.AlienSkinColorPrimary.SelectedColor;
        }

        private void SecondarySkinColorChange(object sender)
        {
            var c = (this.alienComp as AlienComp)?.GetChannel("skin");
            if (c != null)
                c.second = base.AlienSkinColorPrimary.SelectedColor;
        }

        private void PrimaryHairColorChange(object sender)
        {
            this.Pawn.story.hairColor = base.HairColorSelectionDto.SelectedColor;//base.AlienHairColorPrimary.SelectedColor;
        }

        private void GradientHairColorChange(object sender)
        {
            GradientHairColorUtil.SetGradientHair(base.Pawn, base.GradientHairColorSelectionDto.IsGradientEnabled, base.GradientHairColorSelectionDto.SelectedColor);
        }

        /*private void SecondaryHairColorChange(object sender)
        {
            SecondaryHairColorFieldInfo.SetValue(this.alienComp, base.AlienHairColorSecondary.SelectedColor);
        }*/

        private void GenderChange(object sender)
        {
            if (this.Pawn.story.bodyType == BodyTypeDefOf.Male && 
                this.Pawn.gender == Gender.Male)
            {
                this.Pawn.story.bodyType = BodyTypeDefOf.Female;
            }
            else if (this.Pawn.story.bodyType == BodyTypeDefOf.Female &&
                this.Pawn.gender == Gender.Female)
            {
                this.Pawn.story.bodyType = BodyTypeDefOf.Male;
            }
        }
    }
}
