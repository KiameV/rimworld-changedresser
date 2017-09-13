using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    class PawnOutfits : IExposable
    {
        public List<OutfitType> OutfitTypes = new List<OutfitType>();
        public Pawn Pawn = null;

        private Outfit lastBattleOutfit = null;
        public Outfit LastBattleOutfit { set { this.lastBattleOutfit = value; } }

        private Outfit lastCivilianOutfit = null;
        public Outfit LastCivilianOutfit { set { this.lastCivilianOutfit = value; } }

        public bool TryGetBattleOutfit(out Outfit outfit)
        {
            if (this.lastBattleOutfit != null)
            {
                outfit = this.lastBattleOutfit;
                return true;
            }
            foreach (OutfitType o in this.OutfitTypes)
            {
                if (o.ForBattle)
                {
                    outfit = o.Outfit;
                    return true;
                }
            }
            outfit = null;
            return false;
        }

        public bool TryGetCivilianOutfit(out Outfit outfit)
        {
            if (this.lastCivilianOutfit != null)
            {
                outfit = this.lastCivilianOutfit;
                return true;
            }
            foreach (OutfitType o in this.OutfitTypes)
            {
                if (o.ForBattle)
                {
                    outfit = o.Outfit;
                    return true;
                }
            }
            outfit = null;
            return false;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref this.Pawn, "pawn");
            Scribe_Collections.Look(ref this.OutfitTypes, "outfitTypes", LookMode.Deep, new object[0]);
            Scribe_References.Look(ref this.lastBattleOutfit, "lastBattleOutfit");
            Scribe_References.Look(ref this.lastCivilianOutfit, "lastCivilianOutfit");

            if (Scribe.mode == LoadSaveMode.PostLoadInit &&
                this.OutfitTypes == null)
            {
                this.OutfitTypes = new List<OutfitType>(0);
            }
        }
    }
}
