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
        private readonly Building_Dresser Dresser;
        private Pawn Pawn;
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

                        Text.Font = GameFont.Small;
                        Widgets.Label(new Rect(cellHeight + 5f, 0f, rowRect.width - 40f - cellHeight, cellHeight), apparel.Label);

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
                apparelScrollRect = new Rect(0f, 0f, width - 16f, this.Dresser.Count * cellHeight);

                GUI.BeginGroup(apparelListRect);
                this.scrollPosRight = GUI.BeginScrollView(new Rect(GenUI.AtZero(apparelListRect)), this.scrollPosRight, apparelScrollRect);

                GUI.color = Color.white;
                Text.Font = GameFont.Medium;
                for (int i = 0; i < this.CachedApparel.Count; ++i)
                {
                    Apparel apparel = this.cachedApparel[i];
                    Rect rowRect = new Rect(0, 2f + i * cellHeight, apparelScrollRect.width, cellHeight);
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
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(new Rect(cellHeight + 45f, 0f, rowRect.width - cellHeight - 90f, cellHeight), apparel.Label);

                    if (Widgets.ButtonImage(new Rect(rowRect.width - 45f, 0f, cellHeight, cellHeight), WidgetUtil.dropTexture))
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