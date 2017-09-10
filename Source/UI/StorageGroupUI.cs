using ChangeDresser.UI.Util;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;
using ChangeDresser.StoredApparel;

namespace ChangeDresser.UI
{
    public class StorageGroupUI : Window
    {
        private readonly Building_Dresser Dresser;
        private Pawn SelectedPawn = null;
        private List<StoredApparelSet> ApparelSets = null;
        private StoredApparelSet SelectedApparelSet;
        private List<Apparel> SelectedApparel = null;

        private Vector2 scrollPosLeft = new Vector2(0, 0);
        private Vector2 scrollPosRight = new Vector2(0, 0);

        private List<Apparel> possibleApparel;
        private List<Apparel> PossibleApparel
        {
            get
            {
                if (this.SelectedApparelSet == null)
                {
                    return this.Dresser.StoredApparel;
                }
                return this.possibleApparel;
            }
        }

        public static void ClearPlayerPawns() { selectablePawns.Clear(); }
        private static List<Pawn> selectablePawns = new List<Pawn>();
        private static List<Pawn> PlayerPawns
        {
            get
            {
                if (selectablePawns.Count == 0)
                {
                    selectablePawns = new List<Pawn>();
                    foreach (Pawn p in PawnsFinder.AllMapsAndWorld_Alive)
                    {
                        if (p.Faction == Faction.OfPlayer && p.def.race.Humanlike)
                        {
                            selectablePawns.Add(p);
                        }
                    }
                }
                return selectablePawns;
            }
        }

        public StorageGroupUI(Building_Dresser dresser)
        {
            this.Dresser = dresser;
            
            this.closeOnEscapeKey = true;
            this.doCloseButton = false;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
        }

