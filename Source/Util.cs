using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    static class Util
    {
        private static readonly Dictionary<ApparelLayerDef, int> layerDictionary = new Dictionary<ApparelLayerDef, int>();

        private static void InitDic()
        {
            if (layerDictionary.Count == 0)
            {
                int i = 0;
                foreach (ApparelLayerDef layer in DefDatabase<ApparelLayerDef>.AllDefs)
                {
                    layerDictionary.Add(layer, i);
                    ++i;
                }
            }
        }

        public static int LayerCount
        {
            get
            {
                InitDic();
                return layerDictionary.Count;
            }
        }

        public static IEnumerable<ApparelLayerDef> Layers
        {
            get
            {
                InitDic();
                return layerDictionary.Keys;
            }
        }

        public static int ToInt(ApparelLayerDef layer)
        {
            InitDic();
            return layerDictionary[layer];
        }

        public static ApparelLayerDef ToLayer(int layer)
        {
            foreach(KeyValuePair<ApparelLayerDef, int> kv in layerDictionary)
            {
                if (kv.Value == layer)
                    return kv.Key;
            }
            Log.Warning("Unable to find layer matching " + layer + ". Using OnSkin.");
            return ApparelLayerDefOf.OnSkin;
        }
    }
}
