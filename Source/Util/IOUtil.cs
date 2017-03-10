using ChangeDresser.UI.DTO;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ChangeDresser.Util
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
                if (TryGetFileName(type, out fileName))
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
                Messages.Message("Problem while loading Color Presets for " + type + ".", MessageSound.Negative);
                Log.Error(e.GetType() + " " + e.Message);
            }
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
            string fileName;
            TryGetFileName(type, out fileName);

            // Write Data
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine("Version 1");
                for (int i = 0; i < presetsDto.Count; ++i)
                {
                    Color c = presetsDto[i];
                    sw.WriteLine(c.r + ":" + c.g + ":" + c.b);
                }
            }
        }

        private static bool TryGetFileName(ColorPresetType type, out string fileName)
        {
            if (TryGetDirectoryPath(type, out fileName))
            {
                fileName = Path.Combine(fileName, type.ToString() + ".xml");
                if (!File.Exists(fileName))
                {
                    File.Create(fileName);
                    return false;
                }
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
