using RimWorld;
using Verse;

namespace ChangeDresser
{
    class OutfitType : IExposable
    {
        public Outfit Outfit;
        public bool ForBattle = false;

        public void ExposeData()
        {
            Scribe_References.Look(ref this.Outfit, "outfit");
            Scribe_Values.Look(ref this.ForBattle, "forBattle", false, false);
        }
    }
}
