using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ChangeDresser
{
    public class ApparelColorTracker : IExposable
    {
        private Dictionary<Apparel, ApparelColor> colors = null;

        public void Clear()
        {
            if (this.colors != null)
            {
                this.colors.Clear();
                this.colors = null;
            }
        }

        public void PersistWornColors()
        {
#if APPAREL_COLOR_TRACKER
            Log.Warning("Begin ApparelColorTracker.PersistWornColors");
#endif
            if (Settings.PersistApparelOriginalColor)
            {
                if (this.colors == null)
                {
                    this.colors = new Dictionary<Apparel, ApparelColor>();
                }

                foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Colonists)
                {
                    if (p.Faction == Faction.OfPlayer && p.def.race.Humanlike)
                    {
#if APPAREL_COLOR_TRACKER
                        Log.Message("    Pawn: " + p.Name.ToStringShort);
#endif
                        foreach (Apparel a in p.apparel.WornApparel)
                        {
                            this.PersistColor(a);
                        }
                    }
                }
            }
#if APPAREL_COLOR_TRACKER
            Log.Warning("End ApparelColorTracker.PersistWornColors");
#endif
        }

        public void PersistColor(Apparel a)
        {
#if APPAREL_COLOR_TRACKER
            Log.Warning("Begin ApparelColorTracker.PersistColor Apparel: " + a.Label);
#endif
            if (Settings.PersistApparelOriginalColor)
            {
#if TRACE && APPAREL_COLOR_TRACKER
                Log.Message("    Persist Color");
#endif
                if (colors == null)
                {
#if TRACE && APPAREL_COLOR_TRACKER
                    Log.Message("    Create list");
#endif
                    colors = new Dictionary<Apparel, ApparelColor>();
                }

                ApparelColor ac;
                if (!this.colors.TryGetValue(a, out ac))
                {
#if TRACE && APPAREL_COLOR_TRACKER
                    Log.Message("    Add Color");
#endif
                    colors.Add(a, new ApparelColor(a));
                }
#if TRACE && APPAREL_COLOR_TRACKER
                else
                {
                    Log.Message("    Don't update color");
                }
#endif
            }
#if APPAREL_COLOR_TRACKER
            Log.Warning("End ApparelColorTracker.TryGetApparelColor");
#endif
        }

        public void ResetColor(Apparel a)
        {
#if APPAREL_COLOR_TRACKER
            Log.Warning("Begin ApparelColorTracker.ResetColor " + a.Label);
#endif
            if (this.colors != null)
            {
                ApparelColor ac;
                if (this.colors.TryGetValue(a, out ac))
                {
#if TRACE && APPAREL_COLOR_TRACKER
                Log.Message("    Resetting Color to " + ac.Color);
#endif
                    this.colors.Remove(a);
                    a.DrawColor = ac.Color;
                }
            }
#if APPAREL_COLOR_TRACKER
            Log.Warning("End ApparelColorTracker.ResetColor New Color: " + a.DrawColor);
#endif
        }

        private List<ApparelColor> l = null;
        public void ExposeData()
        {
#if APPAREL_COLOR_TRACKER
            Log.Warning("Begin ApparelColorTracker.Expose " + Scribe.mode);
#endif
            if (Scribe.mode == LoadSaveMode.Saving)
            {
#if TRACE && APPAREL_COLOR_TRACKER
                Log.Message("    Saving. Create the list:");
#endif
                if (this.colors != null)
                {
                    l = new List<ApparelColor>(this.colors.Count);
                    foreach (ApparelColor ac in this.colors.Values)
                    {
#if TRACE && APPAREL_COLOR_TRACKER
                        Log.Message("        Apparel: " + ac.Apparel.Label + " Destroyed: " + ac.Apparel.Destroyed + " Color: " + ac.Color);
#endif
                        if (ac.Apparel != null && !ac.Apparel.Destroyed)
                        {
#if TRACE && APPAREL_COLOR_TRACKER
                            Log.Message("            Added");
#endif
                            this.l.Add(ac);
                        }
#if TRACE && APPAREL_COLOR_TRACKER
                        else
                        {
                            Log.Message("        Not added");
                        }
#endif
                    }
                }
                else
                {
#if TRACE && APPAREL_COLOR_TRACKER
                    Log.Message("    No apparel to add. Empty list.");
#endif
                    l = new List<ApparelColor>(0);
                }
            }

            Scribe_Collections.Look(ref this.l, "ApparelColors", LookMode.Deep, new object[0]);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
#if TRACE && APPAREL_COLOR_TRACKER
                Log.Message("    Populate Colors");
#endif
                this.Clear();

                if (Settings.PersistApparelOriginalColor)
                {
                    if (this.l != null)
                    {
                        if (this.colors == null)
                        {
                            this.colors = new Dictionary<Apparel, ApparelColor>();
                        }
                        
#if TRACE && APPAREL_COLOR_TRACKER
                        Log.Message("    ApparelColors to add: " + this.l.Count);
#endif
                        foreach (ApparelColor ac in this.l)
                        {
                            if (ac != null)
                            {
#if TRACE && APPAREL_COLOR_TRACKER
                                Log.Message("        " + ac.Apparel.Label + " " + ac.Color);
#endif
                                this.colors.Add(ac.Apparel, ac);
                            }
                        }
                    }
#if TRACE && APPAREL_COLOR_TRACKER
                    else
                        Log.Message("    No ApparelColors to add");
#endif
                }
            }
#if TRACE && APPAREL_COLOR_TRACKER
            Log.Message("        Colors: " + ((this.colors == null) ? "<null>" : this.colors.Count.ToString()));
#endif

            if (Scribe.mode == LoadSaveMode.Saving || 
                Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (l != null)
                {
                    l.Clear();
                    l = null;
                }
            }
#if APPAREL_COLOR_TRACKER
            Log.Warning("End ApparelColorTracker.Expose");
#endif
        }

        private class ApparelColor : IExposable
        {
            public Apparel Apparel = null;
            public Color Color = Color.white;

            public ApparelColor() { }

            public ApparelColor(Apparel a)
            {
                this.Apparel = a;
                this.Color = a.DrawColor;
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref this.Color, "color", Color.white, true);
                Scribe_References.Look(ref this.Apparel, "apparel");
            }
        }
    }
}
