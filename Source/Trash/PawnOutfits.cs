using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ChangeDresser.Trash
{
    public class PawnOutfits : IExposable
    {
        public List<Outfit> Outfits = new List<Outfit>();
        public Pawn Pawn = null;

        private Outfit lastBattleOutfit = null;
        public Outfit LastBattleOutfit { set { this.lastBattleOutfit = value; } }

        private Outfit lastCivilianOutfit = null;
        public Outfit LastCivilianOutfit { set { this.lastCivilianOutfit = value; } }

        private List<bool> IsColorAssigned = null;
        private List<Color> ColorForLayer = null;

        public PawnOutfits()
        {
            this.InitializeIsColorAssigned();
        }

        private void InitializeIsColorAssigned()
        {
            if (!HaveColorsBeenAssigned)
            {
                int size = Enum.GetValues(typeof(ApparelLayer)).Length;
                if (this.IsColorAssigned == null || this.IsColorAssigned.Count == 0)
                {
                    this.IsColorAssigned = new List<bool>(size);
                    for (int i = 0; i < size; ++i)
                    {
                        this.IsColorAssigned.Add(false);
                    }
                }

                if (this.ColorForLayer == null || this.ColorForLayer.Count == 0)
                {
                    this.ColorForLayer = new List<Color>(size);
                    for (int i = 0; i < size; ++i)
                    {
                        this.ColorForLayer.Add(Color.white);
                    }
                }
            }
        }

        private bool HaveColorsBeenAssigned
        {
            get
            {
                if (this.IsColorAssigned == null || this.IsColorAssigned.Count == 0 || 
                    this.ColorForLayer == null || this.ColorForLayer.Count == 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool ColorApparel(Apparel apparel)
        {
#if DEBUG || DEBUG_APPAREL_COLOR
            Log.Warning("Start PawnOutfits.ColorApparel(Apparel: " + apparel.Label + ")");
            /*ApparelLayer debugLayer = apparel.def.apparel.LastLayer;
            Log.Message(Environment.NewLine + "Start PawnOutfits.TryGetColorFor Layer: " + debugLayer + " " + ((int)debugLayer).ToString());

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < this.IsColorAssigned.Count; ++i)
            {
                sb.Append(this.IsColorAssigned[i]);
                if (this.ColorForLayer != null)
                {
                    sb.Append(" ");
                    sb.Append(this.ColorForLayer[i].ToString());
                }
                sb.Append(" -- ");
            }
            Log.Warning(sb.ToString());*/
#endif
            if (!HaveColorsBeenAssigned)
            {
#if DEBUG || DEBUG_APPAREL_COLOR
                Log.Message("    No colors assigned yet");
#endif
                return true;
            }

            foreach (ApparelLayer layer in apparel.def.apparel.layers)
            {
#if DEBUG || DEBUG_APPAREL_COLOR
                Log.Message("    Layer: " + layer);
#endif
                if ((int)layer < this.IsColorAssigned.Count &&
                    this.IsColorAssigned[(int)layer] == true)
                {
                    apparel.SetColor(this.ColorForLayer[(int)layer]);
#if DEBUG || DEBUG_APPAREL_COLOR
                    Log.Message("        Set color to: " + this.ColorForLayer[(int)layer]);
#endif
                    return true;
                }
#if DEBUG || DEBUG_APPAREL_COLOR
                else
                {
                    Log.Message("        No color set for layer");
                }
#endif
            }

#if DEBUG || DEBUG_APPAREL_COLOR
            Log.Message("    No color match found");
#endif
            return false;
        }

        /*public void ColorApparel(Pawn pawn)
        {
            if (HaveColorsBeenAssigned)
            {
                foreach (Apparel a in pawn.apparel.WornApparel)
                {
                    this.ColorApparel(a);
                }
            }
        }*/

        public void SetColorFor(Apparel apparel, Color color)
        {
            if (apparel != null)
            {
#if DEBUG || DEBUG_APPAREL_COLOR
            Log.Warning("Start PawnOutfits.SetColorFor (Apparel: " + apparel.Label + " Color: " + color + ")");
#endif
                /*Log.Message("IsColorAssigned:");
                for(int i = 0; i < IsColorAssigned.Count; ++i)
                {
                    Log.Message("    i: " + i + " Value: " + IsColorAssigned[i]);
                }
                Log.Message("ColorForLayer:");
                for (int i = 0; i < ColorForLayer.Count; ++i)
                {
                    Log.Message("    i: " + i + " Value: " + ColorForLayer[i]);
                }*/


                this.InitializeIsColorAssigned();
                foreach (ApparelLayer layer in apparel.def.apparel.layers)
                {
#if DEBUG || DEBUG_APPAREL_COLOR
                    Log.Warning("    Setting layer " + layer);
#endif
                    this.IsColorAssigned[(int)layer] = true;
                    this.ColorForLayer[(int)layer] = color;
                }
            }
#if DEBUG || DEBUG_APPAREL_COLOR
            else
            {
                Log.Warning("PawnOutfits.SetColorFor [null] Apparel");
            }
#endif
        }

        public bool TryGetBattleOutfit(out Outfit outfit)
        {
            if (this.lastBattleOutfit != null)
            {
                if (WorldComp.OutfitsForBattle.Contains(this.lastBattleOutfit) &&
                    this.Outfits.Contains(this.lastBattleOutfit))
                {
                    outfit = this.lastBattleOutfit;
                    return true;
                }
                else
                {
                    this.lastBattleOutfit = null;
                }
            }

            foreach (Outfit o in this.Outfits)
            {
                if (WorldComp.OutfitsForBattle.Contains(o))
                {
                    outfit = o;
                    return true;
                }
            }
            outfit = null;
            return false;
        }

        public bool TryGetCivilianOutfit(out Outfit outfit)
        {
            if (this.lastCivilianOutfit != null)
            {
                if (!WorldComp.OutfitsForBattle.Contains(this.lastCivilianOutfit) &&
                    this.Outfits.Contains(this.lastCivilianOutfit))
                {
                    outfit = this.lastCivilianOutfit;
                    return true;
                }
                else
                {
                    this.lastCivilianOutfit = null;
                }
            }

            foreach (Outfit o in this.Outfits)
            {
                if (!WorldComp.OutfitsForBattle.Contains(o))
                {
                    outfit = o;
                    return true;
                }
            }
            outfit = null;
            return false;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref this.Pawn, "pawn");
            Scribe_Collections.Look(ref this.Outfits, "outfits", LookMode.Reference, new object[0]);
            Scribe_References.Look(ref this.lastBattleOutfit, "lastBattleOutfit");
            Scribe_References.Look(ref this.lastCivilianOutfit, "lastCivilianOutfit");
            Scribe_Collections.Look(ref this.IsColorAssigned, "isColorAssigned", LookMode.Value);
            Scribe_Collections.Look(ref this.ColorForLayer, "colorForLayer", LookMode.Value, new object[0]);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.Outfits == null)
                {
                    this.Outfits = new List<Outfit>(0);
                }

                if (this.IsColorAssigned == null)
                {
                    this.InitializeIsColorAssigned();
                    this.ColorForLayer = null;
                }
            }
        }
    }
}
