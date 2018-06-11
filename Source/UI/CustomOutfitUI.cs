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
using ChangeDresser.UI.Util;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;

namespace ChangeDresser.UI
{
    public class CustomOutfitUI : Window
    {
        //public enum ApparelFromEnum { Pawn, Storage };
        private readonly Building_Dresser Dresser;
        private Pawn pawn = null;
        private PawnOutfitTracker outfitTracker = null;
        private CustomOutfit customOutfit;
        private string searchText = "";

        private Vector2 scrollPosLeft = new Vector2(0, 0);
        private Vector2 scrollPosRight = new Vector2(0, 0);
        private List<Pawn> selectablePawns = new List<Pawn>();
        private List<Outfit> selectableOutfits = new List<Outfit>();
        private List<Apparel> availableApparel = new List<Apparel>();

        private const int HEIGHT = 35;
        private const int X_BUFFER = 10;
        private const int Y_BUFFER = 5;
        private const float CELL_HEIGHT = 40f;

        public CustomOutfitUI(Building_Dresser dresser)
        {
#if CUSTOM_OUTFIT_UI
            Log.Warning("Begin CustomOutfitUI.CustomOutfitUI(Dresser: " + dresser.Label + ")");
#endif
            this.Dresser = dresser;

            this.closeOnEscapeKey = false;
            this.doCloseButton = false;
            this.doCloseX = false;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            
#if CUSTOM_OUTFIT_UI
            Log.Message("    Populate Selectable Pawns:");
#endif
            foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Colonists)
            {
#if CUSTOM_OUTFIT_UI
                Log.Message("        " + p.Name.ToStringShort + " " + p.Faction + " " + p.def.defName);
#endif
                if (p.Faction == Faction.OfPlayer && p.def.race.Humanlike)
                {
#if CUSTOM_OUTFIT_UI
                    Log.Message("            -- Added");
#endif
                    selectablePawns.Add(p);
                }
            }

            this.UpdateAvailableApparel();

#if CUSTOM_OUTFIT_UI
            Log.Warning("End CustomOutfitUI.CustomOutfitUI");
#endif
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(750f, 600f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            try
            {
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                Widgets.Label(new Rect(0, 0, 200, 50), "ChangeDresser.CustomOutfits".Translate());
                int y = HEIGHT + Y_BUFFER;
                int x = 0;
                
                x = this.DrawPawnSelection(x, y);
                x = this.DrawOutfitSelection(x, y);
                // New Outfit button
                if (this.outfitTracker != null &&
                    Widgets.ButtonText(new Rect(x, y, 75, 30), "ChangeDresser.New".Translate()))
                {
                    this.customOutfit = new CustomOutfit();
                    this.customOutfit.Name = "";
                    this.UpdateAvailableApparel();
                }

                y += HEIGHT + Y_BUFFER;

                if (this.customOutfit != null)
                {
                    Widgets.Label(new Rect(0, y, 60, 30), "ChangeDresser.OutfitName".Translate() + ":");
                    this.customOutfit.Name = Widgets.TextField(new Rect(70, y, 100, 30), this.customOutfit.Name);
                    
                    x = this.DrawUseInBattle(200, y);
                    x = this.DrawBaseOutfit(x, y);

                    y += HEIGHT + Y_BUFFER;
                }
                
                float height = inRect.height - y;
                height -= HEIGHT * 3;
                height -= Y_BUFFER * 2;

                this.DrawAvailableApparel(0, y, inRect.width * 0.5f, height);
                this.DrawOutfitApparel(inRect.width * 0.5f, y, inRect.width * 0.5f, height);
                y += (int)height;

                this.DrawBottomButtons(x, (int)inRect.yMax - 40, inRect.width);
            }
            catch (Exception e)
            {
                Log.Error(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message);
                Messages.Message(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message, MessageTypeDefOf.NegativeEvent);

                base.Close();
            }
        }

        public override void PreClose()
        {
            base.PreClose();

            if (this.pawn != null && this.outfitTracker != null && this.customOutfit != null)
            {
                this.outfitTracker.UpdateCustomApparel(this.Dresser);
            }
        }

