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

namespace ChangeDresser.UI.DTO.StorageDTOs
{
    /// <summary>
    /// This class is not used by default. This is used to expose extra functionality for other mods - specifically ChangeDresserDraftSwitch
    /// </summary>
    public static class BattleApparelGroupDTO
    {
        private static readonly Dictionary<string, StorageGroupDTO> PawnIdBattleStorageGroups = new Dictionary<string, StorageGroupDTO>();

        public static bool ShowForceBattleSwitch { get; set; }

        static BattleApparelGroupDTO()
        {
            ShowForceBattleSwitch = false;
        }

        public static void AddBattleGroup(StorageGroupDTO dto)
        {
            StorageGroupDTO old;
            if (PawnIdBattleStorageGroups.TryGetValue(dto.RestrictToPawnId, out old))
            {
                if (old.Id == dto.Id)
                    return;
                old.SetForceSwitchBattle(false, null);
                RemoveBattleGroup(dto);
            }
            PawnIdBattleStorageGroups.Add(dto.RestrictToPawnId, dto);
        }

        public static void ClearBattleStorageCache()
        {
#if (CHANGE_DRESSER_DEBUG)
            Log.Message("BattleApparelGroupDTO.ClearBattleStorageCache()");
#endif
            PawnIdBattleStorageGroups.Clear();
        }

        public static void RemoveBattleGroup(StorageGroupDTO dto)
        {
            bool result = PawnIdBattleStorageGroups.Remove(dto.RestrictToPawnId);
        }

        public static bool TryGetBattleApparelGroupForPawn(Pawn pawn, out StorageGroupDTO storageGroupDTO)
        {
            return PawnIdBattleStorageGroups.TryGetValue(pawn.ThingID, out storageGroupDTO);
        }

        internal static bool TryGetBattleApparelGroupForPawn(string pawnId, out StorageGroupDTO storageGroupDTO)
        {
            return PawnIdBattleStorageGroups.TryGetValue(pawnId, out storageGroupDTO);
        }
    }
}
