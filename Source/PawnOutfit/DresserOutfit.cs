using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    public enum OutfitType { Battle, Civilian };

    public interface IDresserOutfit : IExposable
    {
        void Dress(Pawn pawn);
        void Undress(Pawn pawn, List<Apparel> customApparel);
        bool IsValid();
        OutfitType OutfitType { get; set; }
        string UniqueId { get; set; }
        string Label { get; }
        ThingDef Icon { get; }
        bool IsBeingWorn { get; }
    }

    public class CustomOutfit : IDresserOutfit
    {
        private OutfitType outfitType = OutfitType.Civilian;
        private bool isBeingWorn = false;
        private string uniqueId;
        public List<Apparel> Apparel = new List<Apparel>();
        public string Name = "";
        public Outfit Outfit = null;

        public CustomOutfit() { }

        public void Dress(Pawn pawn)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin CustomOutfit.Dress(Pawn: " + pawn.Name.ToStringShort + ")");
#endif

            List<Apparel> removed = ApparelUtil.RemoveApparel(pawn);

#if DRESSER_OUTFIT
            Log.Message("    Add Custom Apparel:");
#endif
            // Dress the pawn with this outfit's apparel
            foreach (Apparel a in this.Apparel)
            {
#if DRESSER_OUTFIT
                Log.Message("        " + a.Label);
#endif
                removed.Remove(a);
                pawn.apparel.Wear(a);
                // Force all apparel
                pawn.outfits.forcedHandler.ForcedApparel.Add(a);
                //tracker.RemoveCustomApparel(a);
            }

            if (this.Outfit != null)
            {
                pawn.outfits.CurrentOutfit = this.Outfit;
                ApparelUtil.OptimizeApparel(pawn);
            }

            /*/ Add any previously worn apparel that still can be worn
            for (int i = 0; i < removed.Count; ++i)
            {
                Apparel a = wasWearing[i];
                if (pawn.apparel.CanWearWithoutDroppingAnything(a.def))
                {
                    pawn.apparel.Wear(a);
                    removed[i] = null;
                }
            }*/

            ApparelUtil.StoreApparelInWorldDresser(removed, pawn);

            this.isBeingWorn = true;
#if DRESSER_OUTFIT
            Log.Warning("End CustomOutfit.Dress");
#endif
        }

        public void Undress(Pawn pawn, List<Apparel> customApparel)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin CustomOutfit.Undress(Pawn: " + pawn.Name.ToStringShort + ")");
#endif
            List<Apparel> wornApparel = new List<Apparel>(pawn.apparel.WornApparel);
            pawn.outfits.forcedHandler.ForcedApparel.Clear();
#if DRESSER_OUTFIT
            Log.Warning("    Remove Apparel:");
#endif
            foreach (Apparel a in wornApparel)
            {
#if DRESSER_OUTFIT
                Log.Warning("        " + a.Label);
#endif
                pawn.apparel.Remove(a);
                if (!customApparel.Contains(a))
                {
#if DRESSER_OUTFIT
                    Log.Warning("            -- Not a custom piece of apparel. Storing in Dresser.");
#endif
                    if (!WorldComp.AddApparel(a))
                    {
                        BuildingUtil.DropThing(a, pawn.Position, pawn.Map, false);
                    }
                }
            }
            this.isBeingWorn = false;
            /*#if DRESSER_OUTFIT
                        Log.Warning("Begin CustomOutfit.Undress(Pawn: " + pawn.Name.ToStringShort + ")");
            #endif
            #if DRESSER_OUTFIT
                        Log.Warning("    Find any changes in Custom Outfit:");
            #endif
                        LinkedList<Apparel> noLongerWearing = new LinkedList<Apparel>(this.Apparel);
                        foreach (Apparel a in pawn.apparel.WornApparel)
                        {
                            if (!noLongerWearing.Remove(a))
                            {
            #if DRESSER_OUTFIT
                                Log.Warning("        No longer wearing: " + a.Label);
            #endif
                            }
            #if DRESSER_OUTFIT
                            else
                                Log.Message("        Is still wearing: " + a.Label);
            #endif
                        }

            #if DRESSER_OUTFIT
                        if (noLongerWearing.Count > 0)
                            Log.Message("    Remove Apparel from Custom Outfit:");
            #endif

            #if DRESSER_OUTFIT
                        System.Text.StringBuilder sb = new System.Text.StringBuilder("    Tracker-CustomApparel: ");
                        foreach(Apparel a in customApparel)
                        {
                            sb.Append("[" + a.Label + "]  ");
                        }
            #endif

                        // Remove any apparel that's no longer being worn
                        foreach (Apparel a in noLongerWearing)
                        {
            #if DRESSER_OUTFIT
                            Log.Message("        " + a.Label);
            #endif
                            this.Apparel.Remove(a);
                            if (!customApparel.Contains(a))
                            {
            #if DRESSER_OUTFIT
                                Log.Message("        " + a.Label + " NOT custom apparel, going back to dresser");
            #endif
                                WorldComp.AddApparel(a);
                            }
                        }
                        pawn.outfits.forcedHandler.ForcedApparel.Clear();*/

            /*foreach (Apparel a in this.Apparel)
            {
                tracker.AddCustomApparel(a);
            }*/

#if DRESSER_OUTFIT
            Log.Warning("End CustomOutfit.Undress");
