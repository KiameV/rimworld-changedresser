using ChangeDresser.UI.DTO;
using RimWorld;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ChangeDresser
{
    public enum ColorPresetType { Apparel, Hair };
    static class IOUtil
    {
        public static ColorPresetsDTO LoadColorPresets(ColorPresetType type)
        {
            ColorPresetsDTO presetsDto = CreateDefaultColorPresets();
            try
            { 
                string fileName;
                if (TryGetFileName(type, out fileName) &&
                    File.Exists(fileName))
                {
                    // Load Data
                    using (StreamReader sr = new StreamReader(fileName))
                    {
                        string version = sr.ReadLine();
                        if (version.Equals("Version 1"))
                        {
                            for (int i = 0; i < presetsDto.Count; ++i)
                            {
                                try
                                {
                                    string[] s = sr.ReadLine().Split(new Char[] { ':' });
                                    Color c = Color.white;
                                    c.r = float.Parse(s[0]);
                                    c.g = float.Parse(s[1]);
                                    c.b = float.Parse(s[2]);
                                    presetsDto[i] = c;
                                }
                                catch
                                {
                                    presetsDto[i] = Color.white;
                                }
                            }
                        }
                        else
                        {
                            presetsDto = new ColorPresetsDTO();
                            {
                                for (int i = 0; i < presetsDto.Count; ++i)
                                    presetsDto[i] = Color.white;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Messages.Message("ChangeDresser.ProblemLoadingPreset".Translate() + " " + type + ".", MessageTypeDefOf.SilentInput);
                Log.Warning(e.GetType() + " " + e.Message);
                presetsDto = CreateDefaultColorPresets();
            }
            presetsDto.IsModified = false;
            return presetsDto;
        }

        private static ColorPresetsDTO CreateDefaultColorPresets()
        {
            ColorPresetsDTO presetsDto = new ColorPresetsDTO();
            for (int i = 0; i < presetsDto.Count; ++i)
                presetsDto[i] = Color.white;
            return presetsDto;
        }

        public static void SaveColorPresets(ColorPresetType type, ColorPresetsDTO presetsDto)
        {
            try
            {
                string fileName;
                if (!TryGetFileName(type, out fileName))
                {
                    throw new Exception("Unable to get file name.");
                }

                // Write Data
                using (FileStream fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fileStream))
                    {
                        sw.WriteLine("Version 1");
                        for (int i = 0; i < presetsDto.Count; ++i)
                        {
                            Color c = presetsDto[i];
                            sw.WriteLine(c.r + ":" + c.g + ":" + c.b);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Messages.Message("ChangeDresser.ProblemSavingPreset".Translate() + " " + type + ".", MessageTypeDefOf.SilentInput);
                Log.Error(e.GetType() + " " + e.Message);
            }
        }

        private static bool TryGetFileName(ColorPresetType type, out string fileName)
        {
            if (TryGetDirectoryPath(type, out fileName))
            {
                fileName = Path.Combine(fileName, type.ToString() + ".xml");
                return true;
            }
            return false;
        }

        private static bool TryGetDirectoryPath(ColorPresetType type, out string path)
        {
            if (TryGetDirectoryName(out path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                return true;
            }
            return false;
        }

        private static bool TryGetDirectoryName(out string path)
        {
            try
            {
                path = (string)typeof(GenFilePaths).GetMethod("FolderUnderSaveData", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[]
                {
                    "ChangeDresser"
                });
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("ChangeDresser: Failed to get folder name - " + ex);
                path = null;
                return false;
            }
        }
    }
}
