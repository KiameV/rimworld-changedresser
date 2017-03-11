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
using ChangeDresser.UI.DTO;
using ChangeDresser.UI.Util;
using RimWorld;
using UnityEngine;
using Verse;
using ChangeDresser.UI.Enums;
using ChangeDresser.UI.DTO.SelectionWidgetDTOs;
using System.Reflection;
using ChangeDresser.Util;
using ChangeDresser.UI.DTO.StorageDTOs;
using System;

namespace ChangeDresser.UI
{
    class StorageUI : Window
    {
        private StorageGroupDTO storageGroupDto;

        public StorageUI(StorageGroupDTO storageGroupDto)
        {
            this.closeOnEscapeKey = true;
            this.doCloseButton = false;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.storageGroupDto = storageGroupDto;
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
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, 200, 50), "Storage Group");

            Rect rect = new Rect(0, 50, inRect.width, 30);
            Text.Font = GameFont.Small;
            GUI.BeginGroup(rect);
            GUI.Label(new Rect(0, 0, 60, rect.height), "Group Name:");
            this.storageGroupDto.Name = Widgets.TextField(new Rect(70, 0, 60, rect.height), this.storageGroupDto.Name);

            GUI.Label(new Rect(140, 0, 60, rect.height), "Restrict to Pawn:");
            this.storageGroupDto.RestrictToPawn = GUI.Toggle(new Rect(200, 0, rect.height, rect.height), this.storageGroupDto.RestrictToPawn, "");

            GUI.Label(new Rect(240, 0, 60, rect.height), "Force Switch Before/After Combat:");
            this.storageGroupDto.ForceSwitch = GUI.Toggle(new Rect(310, 0, rect.height, rect.height), this.storageGroupDto.ForceSwitch, "");

        }
    }
}
