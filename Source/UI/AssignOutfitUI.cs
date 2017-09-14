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
        private static ThingFilter ApparelGlobalFilter;

        private readonly Building_Dresser Dresser;
        private Vector2 scrollPosition = new Vector2(0, 0);

        public AssignOutfitUI(Building_Dresser dresser)
        {
            this.Dresser = dresser;

            this.closeOnEscapeKey = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.closeOnClickedOutside = false;
            
            foreach (Pawn p in PawnsFinder.AllMapsAndWorld_Alive)
            {
                if (p.Faction == Faction.OfPlayer && p.def.race.Humanlike)
                {
                    if (!WorldComp.PawnOutfits.ContainsKey(p))
                    {
                        PawnOutfits po = new PawnOutfits();
                        po.Pawn = p;
                        Outfit currentOutfit = p.outfits.CurrentOutfit;
                        if (currentOutfit != null)
                        {
                            po.Outfits.Add(currentOutfit);
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

                bool useInApparelLookup = this.Dresser.UseInApparelLookup;
                Widgets.CheckboxLabeled(new Rect(0, 0, 300, HEIGHT), "ChangeDresser.UseAsApparelSource".Translate(), ref useInApparelLookup);
                this.Dresser.UseInApparelLookup = useInApparelLookup;

                if (Widgets.ButtonText(new Rect(450, 0, 150, HEIGHT), "ChangeDresser.ManageOutfits".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_ManageOutfits(null/*Current.Game.outfitDatabase.DefaultOutfit*/));
                }

                List<Outfit> allOutfits = Current.Game.outfitDatabase.AllOutfits;
                int y = 50 + HEIGHT + Y_BUFFER;
                this.scrollPosition = GUI.BeginScrollView(
                    new Rect(0, y, inRect.width, inRect.height - y - 50),
                    this.scrollPosition,
                    new Rect(0, y, 
                        NAME_WIDTH + X_BUFFER + ((CHECKBOX_WIDTH + X_BUFFER) * allOutfits.Count), 
                        (HEIGHT + Y_BUFFER) * WorldComp.PawnOutfits.Count));
                
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
                            WorldComp.OutfitsForBattle.Remove(o);
                        }
                    }
                }
                y += HEIGHT + Y_BUFFER * 2;

                // Table of pawns and assigned outfits
                foreach (PawnOutfits po in WorldComp.PawnOutfits.Values)
                {
                    x = 0;
                    Widgets.Label(new Rect(x, y, NAME_WIDTH, HEIGHT), po.Pawn.NameStringShort);
                    x += NAME_WIDTH + X_BUFFER;

                    foreach (Outfit o in allOutfits)
                    {
                        bool assign = po.Outfits.Contains(o);
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

        private void HandleOutfitAssign(bool assign, Outfit previousOutfit, PawnOutfits po)
        {
            Pawn pawn = po.Pawn;
            if (assign)
            {
                po.Outfits.Add(previousOutfit);
            }
            else
            {
                if (pawn.outfits.CurrentOutfit.Equals(previousOutfit))
                {
                    po.Outfits.Remove(previousOutfit);

                    Outfit newOutfit = null;
                    if (pawn.Drafted)
                    {
                        if (!po.TryGetBattleOutfit(out newOutfit))
                        {
                            Messages.Message(
                                pawn.NameStringShort + " will no longer wear " +
                                previousOutfit.label + ". Could not find another Outfit for them to wear. Please fix this manually.", MessageSound.Standard);
                        }
                    }
                    else if (pawn.outfits.CurrentOutfit.Equals(previousOutfit))
                    {
                        if (!po.TryGetBattleOutfit(out newOutfit))
                        {
                            Messages.Message(
                                pawn.NameStringShort + " will no longer wear " + previousOutfit.label +
                                ". Could not find another Outfit for them to wear. Please fix this manually.", MessageSound.Standard);
                        }
                    }

                    if (newOutfit != null)
                    {
                        Messages.Message(
                                pawn.NameStringShort + " will no longer wear " + previousOutfit.label +
                                " and will instead be assigned to wear " + newOutfit.label, MessageSound.Standard);
                    }
                }
            }
        }
    }
}
