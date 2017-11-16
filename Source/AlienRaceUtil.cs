using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace ChangeDresser
{
    class AlienRaceUtil
    {
        private static Assembly alienRaceAssembly = null;
        private static bool initialized = false;
        private static List<ThingDef> alienRaces = new List<ThingDef>(0);

        #region Get Field/Metho Info
        private static FieldInfo raceSettingsFieldInfo = null;
        private static FieldInfo hairSettingsFieldInfo = null;
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
            return raceSettingsFieldInfo?.GetValue(pawn.def);
        }

        public static object GetHairSettings(Pawn pawn)
        {
            object raceSettings = GetAlienRaceSettings(pawn);
            if (hairSettingsFieldInfo == null)
            {
                hairSettingsFieldInfo = raceSettings?.GetType().GetField("hairSettings");
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
                Log.Warning("hairSettingsFieldInfo found: " + (string)((hairSettingsFieldInfo != null) ? "True" : "False"));
#endif
            }
            return hairSettingsFieldInfo.GetValue(raceSettings);
        }

        public static bool HasHair(Pawn pawn)
        {
            object hairSettings = GetHairSettings(pawn);
            return (bool)hairSettings?.GetType().GetField("hasHair")?.GetValue(hairSettings);
        }

        public static object GetGeneralSettings(Pawn pawn)
        {
            object raceSettings = GetAlienRaceSettings(pawn);
            if (generalSettingsFieldInfo == null)
            {
                object generalSettingsFieldInfo = raceSettings.GetType().GetField("generalSettings");
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
                Log.Warning("generalSettingsFieldInfo found: " + (string)((generalSettingsFieldInfo != null) ? "True" : "False"));
#endif
            }
            return generalSettingsFieldInfo?.GetValue(raceSettings);
        }

        public static List<string> GetHairTags(Pawn pawn)
        {
            object hairSettings = GetHairSettings(pawn);
            object hairTags = hairSettings.GetType().GetField("hairTags")?.GetValue(hairSettings);
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
            Log.Warning("hairTags found: " + (string)((hairTags != null) ? "True" : "False"));
#endif
            return (List<string>)hairTags;
        }

        private static FieldInfo GetMaleGenderProbabilityFieldInfo(Pawn pawn)
        {
            FieldInfo fi = GetGeneralSettings(pawn)?.GetType().GetField("maleGenderProbability");
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
            Log.Warning("maleGenderProbability found: " + (string)((fi != null) ? "True" : "False"));
#endif
            return fi;
        }

        public static bool HasMaleGenderProbability(Pawn pawn)
        {
            return GetMaleGenderProbabilityFieldInfo(pawn) != null;
        }

        public static float GetMaleGenderProbability(Pawn pawn)
        {
            return (float)GetMaleGenderProbabilityFieldInfo(pawn)?.GetValue(GetGeneralSettings(pawn));
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
                                alienRaceAssembly = assembly;
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
                return alienRaceAssembly != null;
            }
        }

        public static List<ThingDef> AlienRaces
        {
            get
            {
                if (Exists && alienRaces.Count == 0)
                {
                    foreach(ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (def.defName.StartsWith("Alien") || 
                            def.defName.StartsWith("alien"))
                        {
#if ALIEN_DEBUG && DEBUG
                            Log.Warning("Def: " + def.defName);
#endif
                            for (Type type = def.GetType(); type != null; type = type.BaseType)
                            {
#if ALIEN_DEBUG && DEBUG
                                Log.Warning(" Type: " + type.Name);
#endif
                                if (type.Name.EqualsIgnoreCase("ThingDef_AlienRace"))
                                {
#if ALIEN_DEBUG && DEBUG
                                    Log.Warning("  Added");
#endif
                                    alienRaces.Add(def);
                                    break;
                                }
                            }
                        }
                    }
                }
                return alienRaces;
            }
        }

        public static bool IsAlien(Pawn pawn)
        {
            return AlienRaces.Contains(pawn.def);
        }
    }
}
