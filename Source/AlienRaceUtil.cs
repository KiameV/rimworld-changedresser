using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace ChangeDresser
{
    class AlienRaceUtil
    {
        private static bool alienRaceEnabled = false;
        private static bool initialized = false;
        private static List<ThingDef> alienRaces = new List<ThingDef>(0);

        #region Get Field/Metho Info
        private static FieldInfo raceSettingsFieldInfo = null;
        private static FieldInfo styleSettingsFieldInfo = null;
        private static FieldInfo generalSettingsFieldInfo = null;

        public static object GetAlienRaceSettings(Pawn pawn)
        {
            if (raceSettingsFieldInfo == null)
            {
                raceSettingsFieldInfo = pawn.def.GetType().GetField("alienRace");
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
                Log.Warning("raceSettingsFieldInfo found: " + (string)((raceSettingsFieldInfo != null) ? "True" : "False"));
#endif
            }
            if (raceSettingsFieldInfo == null)
            {
                Log.ErrorOnce("Unable to get raceSettingsFieldInfo", "raceSettingsFieldInfo".GetHashCode());
                return null;
            }
            return raceSettingsFieldInfo.GetValue(pawn.def);
        }

        public static object GetStyleSettings(Pawn pawn)
        {
            object raceSettings = GetAlienRaceSettings(pawn);
            if (raceSettings == null)
            {
                return null;
            }
            if (styleSettingsFieldInfo == null)
            {
                styleSettingsFieldInfo = raceSettings.GetType().GetField("styleSettings");
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
                Log.Warning("hairSettingsFieldInfo found: " + (string)((hairSettingsFieldInfo != null) ? "True" : "False"));
#endif
            }
            if (styleSettingsFieldInfo == null)
            {
                Log.ErrorOnce("Unable to get styleSettingsFieldInfo", "styleSettingsFieldInfo".GetHashCode());
                return null;
            }
            return styleSettingsFieldInfo.GetValue(raceSettings) as Dictionary<Type, AlienRace.StyleSettings>;
        }

        public static bool HasHair(Pawn pawn)
        {
            Dictionary<Type, AlienRace.StyleSettings> styleSettings = GetStyleSettings(pawn) as Dictionary<Type, AlienRace.StyleSettings>;
            if (styleSettings == null)
            {
                return false;
            }
            return styleSettings.ContainsKey(typeof(HairDef));
        }

        public static object GetGeneralSettings(Pawn pawn)
        {
            object raceSettings = GetAlienRaceSettings(pawn);
            if (raceSettings == null)
            {
                return null;
            }
            if (generalSettingsFieldInfo == null)
            {
                generalSettingsFieldInfo = raceSettings.GetType().GetField("generalSettings");
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
                Log.Warning("generalSettingsFieldInfo found: " + (string)((generalSettingsFieldInfo != null) ? "True" : "False"));
#endif
            }
            if (generalSettingsFieldInfo == null)
            {
                Log.ErrorOnce("Unable to get generalSettings", "generalSettings".GetHashCode());
                return null;
            }
            return generalSettingsFieldInfo.GetValue(raceSettings);
        }

        public static List<string> GetHairTags(Pawn pawn)
        {
            Dictionary<Type, AlienRace.StyleSettings> styleSettings = GetStyleSettings(pawn) as Dictionary<Type, AlienRace.StyleSettings>;
            AlienRace.StyleSettings s = null;
            if (styleSettings?.TryGetValue(typeof(HairDef), out s) == false)
            {
                Log.ErrorOnce("Unable to get hairTags", "hairTags".GetHashCode());
                return null;
            }
            return s.styleTags;
        }

        private static FieldInfo GetMaleGenderProbabilityFieldInfo(Pawn pawn)
        {
            object generalSettings = GetGeneralSettings(pawn);
            if (generalSettings == null)
                return null;

            return generalSettings.GetType().GetField("maleGenderProbability");
        }

        public static bool HasMaleGenderProbability(Pawn pawn)
        {
            return GetMaleGenderProbabilityFieldInfo(pawn) != null;
        }

        public static float GetMaleGenderProbability(Pawn pawn)
        {
            FieldInfo fi = GetMaleGenderProbabilityFieldInfo(pawn);
            if (fi == null)
            {
                Log.ErrorOnce("Unable to get male gender probability. Setting to 0.5f", "maleGenderProbability".GetHashCode());
                return .5f;
            }
            return (float)fi.GetValue(GetGeneralSettings(pawn));
        }
        #endregion

        public static bool Exists
        {
            get
            {
#if ALIEN_DEBUG && DEBUG
                Log.Warning("Aliens Exists?");
#endif
                if (!initialized)
                {
#if ALIEN_DEBUG && DEBUG
                    Log.Warning("Aliens Not Initialized");
#endif
                    foreach (ModContentPack pack in LoadedModManager.RunningMods)
                    {
                        foreach (Assembly assembly in pack.assemblies.loadedAssemblies)
                        {
                            if (assembly.GetName().Name.Equals("AlienRace") &&
                                assembly.GetType("AlienRace.ThingDef_AlienRace") != null)
                            {
                                initialized = true;
                                alienRaceEnabled = true;
                                break;
                            }
                        }
                        if (initialized)
                        {
                            break;
                        }
                    }
                    initialized = true;
                }
#if ALIEN_DEBUG && DEBUG
                Log.Warning("Aliens Exists: " + ((bool)(alienRaceAssembly != null)).ToString());
#endif
                return alienRaceEnabled;
            }
        }

        public static List<ThingDef> AlienRaces
        {
            get
            {
#if ALIEN_DEBUG && DEBUG
                Log.Warning("Begin AlienRaces GET");
#endif
                if (Exists && alienRaces.Count == 0)
                {
                    foreach(ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (def.GetType().GetField("alienRace") != null)
                        {
                            for (Type type = def.GetType(); type != null; type = type.BaseType)
                            {
#if ALIEN_DEBUG && DEBUG
                                Log.Message("    Alien Race Found: " + def.label);
#endif
                                if (type.Name.EqualsIgnoreCase("ThingDef_AlienRace"))
                                {
                                    alienRaces.Add(def);
                                    break;
                                }
                            }
                        }
                    }
                }
#if ALIEN_DEBUG && DEBUG
                Log.Warning("End AlienRaces GET -- " + ((alienRaces == null) ? "<null>" : alienRaces.Count.ToString()));
#endif
                return alienRaces;
            }
        }

        public static bool IsAlien(Pawn pawn)
        {
            return AlienRaces.Contains(pawn.def);
        }
    }
}
