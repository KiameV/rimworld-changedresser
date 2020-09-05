using ChangeDresser.UI.Util;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;

namespace ChangeDresser.UI
{
    class StorageUI : Window
    {
        private Building_Dresser Dresser;
        private Pawn Pawn;
        private string Filter = "";
        private float height = 2f;
        private Vector2 scrollPosLeft = new Vector2(0, 0);
        private Vector2 scrollPosRight = new Vector2(0, 0);

        private List<Apparel> cachedApparel = null;
        private List<Apparel> CachedApparel
        {
            get
            {
                if (this.cachedApparel == null)
                {
                    this.cachedApparel = new List<Apparel>(this.Dresser.Count);
                    this.cachedApparel.AddRange(this.Dresser.Apparel);
                }
                return this.cachedApparel;
            }
        }

        public StorageUI(Pawn pawn)
        {
            this.Dresser = null;
            this.Pawn = pawn;

            this.closeOnClickedOutside = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
        }

        public StorageUI(Building_Dresser dresser, Pawn pawn)
        {
            this.Dresser = dresser;
            this.Pawn = pawn;

            this.closeOnClickedOutside = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
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
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(0, 0, 200, 50), "ChangeDresser.ApparelStorageLabel".Translate());

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                this.Filter = Widgets.TextArea(new Rect(250, 0, 150, 32), this.Filter);

                Text.Anchor = TextAnchor.MiddleCenter;
                if (Widgets.ButtonText(new Rect(425, 0, 250, 32), ((this.Dresser == null) ? (string)"ChangeDresser".Translate() : this.Dresser.Label)))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Building_Dresser cd in WorldComp.GetDressers(null))
                    {
                        options.Add(new FloatMenuOption(cd.Label, delegate ()
                        {
                            this.Dresser = cd;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }

                if (this.Dresser == null)
                    return;

                Text.Font = GameFont.Small;

                List<Apparel> wornApparel = (this.Pawn != null) ? this.Pawn.apparel.WornApparel : null;

                const float cellHeight = 40f;
                float apparelListWidth = inRect.width * 0.5f - 10f;
                Rect apparelListRect;
                Rect apparelScrollRect;

                if (wornApparel != null)
                {
                    GUI.Label(new Rect(0, 60, 100, 30), ("ChangeDresser.Worn").Translate(), WidgetUtil.MiddleCenter);

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

                        if (Widgets.InfoCardButton(40, 0, apparel))
                        {
                            Find.WindowStack.Add(new Dialog_InfoCard(apparel));
                        }

                        Text.Font = GameFont.Small;
                        Widgets.Label(new Rect(35f + cellHeight, 0f, rowRect.width - 110f, cellHeight), apparel.Label);

                        GUI.color = Color.white;
                        if (Widgets.ButtonImage(new Rect(rowRect.width - 35f, 10, 20, 20), WidgetUtil.nextTexture))
                        {
                            this.Pawn.apparel.Remove(apparel);
                            this.Dresser.AddApparel(apparel);
                            this.cachedApparel.Clear();
                            this.cachedApparel = null;
                            GUI.EndGroup();
                            break;
                        }
                        GUI.EndGroup();
                    }
                    GUI.EndScrollView();
                    GUI.EndGroup();
                }
                
                GUI.Label(new Rect((wornApparel == null) ? 0 : inRect.width * 0.5f, 60, (wornApparel == null) ? inRect.width : 100, 30), ("ChangeDresser.Storage").Translate(), WidgetUtil.MiddleCenter);

                float left = (wornApparel == null) ? 0 : inRect.width - apparelListWidth;
                float width = (wornApparel == null) ? inRect.width : apparelListWidth;
                apparelListRect = new Rect(left, 90, width, inRect.height - 130);
                apparelScrollRect = new Rect(0f, 0f, width - 16f, this.height);
                this.height = 2f;

                GUI.BeginGroup(apparelListRect);
                this.scrollPosRight = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosRight, apparelScrollRect);

                string filter = this.Filter.Trim().ToLower();

                GUI.color = Color.white;
                Text.Font = GameFont.Medium;
                for (int i = 0; i < this.CachedApparel.Count; ++i)
                {
                    Apparel apparel = this.cachedApparel[i];

                    if (filter != "" && apparel.Label.ToLower().IndexOf(filter) == -1)
                        continue;

                    Rect rowRect = new Rect(0, this.height, apparelScrollRect.width, cellHeight);
                    this.height += cellHeight;
                    GUI.BeginGroup(rowRect);

                    if (this.Pawn != null)
                    {
                        Rect buttonRect = new Rect(5, 10, 20, 20);
                        bool canWear = this.Pawn.apparel.CanWearWithoutDroppingAnything(apparel.def);
                        if (canWear)
                        {
                            if (Widgets.ButtonImage(buttonRect, WidgetUtil.previousTexture))
                            {
                                if (this.Dresser.TryRemove(apparel, false))
                                {
                                    this.cachedApparel.Clear();
                                    this.cachedApparel = null;
                                    PawnOutfitTracker outfits;
                                    if (WorldComp.PawnOutfits.TryGetValue(this.Pawn, out outfits))
                                    {
                                        outfits.ApplyApparelColor(apparel);
                                    }
                                    this.Pawn.apparel.Wear(apparel);
                                    GUI.EndGroup();
                                    break;
                                }
                                else
                                {
                                    Log.Error("Problem dropping " + apparel.Label);
                                }
                            }
                        }
                        else
                        {
                            Widgets.ButtonImage(buttonRect, WidgetUtil.cantTexture);
                        }
                    }

                    Widgets.ThingIcon(new Rect(35f, 0f, cellHeight, cellHeight), apparel);

                    if (Widgets.InfoCardButton(75, 0, apparel))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(apparel));
                    }

                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(new Rect(30 + cellHeight + 15f, 0f, rowRect.width - 110f, cellHeight), apparel.Label);

                    if (Widgets.ButtonImage(new Rect(rowRect.width - 45f, 0f, 20, 20), WidgetUtil.dropTexture))
                    {
                        if (this.Dresser.TryRemove(apparel, false))
                        {
                            this.cachedApparel.Clear();
                            this.cachedApparel = null;
                            GUI.EndGroup();
                            break;
                        }
                        else
                        {
                            Log.Error("Problem dropping " + apparel.Label);
                        }
                    }

                    GUI.EndGroup();
                }
                GUI.EndScrollView();

                GUI.EndGroup();
            }
            catch (Exception e)
            {
                Log.Error(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message);
                Messages.Message(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message, MessageTypeDefOf.RejectInput);
                base.Close();
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }
    }
}