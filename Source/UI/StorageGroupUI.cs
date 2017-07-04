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
using ChangeDresser.StoredApparel;

namespace ChangeDresser.UI
{
    public class StorageGroupUI : Window
    {
        public enum ApparelFromEnum { Pawn, Storage };
        private readonly Building_Dresser Dresser;
        private readonly StoredApparelSet set;
        private Pawn Pawn;
        private readonly ApparelFromEnum ApparelFrom;
        private readonly bool IsNew;
        private readonly bool FromGizmo;
        private bool isRestricted = false;

        private Vector2 scrollPosLeft = new Vector2(0, 0);
        private Vector2 scrollPosRight = new Vector2(0, 0);

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
                        if (p.Faction == Faction.OfPlayer && p.def.defName.Equals("Human"))
                        {
                            selectablePawns.Add(p);
                        }
                    }
                }
                return selectablePawns;
            }
        }
        internal static void ClearPlayerPawns() { selectablePawns.Clear();  }

        public StorageGroupUI(StoredApparelSet set, ApparelFromEnum apparelFrom, Building_Dresser dresser, Pawn pawn, bool isNew, bool fromGizmo = false)
        {
            this.set = set;
            this.ApparelFrom = apparelFrom;
            this.Dresser = dresser;
            this.Pawn = pawn;
            this.IsNew = isNew;
            this.FromGizmo = fromGizmo;
            if (this.FromGizmo)
            {
                if (this.set.HasOwner)
                    this.isRestricted = true;
            }
            
            this.closeOnEscapeKey = false;
            this.doCloseButton = false;
            this.doCloseX = false;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
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
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(0, 0, 200, 50), "ChangeDresser.ApparelGroupLabel".Translate());
                
                Text.Font = GameFont.Small;
                if (!this.FromGizmo)
                {
                    Widgets.Label(new Rect(250, 0, 200, 50), "ChangeDresser.ApparelGroupOwner".Translate() + ": " + this.Pawn.Name.ToStringShort);
                }
                else if (this.FromGizmo && this.isRestricted)
                {
                    string label;
                    if (this.set.HasOwner)
                        label = this.set.OwnerName;
                    else
                        label = "ChangeDresser.ApparelGroupOwner".Translate();

                    if (Widgets.ButtonText(new Rect(275, 10, 150, 30), label))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (Pawn p in PlayerPawns)
                        {
                            options.Add(new FloatMenuOption(p.Name.ToStringShort, delegate
                            {
                                StoredApparelSet tempSet;
                                if (this.set.SwitchForBattle && p != this.set.Owner &&
                                    StoredApparelContainer.TryGetBattleApparelSet(p, out tempSet))
                                {
                                    Messages.Message(p.Name.ToStringShort + " " + "ChangeDresser.AlreadyHasCombatApparel".Translate(), MessageSound.Negative);
                                }
                                else
                                    this.set.SetOwner(p);
                            }, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }

                Rect rect = new Rect(0, 50, inRect.width, 30);
                Text.Font = GameFont.Small;
                GUI.BeginGroup(rect);
                GUI.Label(new Rect(0, 0, 100, rect.height), "ChangeDresser.ApparelGroupName".Translate() + ":", WidgetUtil.MiddleCenter);
                this.set.Name = Widgets.TextField(new Rect(110, 0, 150, rect.height), this.set.Name);

                GUI.Label(new Rect(280, 0, 120, rect.height), "ChangeDresser.ApparelGroupRestrictToPawnCheckBox".Translate() + ":", WidgetUtil.MiddleCenter);
                bool temp = this.set.HasOwner || (this.FromGizmo && this.isRestricted);
                temp = GUI.Toggle(new Rect(410, 7, rect.height, rect.height), temp, "");
                if (this.isRestricted != temp)
                {
                    this.isRestricted = temp;
                    if (this.isRestricted && !this.FromGizmo)
                    {
                        this.set.SetOwner(this.Pawn);
                    }
                    else
                    {
                        this.set.SetOwner(null);
                        this.set.SwitchForBattle = false;
                    }
                }
                
                if (!this.FromGizmo || (this.FromGizmo && this.set.HasOwner))
                {
                    GUI.Label(new Rect(440, 0, 150, rect.height), "ChangeDresser.ForceSwitchCombat".Translate() + ":", WidgetUtil.MiddleCenter);
                    bool forceSwitch = GUI.Toggle(new Rect(600, 7, rect.height, rect.height), this.set.SwitchForBattle, "");
                    if (forceSwitch != this.set.SwitchForBattle)
                    {
                        if (forceSwitch)
                        {
                            StoredApparelSet tempSet;
                            if (this.Pawn != null && StoredApparelContainer.TryGetBattleApparelSet(this.Pawn, out tempSet))
                            {
                                Messages.Message(this.Pawn.Name.ToStringShort + " " + "ChangeDresser.AlreadyHasCombatApparel".Translate(), MessageSound.Negative);
                            }
                            else
                            {
                                this.set.SwitchForBattle = forceSwitch;
                                if (!this.set.HasOwner)
                                {
                                    this.set.SetOwner(this.Pawn);
                                }
                            }
                        }
                    }
                }

                GUI.EndGroup();

                List<Apparel> possibleApparel = (this.ApparelFrom == ApparelFromEnum.Pawn && this.Pawn != null) ? this.Pawn.apparel.WornApparel : this.Dresser.StoredApparel;
                List<Apparel> groupApparel = this.set.Apparel;
                List<string> forcedApparelIds = this.set.ForcedApparelIds;

                GUI.BeginGroup(new Rect(0, 90, inRect.width, 30));
                GUI.Label(new Rect(0, 0, 100, 30), ((string)((this.ApparelFrom == ApparelFromEnum.Pawn) ? "ChangeDresser.Worn" : "ChangeDresser.Storage")).Translate(), WidgetUtil.MiddleCenter);
                GUI.Label(new Rect(inRect.width * 0.5f, 0, 100, 30), "ChangeDresser.ApparelGroup".Translate(), WidgetUtil.MiddleCenter);
                GUI.EndGroup();

                const float cellHeight = 40f;
                float apparelListWidth = inRect.width * 0.5f - 10f;
                Rect apparelListRect = new Rect(0, 120, apparelListWidth, inRect.height - 150);
                Rect apparelScrollRect = new Rect(0f, 0f, apparelListRect.width - 16f, possibleApparel.Count * cellHeight);

                GUI.BeginGroup(apparelListRect);
                this.scrollPosLeft = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosLeft, apparelScrollRect);

                GUI.color = Color.white;
                Text.Font = GameFont.Medium;
                Pawn pawnToUse = this.Pawn;
                if (pawnToUse == null && this.FromGizmo && this.set.HasOwner)
                {
                    pawnToUse = set.Owner;
                }
                for (int i = 0; i < possibleApparel.Count; ++i)
                {
                    Apparel apparel = possibleApparel[i];
                    Rect rowRect = new Rect(0, 2f + i * cellHeight, apparelListRect.width, cellHeight);
                    GUI.BeginGroup(rowRect);

                    Widgets.ThingIcon(new Rect(0f, 0f, cellHeight, cellHeight), apparel);

                    Text.Font = GameFont.Small;
                    Widgets.Label(new Rect(cellHeight + 5f, 0f, rowRect.width - 40f - cellHeight, cellHeight), apparel.Label);

                    GUI.color = Color.white;

                    Rect buttonRect = new Rect(rowRect.width - 35f, 10, 20, 20);
                    
                    if (pawnToUse != null && this.CanWear(groupApparel, apparel))
                    {
                        if (Widgets.ButtonImage(buttonRect, WidgetUtil.nextTexture))
                        {
                            this.RemoveApparelFromSender(apparel);
                            pawnToUse.apparel.Remove(apparel);
                            groupApparel.Add(apparel);
                            GUI.EndGroup();
                            break;
                        }
                    }
                    else
                    {
                        Widgets.ButtonImage(buttonRect, WidgetUtil.cantTexture);
                    }
                    GUI.EndGroup();
                }
                GUI.EndScrollView();
                GUI.EndGroup();
                
                apparelListRect = new Rect(inRect.width * 0.5f + 10f, 120, apparelListWidth, inRect.height - 150);
                apparelScrollRect = new Rect(0f, 0f, apparelListRect.width - 16f, groupApparel.Count * cellHeight);
                
                GUI.BeginGroup(apparelListRect);
                this.scrollPosRight = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosRight, apparelScrollRect);
                
                GUI.color = Color.white;
                Text.Font = GameFont.Medium;
                for (int i = 0; i < groupApparel.Count; ++i)
                {
                    Apparel apparel = groupApparel[i];
                    Rect rowRect = new Rect(0, 2f + i * cellHeight, apparelListRect.width, cellHeight);
                    GUI.BeginGroup(rowRect);
                    
                    if (Widgets.ButtonImage(new Rect(5, 10, 20, 20), WidgetUtil.previousTexture))
                    {
                        this.AddApparelToSender(apparel, forcedApparelIds.Contains(apparel.ThingID));
                        groupApparel.RemoveAt(i);
                        GUI.EndGroup();
                        break;
                    }
                    
                    Widgets.ThingIcon(new Rect(35f, 0f, cellHeight, cellHeight), apparel);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    string label = apparel.Label;
                    if (forcedApparelIds != null && forcedApparelIds.Contains(apparel.ThingID))
                    {
                        label += "(" + "ApparelForcedLower".Translate() + ")";
                    }
                    Widgets.Label(new Rect(cellHeight + 45f, 0f, rowRect.width - cellHeight - 45f, cellHeight), label);

                    GUI.EndGroup();
                }
                GUI.EndScrollView();
                GUI.EndGroup();
                
                Text.Font = GameFont.Small;
                GUI.BeginGroup(new Rect(0, inRect.height - 35, inRect.width, 30));
                float middle = inRect.width / 2;
                if (Widgets.ButtonText(new Rect(middle - 110, 0, 100, 30), "ChangeDresser.Save".Translate(), true, false, this.set.HasName))
                {
                    this.set.ClearWornBy();
                    if (IsNew)
                        StoredApparelContainer.AddApparelSet(this.set);
                    this.Close();
                }
                Rect rightButton = new Rect(middle + 10, 0, 100, 30);
                if (IsNew && this.Pawn != null && Widgets.ButtonText(rightButton, "ChangeDresser.Cancel".Translate()))
                {
                    for (int i = 0; i < this.set.Apparel.Count; ++i)
                    {
                        Apparel apparel = this.set.Apparel[i];
                        this.AddApparelToSender(apparel, this.set.ForcedApparelIds.Contains(apparel.ThingID));
                    }
                    this.Close();
                }
                if (this.set.Apparel.Count > 0)
                {
                    Text.Font = GameFont.Small;
                    rightButton.width = 300;
                    GUI.Label(rightButton, "ChangeDresser.RemoveToEnableDelete".Translate(), WidgetUtil.MiddleCenter);
                }
                else if (Widgets.ButtonText(rightButton, "ChangeDresser.Delete".Translate(), true, false, this.set.Apparel.Count == 0))
                {
                    if (!IsNew)
                        StoredApparelContainer.RemoveApparelSet(this.set);
                    this.Close();
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

        private void AddApparelToSender(Apparel apparel, bool forced)
        {
            if (this.ApparelFrom == ApparelFromEnum.Pawn)
            {
                this.Pawn.apparel.Wear(apparel);
                if (forced)
                    this.Pawn.outfits.forcedHandler.ForcedApparel.Add(apparel);
            }
            else
            {
                this.Dresser.StoredApparel.Add(apparel);
            }
        }

        private void RemoveApparelFromSender(Apparel apparel)
        {
            if (this.ApparelFrom == ApparelFromEnum.Pawn)
            {
                this.Pawn.apparel.Remove(apparel);
            }
            else
            {
                this.Dresser.StoredApparel.Remove(apparel);
            }
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
