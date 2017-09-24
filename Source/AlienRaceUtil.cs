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
