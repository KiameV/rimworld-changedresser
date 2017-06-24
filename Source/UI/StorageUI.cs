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
using static ChangeDresser.UI.StorageGroupUI;
using ChangeDresser.StoredApparel;
using System.Text;

namespace ChangeDresser.UI
{
    class StorageUI : Window
    {
        private readonly Building_Dresser Dresser;
        private readonly bool FromGizmo;
        private Pawn Pawn;
        private Vector2 scrollPosLeft = new Vector2(0, 0);
        private Vector2 scrollPosRight = new Vector2(0, 0);

        public StorageUI(Building_Dresser dresser, Pawn pawn, bool fromGizmo = false)
        {
            this.Dresser = dresser;
            this.Pawn = pawn;
            this.FromGizmo = fromGizmo;

            this.closeOnEscapeKey = true;
            this.doCloseButton = true;
            this.doCloseX = true;
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
                Widgets.Label(new Rect(0, 0, 200, 50), "ChangeDresser.ApparelStorageLabel".Translate());

                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(new Rect(inRect.width * 0.5f, 10, 200, 30), "ChangeDresser.ApparelGroup".Translate()))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    if (!this.FromGizmo)
                    {
                        list.Add(new FloatMenuOption("ChangeDresser.CreateFromWorn".Translate(), delegate
                        {
                            StoredApparelSet set = new StoredApparelSet(this.Dresser);
                            Find.WindowStack.Add(new StorageGroupUI(set, ApparelFromEnum.Pawn, this.Dresser, this.Pawn, true, this.FromGizmo));
                        }));
                    }
                    list.Add(new FloatMenuOption("ChangeDresser.CreateFromStorage".Translate(), delegate
                    {
                        StoredApparelSet set = new StoredApparelSet(this.Dresser);
                        Find.WindowStack.Add(new StorageGroupUI(set, ApparelFromEnum.Storage, this.Dresser, this.Pawn, true, this.FromGizmo));
                    }));

                    if (this.FromGizmo)
                    {
                        this.CreateOptionsForUser(list);
                    }
                    else
                    {
                        this.CreateOptionsForPawn(list);
                    }
                    Find.WindowStack.Add(new FloatMenu(list, null, false));
                }

                /*if (this.storageGroupDto.IsPawnRestricted)
                {
                    Widgets.Label(new Rect(250, 0, 200, 50), "Owner: " + this.storageGroupDto.RestrictToPawnName);
                }

                Rect rect = new Rect(0, 50, inRect.width, 30);
                Text.Font = GameFont.Small;
                GUI.BeginGroup(rect);
                GUI.Label(new Rect(0, 0, 100, rect.height), "Group Name:", WidgetUtil.MiddleCenter);
                this.storageGroupDto.GroupName = Widgets.TextField(new Rect(110, 0, 150, rect.height), this.storageGroupDto.GroupName);

                GUI.Label(new Rect(280, 0, 100, rect.height), "Restrict to Pawn:", WidgetUtil.MiddleCenter);
                this.storageGroupDto.IsPawnRestricted = GUI.Toggle(new Rect(390, 7, rect.height, rect.height), this.storageGroupDto.IsPawnRestricted, "");

                GUI.Label(new Rect(440, 0, 150, rect.height), "Force Switch Combat:", WidgetUtil.MiddleCenter);
                this.storageGroupDto.ForceSwitchBattle = GUI.Toggle(new Rect(600, 7, rect.height, rect.height), this.storageGroupDto.ForceSwitchBattle, "");
                GUI.EndGroup();*/

                List<Apparel> wornApparel = (!this.FromGizmo) ? this.Pawn.apparel.WornApparel : null;
                List<Apparel> storedApparel = this.Dresser.StoredApparel;

                const float cellHeight = 40f;
                float apparelListWidth = inRect.width * 0.5f - 10f;
                Rect apparelListRect;
                Rect apparelScrollRect;

                if (!this.FromGizmo)
                {
                    apparelListRect = new Rect(0, 90, apparelListWidth, inRect.height - 130);
                    apparelScrollRect = new Rect(0f, 0f, apparelListWidth - 16f, wornApparel.Count * cellHeight);

                    GUI.BeginGroup(apparelListRect);
                    this.scrollPosLeft = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosLeft, apparelScrollRect);

