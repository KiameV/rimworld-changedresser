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
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class ApparelLayerColorSelectionDTO : SelectionColorWidgetDTO
    {
        public readonly ApparelLayerDef ApparelLayerDef;
        public readonly PawnOutfitTracker PawnOutfitTracker;

        public ApparelLayerColorSelectionDTO(ApparelLayerDef layer, PawnOutfitTracker po) : base((po != null) ? po.GetLayerColor(layer, true) : Color.white)
        {
            this.ApparelLayerDef = layer;
            this.PawnOutfitTracker = po;
        }

        public override bool Equals(object o)
        {
            if (o != null &&
                o is ApparelLayerColorSelectionDTO)
            {
                return this.ApparelLayerDef == ((ApparelLayerColorSelectionDTO)o).ApparelLayerDef;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ("ApparelLayerColorSelectionDTO" + this.ApparelLayerDef + this.PawnOutfitTracker.Pawn.ThingID).GetHashCode();
        }
    }

    class ApparelLayerSelectionsContainer
    {
        public List<ApparelLayerColorSelectionDTO> ApparelLayerSelections { get; private set; }
        public List<SelectionColorWidgetDTO> SelectedApparel { get; private set; }
        public bool CopyColorSelected { get; private set; }
        public ColorPresetsDTO ColorPresetsDTO { get; private set; }
        public readonly PawnOutfitTracker PawnOutfitTracker;
        private Color copyColor = Color.white;

        public ApparelLayerSelectionsContainer(Pawn pawn, ColorPresetsDTO presetsDto)
        {
            PawnOutfitTracker po;
            if (!WorldComp.PawnOutfits.TryGetValue(pawn, out po))
            {
                po = new PawnOutfitTracker(pawn);
                WorldComp.PawnOutfits.Add(pawn, po);
            }
            this.PawnOutfitTracker = po;
            
            this.ApparelLayerSelections = new List<ApparelLayerColorSelectionDTO>(ChangeDresser.Util.LayerCount);
            IEnumerable<ApparelLayerDef> layers = ChangeDresser.Util.Layers;
            foreach (ApparelLayerDef layer in layers)
            {
                this.ApparelLayerSelections.Add(new ApparelLayerColorSelectionDTO(layer, this.PawnOutfitTracker));
            }
            this.SelectedApparel = new List<SelectionColorWidgetDTO>();
            this.ColorPresetsDTO = presetsDto;
            this.CopyColorSelected = false;
        }

        public Color CopyColor
        {
            get { return this.copyColor; }
            set
            {
                this.copyColor = value;
                this.CopyColorSelected = true;
            }
        }

        public int Count { get { return this.ApparelLayerSelections.Count; } }

        public ApparelLayerColorSelectionDTO this[int i]
        {
            get { return this.ApparelLayerSelections[i]; }
        }

        public void ResetToDefault()
        {
            foreach (ApparelLayerColorSelectionDTO dto in this.ApparelLayerSelections)
            {
                dto.ResetToDefault();
            }
            this.SelectedApparel.Clear();
            this.ColorPresetsDTO.Deselect();
        }

        public void DeselectAll()
        {
            this.SelectedApparel.Clear();
        }

        public void SelectAll()
        {
            this.DeselectAll();
            this.ColorPresetsDTO.Deselect();
            foreach (ApparelLayerColorSelectionDTO dto in this.ApparelLayerSelections)
            {
                this.SelectedApparel.Add(dto);
            }
        }

        public void Select(ApparelLayerColorSelectionDTO dto, bool isShiftPressed)
        {
            this.ColorPresetsDTO.Deselect();
            if (!isShiftPressed)
            {
                this.DeselectAll();
                this.SelectedApparel.Add(dto);
            }
            else
            {
                bool removed = this.SelectedApparel.Remove(dto);
                if (!removed)
                {
                    this.SelectedApparel.Add(dto);
                }
            }
        }

        public bool IsSelected(ApparelLayerColorSelectionDTO dto)
        {
            return this.SelectedApparel.Contains(dto);
        }

        internal void UpdatePawn(object sender, object value)
        {
            if (sender is ApparelColorSelectionDTO)
            {
                ApparelColorSelectionDTO apparelColorSelectDto = (ApparelColorSelectionDTO)sender;
                Apparel a = apparelColorSelectDto.Apparel;
                ApparelLayerDef layer = this.PawnOutfitTracker.GetOuterMostLayer(a);
                foreach (ApparelLayerColorSelectionDTO dto in ApparelLayerSelections)
                {
                    if (dto.ApparelLayerDef == layer)
                        dto.SelectedColor = a.DrawColor;
                    //if (a.def.apparel.layers.Contains(dto.ApparelLayerDef))
                }
            }
        }
    }
}
