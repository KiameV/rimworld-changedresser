using ChangeDresser.UI.Util;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ChangeDresser
{
    class Dialog_Reorder : Verse.Window
    {
        private Vector2 scroll = new(0, 0);
        private float previousY = 0;
        private static bool storageOrder = true;

        public Dialog_Reorder() : base()
        {
            this.closeOnClickedOutside = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
        }

        public override Vector2 InitialSize => new(650f, 600f);

        public override void DoWindowContents(Rect r)
        {
            float x = 0, y = 0;
            string label;
            LinkedList<Building_Dresser> l;
            if (storageOrder)
            {
                label = "ChangeDresser.StorageOrder".Translate();
                l = WorldComp.DresserStorageOrder;
            }
            else
            {
                label = "ChangeDresser.DressFromOrder".Translate();
                l = WorldComp.DresserPullOrder;
            }

            if (Widgets.ButtonText(new Rect(0, 0, 200, 32), label))
            {
                storageOrder = !storageOrder;
                return;
            }

            if (Widgets.ButtonText(new Rect(250, 0, 100, 32), "ChangeDresser.SortApparel".Translate()))
            {
                WorldComp.SortApparel();
                return;
            }

            try
            {
                Widgets.BeginScrollView(new Rect(0, 40, r.width, r.height - 80), ref scroll, new Rect(0, 0, r.width - 16, previousY));
                LinkedListNode<Building_Dresser> prev = null;
                for (var n = l.First; n != null; n = n.Next)
                {
                    var d = n.Value;
                    x = 0;
                    if (prev != null && 
                        d.settings.Priority == prev.Value.settings.Priority &&
                        Widgets.ButtonImage(new Rect(0, y + 1, 30, 30), WidgetUtil.upTexture, false))
                    {
                        MoveUp(l, n);
                        break;
                    }
                    x += 32;
                    if (n.Next != null &&
                        d.settings.Priority == n.Next.Value.settings.Priority &&
                        Widgets.ButtonImage(new Rect(x, y + 1, 30, 30), WidgetUtil.dropTexture, false))
                    {
                        MoveDown(l, n);
                        break;
                    }
                    x += 32;
                    Widgets.Label(new Rect(x, y, 400, 32), d.Label);
                    x += 410;
                    if (Widgets.ButtonText(new Rect(x, y, 100, 32), d.settings.Priority.Label()))
                    {
                        List<FloatMenuOption> list = new();
                        foreach (StoragePriority s in Enum.GetValues(typeof(StoragePriority)))
                        {
                            if (s != 0)
                            {
                                list.Add(new FloatMenuOption(s.Label().CapitalizeFirst(), delegate
                                {
                                    d.settings.Priority = s;
                                    WorldComp.RemoveDesser(d);
                                    WorldComp.AddDresser(d);
                                    return;
                                }));
                            }
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                    y += 34;
                    prev = n;
                }
                previousY = y;
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }

        private void MoveUp(LinkedList<Building_Dresser> l, LinkedListNode<Building_Dresser> node)
        {
            for (var n = l.First; n != null; n = n.Next)
            {
                if (n.Next == node)
                {
                    l.Remove(node);
                    l.AddBefore(n, node);
                }
            }
        }

        private void MoveDown(LinkedList<Building_Dresser> l, LinkedListNode<Building_Dresser> node)
        {
            var n = node.Next;
            if (n != null)
            {
                l.Remove(node);
                l.AddAfter(n, node);
            }
        }
    }
}
