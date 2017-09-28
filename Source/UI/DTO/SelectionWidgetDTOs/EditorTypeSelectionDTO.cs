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
using ChangeDresser.UI.Enums;
using System;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class EditorTypeSelectionDTO : ASelectionWidgetDTO
    {
        private List<CurrentEditorEnum> editors;
        public EditorTypeSelectionDTO(CurrentEditorEnum currentEditor, List<CurrentEditorEnum> editors) : base()
        {
            this.editors = new List<CurrentEditorEnum>(editors);

            this.SetSelectedEditor(currentEditor);
        }

        public override int Count
        {
            get
            {
                return this.editors.Count;
            }
        }

        public override string SelectedItemLabel
        {
            get
            {
                return this.editors[base.index].ToString().Translate();
            }
        }

        public override object SelectedItem
        {
            get
            {
                return this.editors[base.index];
            }
        }

        public override void ResetToDefault()
        {
            // Do nothing
        }

        internal bool Contains(CurrentEditorEnum currentEditorEnum)
        {
            return this.editors.Contains(currentEditorEnum);
        }

        internal void Remove(params CurrentEditorEnum[] toRemove)
        {
            foreach (CurrentEditorEnum ed in toRemove)
            {
                this.editors.Remove(ed);
            }
            base.index = 0;
        }

        public void SetSelectedEditor(CurrentEditorEnum editor)
        {
            base.index = 0;
            for (int i = 0; i < this.editors.Count; ++i)
            {
                if (this.editors[i] == editor)
                {
                    base.index = i;
                    break;
                }
            }
        }
    }
}
