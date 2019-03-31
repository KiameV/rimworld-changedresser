using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ChangeDresser
{
    class GradientHairColorUtil
    {
        private static Type gradientHairType = null;
        private static bool initialized = false;

        public static bool IsGradientHairAvailable
        {
            get
            {
                if (!initialized)
                {
                    try
                    {
                        gradientHairType = GenTypes.GetTypeInAnyAssembly("GradientHair.PublicApi");
                    }
                    catch
                    {
                        gradientHairType = null;
                    }
                    finally
                    {
                        initialized = true;
                    }
                }
                return gradientHairType != null;
            }
        }

        public static bool GetGradientHair(Pawn pawn, out bool enabled, out Color color)
        {
            try
            {
                var p = new object[] { pawn, null, null };
                bool result = (bool)gradientHairType.GetMethod("GetGradientHair", (BindingFlags.Static | BindingFlags.Public))?.Invoke(null, p);
                enabled = (bool)p[1];
                color = (Color)p[2];
                return result;
            }
            catch (Exception)
            {
                enabled = false;
                color = Color.white;
                return false;
            }
        }

        public static void SetGradientHair(Pawn pawn, bool enabled, Color color)
        {
            try
            {
                gradientHairType.GetMethod("SetGradientHair", (BindingFlags.Static | BindingFlags.Public))?.Invoke(null, new object[] { pawn, enabled, color });
            }
            catch (Exception)
            {
            }
        }
    }
}