#endif
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref this.Name, "name");
            Scribe_Values.Look<OutfitType>(ref this.outfitType, "type");
            Scribe_Values.Look<bool>(ref this.isBeingWorn, "isBeingWorn", false, false);
            Scribe_Values.Look<string>(ref this.uniqueId, "uniqueId");
            Scribe_References.Look<Outfit>(ref this.Outfit, "outfit");

            Scribe_Collections.Look<Apparel>(ref this.Apparel, false, "apparel", LookMode.Reference, new object[0]);
        }

        public bool IsValid()
        {
            return true;
        }

        public string GetUniqueLoadID()
        {
            return this.uniqueId;
        }

        public OutfitType OutfitType { get { return this.outfitType; } set { this.outfitType = value; } }
        public string UniqueId { get { return this.uniqueId; } set { this.uniqueId = value; } }
        public string Label { get { return this.Name; } }
        public ThingDef Icon
        {
            get
            {
                if (this.Apparel != null && this.Apparel.Count > 0)
                {
                    return this.Apparel[0].def;
                }
                return null;
            }
        }
        public bool IsBeingWorn { get { return this.isBeingWorn; } }
    }

    public class DefinedOutfit : IDresserOutfit
    {
        private bool isBeingWorn = false;
        private string uniqueId;
        private OutfitType outfitType = OutfitType.Civilian;
        public Outfit Outfit;

        public DefinedOutfit() { }

        public DefinedOutfit(Outfit outfit, OutfitType outfitType)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin DefinedOutfit(Outfit: " + outfit.label + ", OutfitType: " + outfitType + ")");
#endif
            this.Outfit = outfit;
            this.outfitType = outfitType;
#if DRESSER_OUTFIT
            Log.Warning("End DefinedOutfit");
#endif
        }

        public void Dress(Pawn pawn)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin DefinedOutfit.Dress(Pawn: " + pawn.Name.ToStringShort + ")");
#endif
            List<Apparel> removed = ApparelUtil.RemoveApparel(pawn);
            ApparelUtil.StoreApparelInWorldDresser(removed, pawn);
            pawn.outfits.CurrentOutfit = this.Outfit;
#if DRESSER_OUTFIT
            Log.Message("     Pawn's outfit is now: " + pawn.outfits.CurrentOutfit.label);
#endif
            /*
            bool done = false;
            for(int i = 0; i < 10 && !done; ++i)
            {
                Apparel a;
                Building_Dresser d;
                if (!(done = WorldComp.TryFindBestApparel(pawn, out a, out d)))
                {
                    if (d != null)
                        d.RemoveNoDrop(a);
                    pawn.apparel.Wear(a);
                    if (customApparel.Contains(a))
                        pawn.outfits.forcedHandler.ForcedApparel.Add(a);
                }
            }
            */
            ApparelUtil.OptimizeApparel(pawn);
            this.isBeingWorn = true;
#if DRESSER_OUTFIT
            Log.Warning("End DefinedOutfit.Dress");
#endif
        }

        public void Undress(Pawn pawn, List<Apparel> customApparel)
        {
#if DRESSER_OUTFIT
Log.Warning("Begin DefinedOutfit.Undress(Pawn: " + pawn.Name.ToStringShort + ")");
#endif
            List<Apparel> wornApparel = new List<Apparel>(pawn.apparel.WornApparel);
            pawn.outfits.forcedHandler.ForcedApparel.Clear();
#if DRESSER_OUTFIT
Log.Warning("    Remove Apparel:");
#endif
            foreach (Apparel a in wornApparel)
            {
#if DRESSER_OUTFIT
    Log.Warning("        " + a.Label);
#endif
                pawn.apparel.Remove(a);
                if (!customApparel.Contains(a))
                {
#if DRESSER_OUTFIT
        Log.Warning("            -- Not a custom piece of apparel. Storing in Dresser.");
#endif
                    if (!WorldComp.AddApparel(a))
                    {
                        BuildingUtil.DropThing(a, pawn.Position, pawn.Map, false);
                    }
                }
            }
            this.isBeingWorn = false;
#if DRESSER_OUTFIT
Log.Warning("End DefinedOutfit.Undress");
#endif
        }

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.isBeingWorn, "isBeingWorn", false, false);
            Scribe_Values.Look<OutfitType>(ref this.outfitType, "type");
            Scribe_References.Look<Outfit>(ref this.Outfit, "outfit");
            Scribe_Values.Look<string>(ref this.uniqueId, "uniqueId");
        }

        public bool IsValid()
        {
            return this.Outfit != null;
        }

        public string GetUniqueLoadID()
        {
            return this.uniqueId;
        }

        public OutfitType OutfitType { get { return this.outfitType; } set { this.outfitType = value; } }
        public string UniqueId { get { return this.uniqueId; } set { this.uniqueId = value; } }
        public string Label
        {
            get
            {
                if (this.Outfit != null)
                {
                    return this.Outfit.label;
                }
                else
                {
                    return "[Deleted Outfit]";
                }
            }
        }
        public ThingDef Icon
        {
            get
            {
                if (this.Outfit != null)
                {
                    List<ThingDef> d = new List<ThingDef>(this.Outfit.filter.AllowedThingDefs);
                    if (d.Count > 0)
                    {
                        return d[0];
                    }
                }
                return null;
            }
        }
        public bool IsBeingWorn { get { return this.isBeingWorn; } }
    }
}