        private void DrawCloseButton(int x, int y)
        {
            if (Widgets.ButtonText(new Rect(x, y, 100, 30), "ChangeDresser.Close".Translate()))
            {
                base.Close();
            }
        }

        private void DrawBottomButtons(int x, int y, float width)
        {
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("Begin CustomOutfitUI.DrawBottomButtons " + x + " " + y);
#endif
            float middle = width / 2f;
            if (this.pawn == null || this.outfitTracker == null || this.customOutfit == null)
            {
                this.DrawCloseButton((int)middle - 50, y);
#if TRACE && CUSTOM_OUTFIT_UI
                Log.Warning("End CustomOutfitUI.DrawBottomButtons -- Close Button Only");
#endif
                return;
            }

            float halfMiddle = middle / 2f;
            // Delete
            if (Widgets.ButtonText(new Rect(halfMiddle - 50, y, 100, 30), "Delete".Translate()))
            {
                if (this.outfitTracker.Remove(this.customOutfit))
                    this.outfitTracker.UpdateCustomApparel(this.Dresser);
                this.customOutfit = null;
            }

            // Save
            if (Widgets.ButtonText(new Rect(middle - 50, y, 100, 30), "Save".Translate()))
            {
                this.outfitTracker.AddOutfit(this.customOutfit);
                this.outfitTracker.UpdateCustomApparel(this.Dresser);
                foreach(Apparel a in this.customOutfit.Apparel)
                {
                    if (!this.Dresser.RemoveNoDrop(a))
                    {
#if CUSTOM_OUTFIT_UI
                        Log.Warning("CustomOutfitUI.DrawBottomButtons -- Save failed to removed [" + a.Label + "] from dresser");
#endif
                    }
#if CUSTOM_OUTFIT_UI
                    else
                    {
                        Log.Warning("CustomOutfitUI.DrawBottomButtons -- Save removed [" + a.Label + "] from dresser");
                    }
#endif
                }
                this.customOutfit = null;
            }

            // Cancel
            if (Widgets.ButtonText(new Rect(middle + halfMiddle - 50, y, 100, 30), "ChangeDresser.Cancel".Translate()))
            {
                this.outfitTracker.UpdateCustomApparel(this.Dresser);
                this.customOutfit = null;
            }
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("End CustomOutfitUI.DrawBottomButtons " + x + " " + y);
#endif
        }

        private void DrawAvailableApparel(float x, float y, float width, float height)
        {
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("Begin CustomOutfitUI.DrawAvailableApparel " + x + " " + y);
#endif
            // Apparel Selection Titles
            Widgets.Label(new Rect(x, y, 150, 30), "ChangeDresser.AvailableApparel".Translate());
            searchText = Widgets.TextArea(new Rect(x + 160, y, 100, 30), searchText).ToLower();
            y += HEIGHT + Y_BUFFER;
            
            Rect apparelListRect = new Rect(x, y, width - 10, height);
            Rect apparelScrollRect = new Rect(0f, 0f, apparelListRect.width - 16f, this.availableApparel.Count * CELL_HEIGHT);

            GUI.BeginGroup(apparelListRect);
            this.scrollPosLeft = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosLeft, apparelScrollRect);
            
            for (int i = 0, count = 0; i < this.availableApparel.Count; ++i)
            {
                Apparel apparel = this.availableApparel[i];
                if (searchText.Trim().Length == 0 || 
                    apparel.Label.ToLower().Contains(searchText))
                {
                    Rect rowRect = new Rect(0, 2f + count * CELL_HEIGHT, apparelListRect.width, CELL_HEIGHT);
                    ++count;
                    GUI.BeginGroup(rowRect);

                    Widgets.ThingIcon(new Rect(0f, 0f, CELL_HEIGHT, CELL_HEIGHT), apparel);

                    Widgets.Label(new Rect(CELL_HEIGHT + 5f, 0f, rowRect.width - 40f - CELL_HEIGHT, CELL_HEIGHT), apparel.Label);

                    if (this.customOutfit != null)
                    {
                        Rect buttonRect = new Rect(rowRect.width - 35f, 10, 20, 20);
                        if (this.CanWear(apparel))
                        {
                            if (Widgets.ButtonImage(buttonRect, WidgetUtil.nextTexture))
                            {
                                this.AddApparelToOutfit(apparel);
                                break;
                            }
                        }
                        else
                        {
                            Widgets.ButtonImage(buttonRect, WidgetUtil.cantTexture);
                        }
                    }
                    GUI.EndGroup();
                }
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("End CustomOutfitUI.DrawAvailableApparel " + x + " " + y);
#endif
        }

