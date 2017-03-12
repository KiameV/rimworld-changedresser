/*
 * MIT License
 * 
 * Copyright (c) [2017] [Travis Offtermatt]
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
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
                Messages.Message("Problem while loading Color Presets for " + type + ".", MessageSound.Silent);
                Log.Warning(e.GetType() + " " + e.Message);
                return CreateDefaultColorPresets();
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
            try
            {
                string fileName;
                if (!TryGetFileName(type, out fileName))
                {
                    throw new Exception("Unable to get file name.");
                }

                // Write Data
                using (FileStream fileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write))
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
                Messages.Message("Problem while saving Color Presets for " + type + ".", MessageSound.Silent);
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