        public override void PostClose()
        {
            base.PostClose();
            ClearPlayerPawns();
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
                //Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                //Widgets.Label(new Rect(0, 0, 200, 50), "ChangeDresser.ApparelGroupLabel".Translate());
                Text.Font = GameFont.Small;
                string label = (this.SelectedPawn != null) ? this.SelectedPawn.NameStringShort : "ChangeDresser.SelectPawn".Translate();

                if (Widgets.ButtonText(new Rect(0, 0, 150, 30), label))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Pawn p in PlayerPawns)
                    {
                        options.Add(new FloatMenuOption(p.Name.ToStringShort, delegate
                        {
                            this.PawnSelected(p);
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }

                if (this.SelectedPawn != null)
                {
                    label = (this.SelectedApparelSet != null) ? this.SelectedApparelSet.Name : "ChangeDresser.ApparelGroupLabel".Translate();
                    if (Widgets.ButtonText(new Rect(175, 0, 150, 30), label))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        options.Add(new FloatMenuOption("ChangeDresser.CreateNew".Translate(), delegate
                        {
                            this.NewApparelSet();
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                        
                        foreach (StoredApparelSet s in this.ApparelSets)
                        {
                            options.Add(new FloatMenuOption(s.Name, delegate
                            {
                                this.ApparelSetSelected(s);
                            }, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }

                if (this.SelectedApparelSet != null)
                {
                    Rect rect = new Rect(0, 40, inRect.width, 30);
                    GUI.BeginGroup(rect);
                    GUI.Label(new Rect(0, 0, 100, rect.height), "ChangeDresser.ApparelGroupName".Translate() + ":", WidgetUtil.MiddleCenter);
                    this.SelectedApparelSet.Name = Widgets.TextField(new Rect(110, 0, 150, rect.height), this.SelectedApparelSet.Name);

                    GUI.Label(new Rect(290, 0, 100, rect.height), "ChangeDresser.ForCombat".Translate() + ":", WidgetUtil.MiddleCenter);
                    this.SelectedApparelSet.ForBattle = GUI.Toggle(new Rect(400, 7, rect.height, rect.height), this.SelectedApparelSet.ForBattle, "");
                    GUI.EndGroup();
                }
                
                const float cellHeight = 40f;
                float apparelListWidth = inRect.width * 0.5f - 10f;
                Rect apparelListRect;
                Rect apparelScrollRect;

                if (this.SelectedApparel != null)
                {
                    apparelListRect = new Rect(0, 110, apparelListWidth, inRect.height - 150);
                    apparelScrollRect = new Rect(0f, 0f, apparelListRect.width - 16f, this.SelectedApparel.Count * cellHeight);

                    GUI.Label(new Rect(0, 80, 150, 30), ("ChangeDresser.AssignedApparel").Translate(), WidgetUtil.MiddleCenter);

                    GUI.BeginGroup(apparelListRect);
                    this.scrollPosLeft = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosLeft, apparelScrollRect);
                    
                    for (int i = 0; i < this.SelectedApparel.Count; ++i)
                    {
                        Apparel assigned = this.SelectedApparel[i];
                        Rect rowRect = new Rect(0, 2f + i * cellHeight, apparelListRect.width, cellHeight);
                        GUI.BeginGroup(rowRect);

                        Widgets.ThingIcon(new Rect(0f, 0f, cellHeight, cellHeight), assigned);

                        Text.Font = GameFont.Small;
                        Widgets.Label(new Rect(cellHeight + 5f, 0f, rowRect.width - 40f - cellHeight, cellHeight), assigned.Label);

                        GUI.color = Color.white;

                        Rect buttonRect = new Rect(rowRect.width - 35f, 10, 20, 20);

                        if (Widgets.ButtonImage(buttonRect, WidgetUtil.nextTexture))
                        {
                            this.MoveRight(assigned);
                            GUI.EndGroup();
                            break;
                        }

                        GUI.EndGroup();
                    }
                    GUI.EndScrollView();
                    GUI.EndGroup();
                }
                
                GUI.Label(new Rect(inRect.width * 0.5f, 80, 150, 30), "ChangeDresser.Storage".Translate(), WidgetUtil.MiddleCenter);
                apparelListRect = new Rect(inRect.width * 0.5f + 10f, 120, apparelListWidth, inRect.height - 150);
                apparelScrollRect = new Rect(0f, 0f, apparelListRect.width - 16f, this.PossibleApparel.Count * cellHeight);

                GUI.BeginGroup(apparelListRect);
                this.scrollPosRight = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosRight, apparelScrollRect);

                for (int i = 0; i < this.PossibleApparel.Count; ++i)
                {
                    Apparel possible = this.PossibleApparel[i];
                    Rect rowRect = new Rect(0, 2f + i * cellHeight, apparelListRect.width, cellHeight);
                    GUI.BeginGroup(rowRect);

                    if (this.SelectedPawn != null && this.SelectedApparelSet != null && 
                        this.CanWear(this.SelectedApparel, possible) && 
                        Widgets.ButtonImage(new Rect(5, 10, 20, 20), WidgetUtil.previousTexture))
                    {
                        this.MoveLeft(possible);
                        GUI.EndGroup();
                        break;
                    }
                    
                    Widgets.ThingIcon(new Rect(35f, 0f, cellHeight, cellHeight), possible);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(new Rect(cellHeight + 45f, 0f, rowRect.width - cellHeight - 45f, cellHeight), possible.Label);

                    GUI.EndGroup();
                }
                GUI.EndScrollView();
                GUI.EndGroup();
                
                GUI.BeginGroup(new Rect(0, inRect.height - 35, inRect.width, 30));
                float middle = inRect.width / 2;
                float halfMiddle = middle / 2;
                if (Widgets.ButtonText(new Rect(halfMiddle - 50, 0, 100, 30), "ChangeDresser.Save".Translate(), true, false, this.SelectedApparelSet != null && this.SelectedApparelSet.Name != null && this.SelectedApparelSet.Name.Trim().Length != 0))
                {
                    this.Save();
#if DEBUG
                    Log.Warning("SGUI.DoWindowContents->Save: Set Count: " + this.ApparelSets.Count);
                    foreach (StoredApparelSet s in this.ApparelSets)
                    {
                        Log.Warning(s.ToString());
                    }
#endif
                }
                if (Widgets.ButtonText(new Rect(middle - 50, 0, 100, 30), "CloseButton".Translate()))
                {
                    this.Close();
                }
                if (Widgets.ButtonText(new Rect(middle + halfMiddle - 50, 0, 100, 30), "ChangeDresser.Delete".Translate(), true, false, this.SelectedApparelSet != null))
                {
                    StoredApparelContainer.RemoveApparelSet(this.SelectedPawn, this.SelectedApparelSet, this.Dresser);
                    this.PawnSelected(this.SelectedPawn);
                }
                GUI.EndGroup();
            }
            catch(Exception e)
            {
                Log.Error(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message);
                Messages.Message(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message, MessageSound.Negative);
                base.Close();
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        private void Save()
        {
#if DEBUG
            Log.Warning("SGUI.Save: Apparel Set Added " + StoredApparelContainer.StoredApparelSets.Count);
#endif

            List<Apparel> removedApparel = new List<Apparel>();
            foreach (Apparel previouslyAssigned in this.SelectedApparelSet.AssignedApparel)
            {
                if (!this.SelectedApparel.Contains(previouslyAssigned))
                {
#if DEBUG
                    Log.Warning("SGUI.Save: Remove " + previouslyAssigned.Label);
#endif
                    removedApparel.Add(previouslyAssigned);

                    if (this.SelectedApparelSet.IsBeingWorn)
                    {
#if DEBUG
                        Log.Warning("SGUI.Save: Is being worn, remove!");
#endif
                        this.SelectedPawn.apparel.Remove(previouslyAssigned);
                    }
                }
            }

            foreach (Apparel selected in this.SelectedApparel)
            {
                if (!this.SelectedApparelSet.IsApparelUsed(selected))
                {
#if DEBUG
                    Log.Warning("SGUI.Save: New " + selected.Label);
#endif
                    this.Dresser.RemoveNoDrop(selected);

                    if (this.SelectedApparelSet.IsBeingWorn)
                    {
#if DEBUG
                        Log.Warning("SGUI.Save: Not being worn, wear it!");
#endif
                        this.SelectedPawn.apparel.Wear(selected);
                    }
                }
            }

            this.SelectedApparelSet.AssignedApparel = new List<Apparel> (this.SelectedApparel);
            StoredApparelContainer.AddApparelSet(this.SelectedApparelSet);

            foreach (Apparel removed in removedApparel)
            {
                if (!StoredApparelContainer.IsApparelUsedInSets(this.SelectedPawn, removed))
                {
#if DEBUG
                    Log.Message("Removed apparel [" + removed.Label + "] is not being used in any other apparel set. Will be put into the dresser.");
#endif
                    this.Dresser.StoredApparel.Add(removed);
                }
#if DEBUG
                else
                {
                    //Log.Message("SGUI.Save: Removed apparel [" + removed.Label + "] is still being used in other apparel groups.");
                }
#endif
            }

            Messages.Message(this.SelectedApparelSet.Name + " saved.", MessageSound.Benefit);

            bool found = false;
            foreach (StoredApparelSet s in this.ApparelSets)
            {
                if (s.Equals(this.SelectedApparelSet))
                {
#if DEBUG
                    Log.Warning("SGUI.Save: " + s.Name + " == " + this.SelectedApparelSet.Name);
#endif
                    found = true;
                    break;
                }
#if DEBUG
                else
                {
                    Log.Warning("SGUI.Save: " + s.Name + " != " + this.SelectedApparelSet.Name);
                }
#endif
            }
            if (!found)
            {
#if DEBUG
                Log.Warning("SGUI.Save: Adding " + this.SelectedApparelSet.Name + " to sets");
#endif
                this.ApparelSets.Add(this.SelectedApparelSet);
            }
#if DEBUG
            Log.Warning("SGUI.Save Done: Set Count: " + this.ApparelSets.Count);
            Log.Warning(this.SelectedApparelSet.ToString());
#endif
            bool allWorn = true;
            foreach (Apparel a in this.SelectedApparelSet.AssignedApparel)
            {
                found = false;
                foreach (Apparel w in this.SelectedPawn.apparel.WornApparel)
                {
                    if (a.thingIDNumber == w.thingIDNumber)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    allWorn = false;
                    break;
                }
            }
            if (allWorn)
            {
                this.SelectedApparelSet.IsBeingWorn = true;
            }
        }

        private void MoveRight(Apparel apparel)
        {
            this.SelectedApparel.Remove(apparel);
            this.PossibleApparel.Add(apparel);
        }

        private void MoveLeft(Apparel apparel)
        {
            this.PossibleApparel.Remove(apparel);
            this.SelectedApparel.Add(apparel);
        }

        private void NewApparelSet()
        {
            this.SelectedApparelSet = new StoredApparelSet();
            this.SelectedApparelSet.IsBeingWorn = false;
            this.SelectedApparelSet.ForBattle = false;
            this.SelectedApparelSet.IsTemp = false;
            this.SelectedApparelSet.SwitchedFrom = false;
            this.SelectedApparelSet.Pawn = this.SelectedPawn;
            this.SelectedApparel = new List<Apparel>();

            this.UpdatePossibleApparel();
        }

        private void ApparelSetSelected(StoredApparelSet s)
        {
            this.SelectedApparelSet = s;

            this.SelectedApparel = new List<Apparel>(s.AssignedApparel);

            this.UpdatePossibleApparel();
        }

        private void UpdatePossibleApparel()
        {
            if (this.possibleApparel != null)
                this.possibleApparel.Clear();

            IEnumerable<Apparel> tmp;
            List<Apparel> assignedApparel;
            if (StoredApparelContainer.TryGetAssignedApparel(this.SelectedPawn, out tmp))
            {
                assignedApparel = new List<Apparel>(tmp);
            }
            else
            {
                assignedApparel = new List<Apparel>(0);
            }

            this.possibleApparel = new List<Apparel>(
                this.SelectedPawn.apparel.WornApparelCount + this.Dresser.StoredApparel.Count + assignedApparel.Count);
            this.possibleApparel.AddRange(this.SelectedPawn.apparel.WornApparel);
            foreach (Apparel a in assignedApparel)
            {
                bool found = false;
                foreach(Apparel p in this.possibleApparel)
                {
                    if (a.thingIDNumber == p.thingIDNumber)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    this.possibleApparel.Add(a);
                }
            }
            this.possibleApparel.AddRange(this.Dresser.StoredApparel);
        }

        private void PawnSelected(Pawn pawn)
        {
            this.SelectedPawn = pawn;
            IEnumerable<StoredApparelSet> sets;
            if (StoredApparelContainer.TryGetApparelSets(pawn, out sets))
            {
                this.ApparelSets = new List<StoredApparelSet>(sets);
            }
            else
            {
                this.ApparelSets = new List<StoredApparelSet>();
            }
#if DEBUG
            Log.Warning("SGUI.PawnSelected: Found Set Count: " + this.ApparelSets.Count);
#endif
            this.SelectedApparelSet = null;
            this.SelectedApparel = null;
        }

        private bool CanWear(List<Apparel> worn, Apparel newApparel)
        {
            for (int i = worn.Count - 1; i >= 0; i--)
            {
                Apparel apparel = worn[i];
                if (!ApparelUtility.CanWearTogether(newApparel.def, apparel.def))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
