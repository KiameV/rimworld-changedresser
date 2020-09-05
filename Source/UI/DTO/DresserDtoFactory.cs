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
using System.Collections.Generic;
using Verse;
using Verse.AI;
using ChangeDresser.UI.Enums;

namespace ChangeDresser.UI.DTO
{
    static class DresserDtoFactory
    {
        public static DresserDTO Create(Pawn pawn, Job job, CurrentEditorEnum selectedEditor)
        {
            bool isAlien = AlienRaceUtil.IsAlien(pawn);
            IEnumerable<CurrentEditorEnum> editors;
            if (job == null || job.targetA.Thing is Building_Dresser)
            {
                editors = Building_Dresser.GetSupportedEditors(isAlien);
            }
            else
            {
                editors = Building_ChangeMirror.GetSupportedEditors(isAlien);
            }

            if (isAlien)
            {
                return new AlienDresserDTO(pawn, selectedEditor, editors);
            }
            return new DresserDTO(pawn, selectedEditor, editors);
        }
    }
}
