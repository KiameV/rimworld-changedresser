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

        public StorageUI(Building_Dresser dresser, Pawn pawn)
        {
            this.Dresser = dresser;
            this.Pawn = pawn;

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

                List<Apparel> wornApparel = this.Pawn.apparel.WornApparel;
                List<Apparel> storedApparel = this.Dresser.StoredApparel;

                GUI.Label(new Rect(0, 60, 100, 30), ("ChangeDresser.Worn").Translate(), WidgetUtil.MiddleCenter);

                const float cellHeight = 40f;
                float apparelListWidth = inRect.width * 0.5f - 10f;
                Rect apparelListRect;
                Rect apparelScrollRect;
                
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

                GUI.Label(new Rect(inRect.width * 0.5f, 60, 100, 30), ("ChangeDresser.Storage").Translate(), WidgetUtil.MiddleCenter);

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
    }
}