                    GUI.color = Color.white;
                    Text.Font = GameFont.Medium;
                    for (int i = 0; i < wornApparel.Count; ++i)
                    {
                        Apparel apparel = wornApparel[i];
                        Rect rowRect = new Rect(0, 2f + i * cellHeight, apparelListRect.width, cellHeight);
                        GUI.BeginGroup(rowRect);

                        Widgets.ThingIcon(new Rect(0f, 0f, cellHeight, cellHeight), apparel);

                        Text.Font = GameFont.Small;
                        Widgets.Label(new Rect(cellHeight + 5f, 0f, rowRect.width - 40f - cellHeight, cellHeight), apparel.Label);

                        GUI.color = Color.white;
                        if (Widgets.ButtonImage(new Rect(rowRect.width - 35f, 10, 20, 20), WidgetUtil.nextTexture))
                        {
                            this.Pawn.apparel.Remove(apparel);
                            storedApparel.Add(apparel);
                            GUI.EndGroup();
                            break;
                        }
                        GUI.EndGroup();
                    }
                    GUI.EndScrollView();
                    GUI.EndGroup();
                }

                apparelListRect = new Rect(inRect.width - apparelListWidth, 90, apparelListWidth, inRect.height - 130);
                apparelScrollRect = new Rect(0f, 0f, apparelListWidth - 16f, storedApparel.Count * cellHeight);

                GUI.BeginGroup(apparelListRect);
                this.scrollPosRight = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosRight, apparelScrollRect);

                GUI.color = Color.white;
                Text.Font = GameFont.Medium;
                for (int i = 0; i < storedApparel.Count; ++i)
                {
                    Apparel apparel = storedApparel[i];
                    Rect rowRect = new Rect(0, 2f + i * cellHeight, apparelScrollRect.width, cellHeight);
                    GUI.BeginGroup(rowRect);

                    if (!this.FromGizmo)
                    {
                        Rect buttonRect = new Rect(5, 10, 20, 20);
                        bool canWear = this.Pawn.apparel.CanWearWithoutDroppingAnything(apparel.def);
                        if (canWear)
                        {
                            if (Widgets.ButtonImage(buttonRect, WidgetUtil.previousTexture))
                            {
                                storedApparel.Remove(apparel);
                                this.Pawn.apparel.Wear(apparel);
                                GUI.EndGroup();
                                break;
                            }
                        }
                        else
                        {
                            Widgets.ButtonImage(buttonRect, WidgetUtil.cantTexture);
                        }
                    }

                    Widgets.ThingIcon(new Rect(35f, 0f, cellHeight, cellHeight), apparel);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(new Rect(cellHeight + 45f, 0f, rowRect.width - cellHeight - 90f, cellHeight), apparel.Label);

                    if (Widgets.ButtonImage(new Rect(rowRect.width - 45f, 0f, cellHeight, cellHeight), WidgetUtil.dropTexture))
                    {
                        this.Dresser.Remove(apparel, false);
                        GUI.EndGroup();
                        break;
                    }

                    GUI.EndGroup();
                }
                GUI.EndScrollView();

                GUI.EndGroup();
            }
            catch (Exception e)
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

        private void CreateOptionsForPawn(List<FloatMenuOption> list)
        {
            IEnumerable<StoredApparelSet> sets;
            if (Settings.LinkGroupsToDresser)
                sets = StoredApparelContainer.GetApparelSets(this.Dresser);
            else
                sets = StoredApparelContainer.GetAllApparelSets();

            foreach (StoredApparelSet set in sets)
            {
                string label = "";
                if (set.IsBeingWorn)
                {
                    label = label + " (" + "ChangeDresser.BeingWorn".Translate() + ")";
                }

                if (set.HasOwner && !set.IsOwnedBy(this.Pawn))
                {
                    string ownerName = "";
                    if (set.HasOwner)
                        ownerName = " - " + set.OwnerName;
                    list.Add(new FloatMenuOption("ChangeDresser.ClaimApparelGroup".Translate() + " " + set.Name + ownerName + label, delegate
                    {
                        set.SetOwner(this.Pawn);
                    }));
                }
                else
                {
                    list.Add(new FloatMenuOption("ChangeDresser.EditApparelGroup".Translate() + " " + set.Name + " " + "ChangeDresser.FromWorn".Translate() + label, delegate
                    {
                        Find.WindowStack.Add(new StorageGroupUI(set, ApparelFromEnum.Pawn, this.Dresser, this.Pawn, false, false));
                    }));
                    list.Add(new FloatMenuOption("ChangeDresser.EditApparelGroup".Translate() + " " + set.Name + " " + "ChangeDresser.FromStorage".Translate() + label, delegate
                    {
                        Find.WindowStack.Add(new StorageGroupUI(set, ApparelFromEnum.Storage, this.Dresser, this.Pawn, false, false));
                    }));
                }
            }
        }

        private void CreateOptionsForUser(List<FloatMenuOption> list)
        {
            IEnumerable<StoredApparelSet> sets;
            if (Settings.LinkGroupsToDresser)
                sets = StoredApparelContainer.GetApparelSets(this.Dresser);
            else
                sets = StoredApparelContainer.GetAllApparelSets();

            foreach (StoredApparelSet set in sets)
            {
                StringBuilder sb = new StringBuilder("ChangeDresser.EditApparelGroup".Translate());
                sb.Append(" ");
                sb.Append(set.Name);
                if (set.HasOwner)
                {
                    sb.Append(" - ");
                    sb.Append(set.OwnerName);
                }
                if (set.IsBeingWorn)
                {
                    sb.Append(" (");
                    sb.Append("ChangeDresser.BeingWorn".Translate());
                    sb.Append(")");
                }
                list.Add(new FloatMenuOption(sb.ToString(), delegate
                {
                    Find.WindowStack.Add(new StorageGroupUI(set, ApparelFromEnum.Storage, this.Dresser, null, false, true));
                }));
            }
        }
    }
}