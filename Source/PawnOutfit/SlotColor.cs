using UnityEngine;
using Verse;

namespace ChangeDresser
{
    class SlotColor : IExposable
    {
        public bool IsAssigned = false;
        public Color Color = Color.white;

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.IsAssigned, "isAssigned");
            Scribe_Values.Look<Color>(ref this.Color, "color");
        }
    }
}