        private void DrawOutfitApparel(float x, int y, float width, float height)
        {
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("Begin CustomOutfitUI.DrawOutfitApparel " + x + " " + y);
#endif
            if (pawn != null && this.outfitTracker != null && this.customOutfit != null)
            {
                Widgets.Label(new Rect(x, y, 150, 30), "ChangeDresser.CustomOutfitApparel".Translate());
                y += HEIGHT + Y_BUFFER;

                Rect apparelListRect = new Rect(x, y, width - 10, height);
                Rect apparelScrollRect = new Rect(0f, 0f, apparelListRect.width - 16f, this.customOutfit.Apparel.Count * CELL_HEIGHT);

                GUI.BeginGroup(apparelListRect);
                this.scrollPosRight = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosRight, apparelScrollRect);

                for (int i = 0; i < this.customOutfit.Apparel.Count; ++i)
                {
                    Apparel apparel = this.customOutfit.Apparel[i];
                    Rect rowRect = new Rect(0, 2f + i * CELL_HEIGHT, apparelListRect.width, CELL_HEIGHT);
                    GUI.BeginGroup(rowRect);

                    if (Widgets.ButtonImage(new Rect(5, 10, 20, 20), WidgetUtil.previousTexture))
                    {
                        this.RemoveApparelFromOutfit(apparel);
                        break;
                    }

                    Widgets.ThingIcon(new Rect(35f, 0f, CELL_HEIGHT, CELL_HEIGHT), apparel);
                    Widgets.Label(new Rect(CELL_HEIGHT + 45f, 0f, rowRect.width - CELL_HEIGHT - 45f, CELL_HEIGHT), apparel.Label);
                    this.UpdateAvailableApparel();

                    GUI.EndGroup();
                }
                Widgets.EndScrollView();
                GUI.EndGroup();
            }
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("End CustomOutfitUI.DrawOutfitApparel " + x + " " + y);
#endif
        }

