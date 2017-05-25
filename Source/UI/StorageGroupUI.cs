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
using ChangeDresser.UI.DTO.StorageDTOs;
using System;
using System.Collections.Generic;

namespace ChangeDresser.UI
{
    public class StorageGroupUI : Window
    {
        public enum ApparelFromEnum { Pawn, Storage };
        private readonly Building_Dresser Dresser;
        private readonly StorageGroupDTO StorageGroupDto;
        private readonly Pawn Pawn;
        private readonly ApparelFromEnum ApparelFrom;
        private readonly bool IsNew;

        private Vector2 scrollPosLeft = new Vector2(0, 0);
        private Vector2 scrollPosRight = new Vector2(0, 0);

        public StorageGroupUI(StorageGroupDTO storageGroupDTO, ApparelFromEnum apparelFrom, Building_Dresser dresser, Pawn pawn, bool isNew)
        {
            this.StorageGroupDto = storageGroupDTO;
            this.ApparelFrom = apparelFrom;
            this.Dresser = dresser;
            this.Pawn = pawn;
            this.IsNew = isNew;

            if (this.StorageGroupDto.IsRestricted &&
                this.StorageGroupDto.CanPawnAccess(this.Pawn))
            {
                // Update the name (in case the user changed it)
                this.StorageGroupDto.RestrictToPawn(this.Pawn);
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

                if (this.StorageGroupDto.IsRestricted)
                {
                    Widgets.Label(new Rect(250, 0, 200, 50), "ChangeDresser.ApparelGroupOwner".Translate() + ": " + this.StorageGroupDto.RestrictToPawnName);
                }

                Text.Font = GameFont.Small;

                Rect rect = new Rect(0, 50, inRect.width, 30);
                Text.Font = GameFont.Small;
                GUI.BeginGroup(rect);
                GUI.Label(new Rect(0, 0, 100, rect.height), "ChangeDresser.ApparelGroupName".Translate() + ":", WidgetUtil.MiddleCenter);
                this.StorageGroupDto.Name = Widgets.TextField(new Rect(110, 0, 150, rect.height), this.StorageGroupDto.Name);

                GUI.Label(new Rect(280, 0, 120, rect.height), "ChangeDresser.ApparelGroupRestrictToPawnCheckBox".Translate() + ":", WidgetUtil.MiddleCenter);
                bool isRestricted = this.StorageGroupDto.IsRestricted;
                isRestricted = GUI.Toggle(new Rect(410, 7, rect.height, rect.height), isRestricted, "");
                if (isRestricted != this.StorageGroupDto.IsRestricted)
                {
                    if (isRestricted)
                    {
                        this.StorageGroupDto.RestrictToPawn(this.Pawn);
                    }
                    else
                    {
                        this.StorageGroupDto.Unrestrict();
                    }
                }
                
                if (BattleApparelGroupDTO.ShowForceBattleSwitch)
                {
                    GUI.Label(new Rect(440, 0, 150, rect.height), "ChangeDresser.ForceSwitchCombat".Translate() + ":", WidgetUtil.MiddleCenter);
                    this.StorageGroupDto.SetForceSwitchBattle(
                        GUI.Toggle(new Rect(600, 7, rect.height, rect.height), this.StorageGroupDto.ForceSwitchBattle, ""), 
                        this.Pawn);
                }
                GUI.EndGroup();

                List<Apparel> possibleApparel = (this.ApparelFrom == ApparelFromEnum.Pawn) ? this.Pawn.apparel.WornApparel : this.Dresser.StoredApparel;
                List<Apparel> groupApparel = this.StorageGroupDto.Apparel;
                List<bool> forcedApparel = this.StorageGroupDto.ForcedApparel;

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
                    if (this.CanWear(groupApparel, apparel))
                    {
                        if (Widgets.ButtonImage(buttonRect, WidgetUtil.nextTexture))
                        {
                            this.RemoveApparelFromSender(apparel);
                            Pawn.apparel.Remove(apparel);
                            groupApparel.Add(apparel);
                            forcedApparel.Add(false);
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
                        this.AddApparelToSender(apparel, forcedApparel[i]);
                        groupApparel.RemoveAt(i);
                        forcedApparel.RemoveAt(i);
                        GUI.EndGroup();
                        break;
                    }

                    Widgets.ThingIcon(new Rect(35f, 0f, cellHeight, cellHeight), apparel);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    string label = apparel.Label;
                    if (forcedApparel[i])
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
                if (Widgets.ButtonText(new Rect(middle - 110, 0, 100, 30), "ChangeDresser.Save".Translate(), true, false, this.StorageGroupDto.HasName()))
                {
                    this.StorageGroupDto.ClearWornBy();
                    this.Close();
                }
                Rect rightButton = new Rect(middle + 10, 0, 100, 30);
                if (IsNew && Widgets.ButtonText(rightButton, "ChangeDresser.Cancel".Translate()))
                {
                    for (int i = 0; i < this.StorageGroupDto.Apparel.Count; ++i)
                    {
                        Apparel apparel = this.StorageGroupDto.Apparel[i];
                        this.AddApparelToSender(apparel, this.StorageGroupDto.ForcedApparel[i]);
                    }
                    this.Dresser.Remove(this.StorageGroupDto);
                    this.Close();
                }
                if (!IsNew)
                {
                    if (this.StorageGroupDto.Apparel.Count > 0)
                    {
                        Text.Font = GameFont.Small;
                        rightButton.width = 300;
                        GUI.Label(rightButton, "ChangeDresser.RemoveToEnableDelete".Translate(), WidgetUtil.MiddleCenter);
                    }
                    else if (Widgets.ButtonText(rightButton, "ChangeDresser.Delete".Translate(), true, false, this.StorageGroupDto.Apparel.Count == 0))
                    {
                        this.StorageGroupDto.Delete();
                        this.Dresser.Remove(this.StorageGroupDto);
                        this.Close();
                    }
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
