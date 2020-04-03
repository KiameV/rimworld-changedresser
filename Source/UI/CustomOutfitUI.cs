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
        //private string searchText = "";

        private Vector2 scrollPosLeft = new Vector2(0, 0);
        private Vector2 scrollPosRight = new Vector2(0, 0);
        private List<Pawn> selectablePawns = new List<Pawn>();
        private List<Outfit> selectableOutfits = new List<Outfit>();
        private List<Apparel> availableApparel = new List<Apparel>();

        private const int HEIGHT = 35;
        private const int X_BUFFER = 10;
        private const int Y_BUFFER = 5;
        private const float CELL_HEIGHT = 40f;

        private CDApparelFilters apparelFilter = new CDApparelFilters();
        private Vector2 filterScrollPosition = new Vector2(0, 0);

        public CustomOutfitUI(Building_Dresser dresser)
        {
#if CUSTOM_OUTFIT_UI
            Log.Warning("Begin CustomOutfitUI.CustomOutfitUI(Dresser: " + dresser.Label + ")");
#endif
            this.Dresser = dresser;

            this.closeOnClickedOutside = false;
            this.doCloseButton = false;
            this.doCloseX = false;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;

			WorldComp.CleanupCustomOutfits();
            
#if CUSTOM_OUTFIT_UI
            Log.Message("    Populate Selectable Pawns:");
#endif
            foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
            {
#if CUSTOM_OUTFIT_UI
                Log.Message("        " + p.Name.ToStringShort + " " + p.Faction + " " + p.def.defName);
#endif
                if (p.Faction == Faction.OfPlayer && p.def.race.Humanlike && p.apparel?.LockedApparel?.Count == 0)
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
                return new Vector2(1100f, 600f);
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

                this.DrawAvailableApparel(0, y, 350, height);
                this.DrawOutfitApparel(360, y, 710, height);
                y += (int)height;

                this.DrawBottomButtons(x, (int)inRect.yMax - 40, inRect.width);

                // Filter start
                y = 50;
                
                Widgets.Label(new Rect(750, y, 100, 32), "Filter".Translate());
                y += 40;

                Widgets.Label(new Rect(775, y, 75, 32), "ChangeDresser.Name".Translate());
                this.apparelFilter.Name = Widgets.TextField(new Rect(855, y - 3, 120, 32), this.apparelFilter.Name);
                y += 35;

                if (Widgets.ButtonText(new Rect(775, y, 200, 32), this.apparelFilter.LayerString))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    options.Add(new FloatMenuOption(this.apparelFilter.GetLayerLabel("ChangeDresser.All".Translate()), delegate
                    {
                        this.apparelFilter.Layer = null;
                        this.apparelFilter.LayerString = this.apparelFilter.GetLayerLabel("ChangeDresser.All".Translate());
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    foreach (ApparelLayerDef d in DefDatabase<ApparelLayerDef>.AllDefs)
                    {
                        options.Add(new FloatMenuOption(d.defName.Translate(), delegate
                        {
                            this.apparelFilter.Layer = d;
                            this.apparelFilter.LayerString = this.apparelFilter.GetLayerLabel(d.defName.Translate());
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                y += 35;

                if (Widgets.ButtonText(new Rect(775, y, 200, 32), this.apparelFilter.QualityString))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    options.Add(new FloatMenuOption(this.apparelFilter.GetQualityLabel("ChangeDresser.All".Translate()), delegate
                    {
                        this.apparelFilter.Quality = QualityRange.All;
                        this.apparelFilter.QualityString = this.apparelFilter.GetQualityLabel("ChangeDresser.All".Translate());
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    foreach (QualityCategory q in Enum.GetValues(typeof(QualityCategory)))
                    {
                        options.Add(new FloatMenuOption(q.ToString(), delegate
                        {
                            this.apparelFilter.Quality.min = q;
                            this.apparelFilter.QualityString = this.apparelFilter.GetQualityLabel(q.GetLabel());
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                y += 35;

                Listing_Standard ls = new Listing_Standard();
                ls.Begin(new Rect(775, y, 216, inRect.height - y ));
                Rect view = new Rect(0, 0, 200, 64 * this.apparelFilter.StatDefs.Count);
                ls.BeginScrollView(
                    new Rect(0, 0, 216, inRect.height - y ), ref filterScrollPosition, ref view);
                
                ls.Label("HitPointsBasic".ToString() + " " + (int)this.apparelFilter.HP);
                this.apparelFilter.HP = ls.Slider(this.apparelFilter.HP, 0, 100);

                for (int i = 0; i < this.apparelFilter.StatDefs.Count; ++i)
                {
                    StatDefValue sdv = this.apparelFilter.StatDefs[i];
                    ls.Label(sdv.Def.label + " " + sdv.Value.ToString("n2"), 28);
                    float f = sdv.Value;
                    f = ls.Slider(f, 0, sdv.Max);
                    if (sdv.Value != f)
                    {
                        sdv.Value = f;
                        this.apparelFilter.StatDefs[i] = sdv;
                    }
                }

                ls.EndScrollView(ref view);
                ls.End();
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
            /*Widgets.Label(new Rect(x, y, 150, 30), "ChangeDresser.AvailableApparel".Translate());
            searchText = Widgets.TextArea(new Rect(x + 160, y, 100, 30), searchText).ToLower();
            y += HEIGHT + Y_BUFFER;*/
            
            Rect apparelListRect = new Rect(x, y, width - 10, height);
            Rect apparelScrollRect = new Rect(0f, 0f, apparelListRect.width - 16f, this.availableApparel.Count * CELL_HEIGHT);

            GUI.BeginGroup(apparelListRect);
            this.scrollPosLeft = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosLeft, apparelScrollRect);
            
            for (int i = 0, count = 0; i < this.availableApparel.Count; ++i)
            {
                Apparel apparel = this.availableApparel[i];
                //if (searchText.Trim().Length == 0 || 
                //    apparel.Label.ToLower().Contains(searchText))
                if (this.apparelFilter.IncludeAppareL(apparel))
                {
                    Rect rowRect = new Rect(0, 2f + count * CELL_HEIGHT, apparelListRect.width, CELL_HEIGHT);
                    ++count;
                    GUI.BeginGroup(rowRect);

                    Widgets.ThingIcon(new Rect(0f, 0f, CELL_HEIGHT, CELL_HEIGHT), apparel);

                    if (Widgets.InfoCardButton(40, 0, apparel))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(apparel));
                    }

                    Widgets.Label(new Rect(30 + CELL_HEIGHT + 5f, 0f, rowRect.width - 40f - CELL_HEIGHT, CELL_HEIGHT), apparel.Label);

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
            GUI.EndScrollView();
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

                    if (Widgets.InfoCardButton(75, 0, apparel))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(apparel));
                    }

                    Widgets.Label(new Rect(30 + CELL_HEIGHT + 45f, 0f, rowRect.width - CELL_HEIGHT - 45f, CELL_HEIGHT), apparel.Label);
                    this.UpdateAvailableApparel();

                    GUI.EndGroup();
                }
                GUI.EndScrollView();
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

    public struct StatDefValue
    {
        public readonly StatDef Def;
        public float Value;
        public readonly float Max;
        public StatDefValue(StatDef def, float max, float value = 0)
        {
            this.Def = def;
            this.Max = max;
            this.Value = value;
        }
    }

    public class CDApparelFilters
    {
        public string Name = "";

        public ApparelLayerDef Layer = null;
        public string LayerString;

        public QualityRange Quality = QualityRange.All;
        public string QualityString;

        public float HP = 0;

        public readonly List<StatDefValue> StatDefs = new List<StatDefValue>();

        public CDApparelFilters()
        {
            this.LayerString = this.GetLayerLabel("ChangeDresser.All".Translate());
            this.QualityString = this.GetQualityLabel("ChangeDresser.All".Translate());

            this.StatDefs.Add(new StatDefValue(StatDefOf.ArmorRating_Blunt, 2));
            this.StatDefs.Add(new StatDefValue(StatDefOf.ArmorRating_Sharp, 2));
            this.StatDefs.Add(new StatDefValue(StatDefOf.ArmorRating_Heat, 2));

            this.StatDefs.Add(new StatDefValue(StatDefOf.Insulation_Cold, 20));
            this.StatDefs.Add(new StatDefValue(StatDefOf.Insulation_Heat, 20));

            /*
            this.StatDefs.Add(new StatDefValue(StatDefOf.CarryingCapacity, 20));
            this.StatDefs.Add(new StatDefValue(StatDefOf.GlobalLearningFactor, 1));

            this.StatDefs.Add(new StatDefValue(StatDefOf.MeleeDodgeChance, 1));
            this.StatDefs.Add(new StatDefValue(StatDefOf.MeleeHitChance, 1));

            this.StatDefs.Add(new StatDefValue(StatDefOf.MoveSpeed, 10));

            this.StatDefs.Add(new StatDefValue(StatDefOf.WorkSpeedGlobal, 1));

            this.StatDefs.Add(new StatDefValue(StatDefOf.AimingDelayFactor, 1));
            this.StatDefs.Add(new StatDefValue(StatDefOf.AnimalGatherSpeed, 1));
            this.StatDefs.Add(new StatDefValue(StatDefOf.AnimalGatherYield, 2));

            this.StatDefs.Add(new StatDefValue(StatDefOf.MiningSpeed, 1));
            this.StatDefs.Add(new StatDefValue(StatDefOf.MiningYield, 1));
            
            this.StatDefs.Add(new StatDefValue(StatDefOf.PlantWorkSpeed, 1));
            this.StatDefs.Add(new StatDefValue(StatDefOf.PlantHarvestYield, 1));

            this.StatDefs.Add(new StatDefValue(StatDefOf.PsychicSensitivity, 1));

            this.StatDefs.Add(new StatDefValue(StatDefOf.SocialImpact, 1));

            this.StatDefs.Add(new StatDefValue(StatDefOf.TameAnimalChance, 1));
            this.StatDefs.Add(new StatDefValue(StatDefOf.TrainAnimalChance, 1));*/
        }

        public bool IncludeAppareL(Apparel a)
        {
            if (this.Name.Length > 0)
            {
                if (a.Label.ToLower().IndexOf(this.Name) == -1 &&
                    a.def.defName.ToLower().IndexOf(this.Name) == -1)
                    return false;
            }

            if (this.Layer != null)
            {
                if (!a.def.apparel.layers.Contains(this.Layer))
                    return false;
            }

            if (this.Quality != QualityRange.All)
            {
                QualityCategory q;
                if (a.TryGetQuality(out q))
                {
                    if (this.Quality.min > q || this.Quality.max < q)
                        return false;
                }
            }

            if (this.HP > 0)
            {
                float percent = (float)a.HitPoints / a.MaxHitPoints;
                float filter = this.HP * 0.01f;
                if (percent < filter)
                    return false;
            }

            foreach(StatDefValue sdv in this.StatDefs)
            {
                if (sdv.Value > 0)
                {
                    float v = Math.Abs(a.GetStatValue(sdv.Def));
                    if (sdv.Value > v)
                        return false;
                }
            }

            return true;
        }

        public string GetLayerLabel(string layer)
        {
            return "Layer".Translate() + ": " + layer;
        }

        public string GetQualityLabel(string q)
        {
            return "Quality".Translate() + ": " + q;
        }
    }
}