        private int DrawBaseOutfit(int x, int y)
        {
            const string baseLabel = "Outfit (None)";
            if (this.pawn != null && this.outfitTracker != null && this.customOutfit != null)
            {
                string label = (this.customOutfit.Outfit == null) ? baseLabel : this.customOutfit.Outfit.label;
                if (Widgets.ButtonText(new Rect(x, y, 100, 30), label))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    options.Add(new FloatMenuOption(baseLabel, delegate
                    {
                        this.customOutfit.Outfit = null;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    foreach (Outfit o in Current.Game.outfitDatabase.AllOutfits)
                    {
                        options.Add(new FloatMenuOption(o.label, delegate
                        {
                            this.customOutfit.Outfit = o;
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                return x + 110;
            }
            return x;
        }

        private int DrawPawnSelection(int x, int y)
        {
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("Begin CustomOutfitUI.DrawPawnSelection " + x + " " + y);
#endif
            string label = (this.pawn != null) ? this.pawn.Name.ToStringShort : "Select Pawn";
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Message("    Label: " + label);
#endif
            if (Widgets.ButtonText(new Rect(x, y, 100, 30), label))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
#if TRACE && CUSTOM_OUTFIT_UI
                Log.Message("    selectablePawns Count: " + this.selectablePawns.Count);
#endif
                foreach (Pawn p in this.selectablePawns)
                {
#if TRACE && CUSTOM_OUTFIT_UI
                    Log.Message("        " + p.Name.ToStringShort);
#endif
                    options.Add(new FloatMenuOption(p.Name.ToStringShort, delegate
                    {
#if CUSTOM_OUTFIT_UI
                        Log.Warning("Begin CustomOutfitUI.DrawAvailableApparel.Delegate " + p.Name.ToStringShort);
#endif
                        if (this.customOutfit != null)
                        {
                            this.outfitTracker.UpdateCustomApparel(this.Dresser);
                            this.customOutfit = null;
                        }

                        this.pawn = p;
                        if (!WorldComp.PawnOutfits.TryGetValue(this.pawn, out outfitTracker))
                        {
                            outfitTracker = new PawnOutfitTracker(this.pawn);
                            WorldComp.PawnOutfits.Add(this.pawn, outfitTracker);
                        }
#if CUSTOM_OUTFIT_UI
                        Log.Warning("End CustomOutfitUI.DrawAvailableApparel.Delegate");
#endif
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("End CustomOutfitUI.DrawPawnSelection");
#endif
            return x + 110;
        }

        private int DrawOutfitSelection(int x, int y)
        {
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("Begin CustomOutfitUI.DrawOutfitSelection " + x + " " + y);
#endif
            if (this.outfitTracker != null && this.outfitTracker.CustomOutfits.Count > 0)
            {
                string label = (this.customOutfit != null) ? this.customOutfit.Label : "Select Outfit";
                if (Widgets.ButtonText(new Rect(x, y, 150, 30), label))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (CustomOutfit o in this.outfitTracker.CustomOutfits)
                    {
                        options.Add(new FloatMenuOption(o.Label, delegate
                        {
#if CUSTOM_OUTFIT_UI
                            Log.Warning("Begin CustomOutfitUI.DrawOutfitSelection.Delegate " + o.Label);
#endif
                            if (this.customOutfit != null)
                            {
                                this.outfitTracker.UpdateCustomApparel(this.Dresser);
                            }
                            this.customOutfit = o;
#if CUSTOM_OUTFIT_UI
                            Log.Warning("End CustomOutfitUI.DrawOutfitSelection.Delegate");
#endif
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                return x + 160;
            }
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("End CustomOutfitUI.DrawOutfitSelection");
#endif
            return x;
        }

        private int DrawUseInBattle(int x, int y)
        {
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("Begin CustomOutfitUI.DrawUseInBattle " + x + " " + y);
#endif
            if (this.pawn != null && this.outfitTracker != null && this.customOutfit != null)
            {
                Widgets.Label(new Rect(x, y, 100, 30), "ChangeDresser.UseForBattle".Translate());
                bool useInBattle = this.customOutfit.OutfitType == OutfitType.Battle;
                bool temp = useInBattle;
                Widgets.Checkbox(x + 110, y, ref useInBattle);
                if (useInBattle != temp)
                {
#if CUSTOM_OUTFIT_UI
                    Log.Warning("CustomOutfitUI.DrawUseInBattle <Changed> from " + temp + " to " + useInBattle);
#endif
                    this.customOutfit.OutfitType = (useInBattle) ? OutfitType.Battle : OutfitType.Civilian;
                }
                return x + 150;
            }
#if TRACE && CUSTOM_OUTFIT_UI
            Log.Warning("End CustomOutfitUI.DrawUseInBattle " + x + " " + y);
#endif
            return x;
        }

        public void UpdateAvailableApparel()
        {
            this.availableApparel.Clear();
            if (this.pawn != null)
            {
                this.availableApparel.AddRange(this.pawn.apparel.WornApparel);
            }
            if (this.outfitTracker != null)
            {
                foreach (Apparel a in this.outfitTracker.CustomApparel)
                {
                    if (!this.availableApparel.Contains(a))
                    {
                        this.availableApparel.Add(a);
                    }
                }
            }
            this.availableApparel.AddRange(this.Dresser.Apparel);
        }

        private void AddApparelToOutfit(Apparel apparel)
        {
            if (this.availableApparel.Remove(apparel))
                this.customOutfit.Apparel.Add(apparel);
        }

        private void RemoveApparelFromOutfit(Apparel apparel)
        {
            if (this.customOutfit.Apparel.Remove(apparel))
                this.availableApparel.Add(apparel);
        }

        private bool CanWear(Apparel apparel)
        {
            foreach (Apparel a in this.customOutfit.Apparel)
            {
                if (!ApparelUtility.CanWearTogether(a.def, apparel.def, this.pawn.RaceProps.body))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
