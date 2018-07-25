using ChangeDresser.UI.Util;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;

namespace ChangeDresser.UI
{
    class AssignOutfitUI : Window
    {
        private readonly Building_Dresser Dresser;
        private Vector2 scrollPosition = new Vector2(0, 0);

        public AssignOutfitUI(Building_Dresser dresser)
        {
            this.Dresser = dresser;

            this.closeOnClickedOutside = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.closeOnClickedOutside = false;
            
            foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
            {
                if (p.Faction == Faction.OfPlayer && p.def.race.Humanlike)
                {
                    if (!WorldComp.PawnOutfits.ContainsKey(p))
                    {
                        PawnOutfitTracker po = new PawnOutfitTracker();
                        po.Pawn = p;
                        Outfit currentOutfit = p.outfits.CurrentOutfit;
                        if (currentOutfit != null)
                        {
                            po.AddOutfit(new DefinedOutfit(currentOutfit, WorldComp.GetOutfitType(currentOutfit)));
                        }
                        WorldComp.PawnOutfits.Add(p, po);
                    }
                }
            }
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(650f, 600f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            try
            {
                const int NAME_WIDTH = 100;
                const int CHECKBOX_WIDTH = 100;
                const int HEIGHT = 35;
                const int X_BUFFER = 10;
                const int Y_BUFFER = 5;

                //bool useInApparelLookup = this.Dresser.UseInApparelLookup;
                //Widgets.CheckboxLabeled(new Rect(0, 0, 300, HEIGHT), "ChangeDresser.UseAsApparelSource".Translate(), ref useInApparelLookup);
                //this.Dresser.UseInApparelLookup = useInApparelLookup;

                if (Widgets.ButtonText(new Rect(/*450*/0, 0, 150, HEIGHT), "ChangeDresser.ManageOutfits".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_ManageOutfits(null/*Current.Game.outfitDatabase.DefaultOutfit*/));
                }

                List<Outfit> allOutfits = Current.Game.outfitDatabase.AllOutfits;
                int y = 50 + HEIGHT + Y_BUFFER;

                GUI.BeginScrollView(
                    new Rect(0, y, inRect.width - 32, HEIGHT * 2 + Y_BUFFER * 3),
                    this.scrollPosition,
                    new Rect(0, y,
                        NAME_WIDTH + X_BUFFER + ((CHECKBOX_WIDTH + X_BUFFER) * allOutfits.Count),
                        HEIGHT * 2 + Y_BUFFER * 3),
                    GUIStyle.none, GUIStyle.none);

                // Header - Lists the Outfit names
                int x = NAME_WIDTH + X_BUFFER;
                //y = 0;
                foreach (Outfit o in allOutfits)
                {
                    //Widgets.Label(new Rect(x, y, NAME_WIDTH, HEIGHT), "Pawns".Translate());
                    //x += CHECKBOX_WIDTH + X_BUFFER;
                    Widgets.Label(new Rect(x, y, CHECKBOX_WIDTH, HEIGHT), o.label);
                    x += CHECKBOX_WIDTH + X_BUFFER;
                }
                y += HEIGHT + Y_BUFFER;

                // Use For Battle row
                x = 0;
                Widgets.Label(new Rect(x, y, NAME_WIDTH, HEIGHT), "ChangeDresser.UseForBattle".Translate());
                x += NAME_WIDTH + X_BUFFER;
                foreach (Outfit o in allOutfits)
                {
                    bool use = WorldComp.OutfitsForBattle.Contains(o);
                    bool useNoChange = use;
                    Widgets.Checkbox(x + 10, y, ref use);
                    x += CHECKBOX_WIDTH + X_BUFFER;

                    if (use != useNoChange)
                    {
                        if (use)
                        {
                            WorldComp.OutfitsForBattle.Add(o);
                        }
                        else
                        {
                            bool removed = WorldComp.OutfitsForBattle.Remove(o);
                        }

                        foreach(PawnOutfitTracker po in WorldComp.PawnOutfits.Values)
                        {
                            po.UpdateOutfitType(o, (use) ? OutfitType.Battle : OutfitType.Civilian);
                        }
                    }
                }
                y += HEIGHT + Y_BUFFER * 2;
                Widgets.DrawLineHorizontal(NAME_WIDTH + X_BUFFER, y - 4, NAME_WIDTH + X_BUFFER + ((CHECKBOX_WIDTH + X_BUFFER) * allOutfits.Count));
                GUI.EndScrollView();

                // Pawn Names
                GUI.BeginScrollView(
                    new Rect(0, y, NAME_WIDTH, inRect.height - y - 82),
                    this.scrollPosition,
                    new Rect(0, y, NAME_WIDTH, (HEIGHT + Y_BUFFER) * WorldComp.PawnOutfits.Values.Count),
                    GUIStyle.none, GUIStyle.none);
                x = 0;
                int py = y;
                foreach (PawnOutfitTracker po in WorldComp.PawnOutfits.Values)
                {
                    Widgets.Label(new Rect(x, py, NAME_WIDTH, HEIGHT), po.Pawn.Name.ToStringShort);
                    py += HEIGHT + Y_BUFFER;
                }
                Widgets.DrawLineVertical(NAME_WIDTH + 2, y, (HEIGHT + Y_BUFFER) * WorldComp.PawnOutfits.Values.Count);
                GUI.EndScrollView();

                int mainScrollXMin = NAME_WIDTH + X_BUFFER + 4;
                this.scrollPosition = GUI.BeginScrollView(
                    new Rect(mainScrollXMin, y, inRect.width - mainScrollXMin, inRect.height - y - 50),
                    this.scrollPosition,
                    new Rect(0, y,
                        NAME_WIDTH + X_BUFFER + ((CHECKBOX_WIDTH + X_BUFFER) * allOutfits.Count) - mainScrollXMin,
                        (HEIGHT + Y_BUFFER) * WorldComp.PawnOutfits.Values.Count));

                // Table of pawns and assigned outfits
                foreach (PawnOutfitTracker po in WorldComp.PawnOutfits.Values)
                {
                    x = 0;
                    foreach (Outfit o in allOutfits)
                    {
                        bool assign = po.Contains(o);
                        bool assignNoChange = assign;
                        Widgets.Checkbox(x + 10, y, ref assign);
                        x += CHECKBOX_WIDTH + X_BUFFER;

                        if (assign != assignNoChange)
                        {
                            this.HandleOutfitAssign(assign, o, po);
                        }

                        /*
                        bool assign = WorldComp.OutfitsForBattle.Contains(o);
                        Widgets.Checkbox(x + 10, y, ref assign);
                        if (Widgets.ButtonInvisible(new Rect(x + 5, y + 5, CHECKBOX_WIDTH - 5, HEIGHT - 5)))
                        {
                            this.HandleOutfitAssign(!WorldComp.OutfitsForBattle.Contains(o), o, po);
                        }
                        */
                    }
                    y += HEIGHT + Y_BUFFER;
                }

                GUI.EndScrollView();
            }
            catch(Exception e)
            {
                Log.Error(e.GetType() + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private void HandleOutfitAssign(bool assign, Outfit outfit, PawnOutfitTracker po)
        {
            Pawn pawn = po.Pawn;
            if (assign)
            {
                po.DefinedOutfits.Add(new DefinedOutfit(outfit, WorldComp.GetOutfitType(outfit)));
            }
            else
            {
                po.Remove(outfit);
                if (pawn.outfits.CurrentOutfit.Equals(outfit))
                {
                    bool newOutfitFound;
                    if (pawn.Drafted)
                    {
                        newOutfitFound = !po.ChangeToBattleOutfit();
                    }
                    else
                    {
                        newOutfitFound = !po.ChangeToCivilianOutfit();
                    }

                    if (!newOutfitFound)
                    {
                        Messages.Message(
                                pawn.Name.ToStringShort + " will no longer wear " + outfit.label +
                                ". Could not find another Outfit for them to wear. Please fix this manually.", MessageTypeDefOf.CautionInput);
                    }
                    else
                    {
                        IDresserOutfit o = po.CurrentOutfit;
                        if (o != null)
                        {
                            Messages.Message(
                                    pawn.Name.ToStringShort + " will no longer wear " + outfit.label +
                                    " and will instead be assigned to wear " + o.Label, MessageTypeDefOf.CautionInput);
                        }
                        else
                        {
                            Messages.Message(
                                    pawn.Name.ToStringShort + " will no longer wear " + outfit.label +
                                    " but could not be assigned anything else to wear.", MessageTypeDefOf.CautionInput);
                        }
                    }
                }
            }
        }
    }
}
