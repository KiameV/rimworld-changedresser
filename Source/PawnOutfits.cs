using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    class PawnOutfits : IExposable
    {
        public List<Outfit> Outfits = new List<Outfit>();
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
            foreach (Outfit o in this.Outfits)
            {
                if (WorldComp.OutfitsForBattle.Contains(o))
                {
                    outfit = o;
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
            foreach (Outfit o in this.Outfits)
            {
                if (!WorldComp.OutfitsForBattle.Contains(o))
                {
                    outfit = o;
                    return true;
                }
            }
            outfit = null;
            return false;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref this.Pawn, "pawn");
            Scribe_Collections.Look(ref this.Outfits, "outfits", LookMode.Reference, new object[0]);
            Scribe_References.Look(ref this.lastBattleOutfit, "lastBattleOutfit");
            Scribe_References.Look(ref this.lastCivilianOutfit, "lastCivilianOutfit");

            if (Scribe.mode == LoadSaveMode.PostLoadInit &&
                this.Outfits == null)
            {
                this.Outfits = new List<Outfit>(0);
            }
        }
    }
}
