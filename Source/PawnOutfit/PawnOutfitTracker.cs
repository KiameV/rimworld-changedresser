using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System;

namespace ChangeDresser
{
    public class PawnOutfitTracker : IExposable
    {
        public Pawn Pawn = null;
        private List<Apparel> customApparel = new List<Apparel>();
		public IEnumerable<Apparel> CustomApparel => customApparel;

        public List<DefinedOutfit> DefinedOutfits = new List<DefinedOutfit>();
        public List<CustomOutfit> CustomOutfits = new List<CustomOutfit>();

        private string currentlyWorn = null;
        private string lastBattleOutfit = null;
        private string lastCivilianOutfit = null;

        private List<SlotColor> ApparelColors = null;

        public PawnOutfitTracker() { }

        public PawnOutfitTracker(Pawn pawn)
        {
            this.Pawn = pawn;
        }

        public void AddOutfit(IDresserOutfit outfit)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.AddOutfit(IDresserOutfit: " + outfit.Label + ")");
#endif
            this.ApplyUniqueId(outfit);
            if (outfit is DefinedOutfit)
            {
                bool found = false;
                foreach (DefinedOutfit o in this.DefinedOutfits)
                {
                    if (o.UniqueId.Equals(outfit.UniqueId))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    this.DefinedOutfits.Add(outfit as DefinedOutfit);
#if DRESSER_OUTFIT
                    Log.Message("    Adding to DefinedOutfits -- New Count " + DefinedOutfits.Count);
#endif
                }
#if DRESSER_OUTFIT
                else
                {
                    Log.Message("    Outfit is already being tracked");
                }
#endif
            }
            else
            {
                bool found = false;
                foreach (CustomOutfit o in this.CustomOutfits)
                {
                    if (o.UniqueId.Equals(outfit.UniqueId))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    this.CustomOutfits.Add(outfit as CustomOutfit);
#if DRESSER_OUTFIT
                    Log.Message("    Adding to CustomOutfits -- New Count " + DefinedOutfits.Count);
#endif
                }
#if DRESSER_OUTFIT
                else
                {
                    Log.Message("    Outfit is already being tracked");
                }
#endif
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.AddOutfit");
#endif
        }

		public void Clean()
		{
			bool found = false;
			for (int i = this.customApparel.Count - 1; i >= 0; --i)
			{
				Apparel a = this.customApparel[i];
				if (a == null || a.Destroyed || a.HitPoints <= 0)
				{
					found = true;
					customApparel.RemoveAt(i);
				}
			}

			if (!found)
				return;

			foreach (CustomOutfit c in CustomOutfits)
				c.Clean();
		}

		public Color GetLayerColor(ApparelLayerDef layer, bool getFromWorn = false)
        {
            int layerInt = Util.ToInt(layer);
            if (this.ApparelColors != null && this.ApparelColors.Count > layerInt)
            {
                SlotColor sc = this.ApparelColors[layerInt];
                if (sc.IsAssigned)
                {
                    return sc.Color;
                }
            }
            if (getFromWorn)
            {
                foreach (Apparel a in this.Pawn.apparel.WornApparel)
                {
                    if (a.def.apparel.LastLayer == layer)
                        return a.DrawColor;
                }
            }
            return Color.white;
        }

        public void SetLayerColor(ApparelLayerDef layer, Color c)
        {
            int layerInt = Util.ToInt(layer);
            this.InitApparelColor();
            if (this.ApparelColors != null && this.ApparelColors.Count > layerInt)
            {
                SlotColor sc = this.ApparelColors[layerInt];
                sc.Color = c;
                sc.IsAssigned = true;
                this.ApparelColors[layerInt] = sc;
            }
        }

        private void ApplyUniqueId(IDresserOutfit outfit)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.ApplyUniqueId(IDresserOutfit: " + outfit.Label + " " + outfit.UniqueId + ")");
#endif
            if (string.IsNullOrEmpty(outfit.UniqueId))
            {
                outfit.UniqueId = ((outfit is DefinedOutfit) ? "d" : "c") + WorldComp.NextDresserOutfitId;
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.ApplyUniqueId -> " + outfit.UniqueId);
#endif
        }

        public void ChangeTo(IDresserOutfit outfit)
        {
            if (string.IsNullOrEmpty(outfit.UniqueId))
            {
                this.ApplyUniqueId(outfit);
            }

#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.ChangeTo(IDresserOutfit: " + outfit.Label + ")");
#endif
            if (currentlyWorn != null)
            {
#if DRESSER_OUTFIT
                Log.Message("    Currently Worn: " + currentlyWorn);
#endif
                IDresserOutfit o = this.GetOutfit(this.currentlyWorn);
                if (o != null)
                {
#if DRESSER_OUTFIT
                    Log.Message("    Currently Worn Found: " + o.Label + ". Undress");
#endif
                    o.Undress(this.Pawn, this.customApparel);
                }
#if DRESSER_OUTFIT
                else
                {
                    Log.Warning("    Currently Worn NOT Found");
                }
#endif
            }

#if DRESSER_OUTFIT
            Log.Message("    Dress into: " + outfit.Label);
#endif
            outfit.Dress(this.Pawn);

            if (outfit.OutfitType == OutfitType.Battle)
            {
#if DRESSER_OUTFIT
                Log.Message("    Set Last Battle Outfit to: " + outfit.Label + " " + outfit.UniqueId);
#endif
                this.lastBattleOutfit = outfit.UniqueId;
            }
            else // OutfitType.Civilian
            {
#if DRESSER_OUTFIT
                Log.Message("    Set Last Civilian Outfit to: " + outfit.Label + " " + outfit.UniqueId);
#endif
                this.lastCivilianOutfit = outfit.UniqueId;
            }
            this.currentlyWorn = outfit.UniqueId;
#if DRESSER_OUTFIT
            Log.Message("    Currently Worn is now: " + currentlyWorn);
#endif
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.ChangeTo(uniqueId: " + outfit.UniqueId + ")");
#endif
        }

        public bool ChangeToBattleOutfit()
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.ChangeToBattleOutfit");
#endif
            if (this.lastBattleOutfit != null)
            {
#if DRESSER_OUTFIT
                Log.Message("    Last Battle Outfit: " + lastBattleOutfit);
#endif
                IDresserOutfit o = this.GetOutfit(this.lastBattleOutfit);
                if (o != null && o.OutfitType == OutfitType.Battle)
                {
                    this.ChangeTo(o);
#if DRESSER_OUTFIT
                    Log.Warning("End PawnOutfitTracker.ChangeToBattleOutfit -> True");
#endif
                    return true;
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.ChangeToBattleOutfit -> ChangeToOutfitType(OutfitType.Battle)");
#endif
            return ChangeToOutfitType(OutfitType.Battle);
        }

        public bool ChangeToCivilianOutfit()
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.ChangeToCivilianOutfit");
#endif
            if (this.lastCivilianOutfit != null)
            {
#if DRESSER_OUTFIT
                Log.Message("    Last Civilian Outfit: " + lastCivilianOutfit);
#endif
                IDresserOutfit o = this.GetOutfit(this.lastCivilianOutfit);
                if (o != null && o.OutfitType == OutfitType.Civilian)
                {
                    this.ChangeTo(o);
#if DRESSER_OUTFIT
                    Log.Warning("End PawnOutfitTracker.ChangeToCivilianOutfit -> True");
#endif
                    return true;
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.ChangeToCivilianOutfit -> ChangeToOutfitType(OutfitType.Civilian)");
#endif
            return ChangeToOutfitType(OutfitType.Civilian);
        }

        private IDresserOutfit GetOutfit(string uniqueId)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.GetOutfit(uniqueId: " + uniqueId + ")");
#endif
            if (!string.IsNullOrEmpty(uniqueId))
            {
                if (uniqueId[0] == 'c')
                {
#if DRESSER_OUTFIT
                    Log.Message("    Search CustomOutfits Count " + CustomOutfits.Count);
#endif
                    foreach (IDresserOutfit o in this.CustomOutfits)
                    {
#if DRESSER_OUTFIT
                        Log.Message("        " + o.Label + " " + o.UniqueId);
#endif
                        if (uniqueId.Equals(o.UniqueId))
                        {
#if DRESSER_OUTFIT
                            Log.Warning("End PawnOutfitTracker.GetOutfit -> " + o.Label);
#endif
                            return o;
                        }
                    }
                }
                else
                {
#if DRESSER_OUTFIT
                    Log.Message("    Search DefinedOutfits Count " + DefinedOutfits.Count);
#endif
                    foreach (IDresserOutfit o in this.DefinedOutfits)
                    {
#if DRESSER_OUTFIT
                        Log.Message("        " + o.Label + " " + o.UniqueId);
#endif
                        if (uniqueId.Equals(o.UniqueId))
                        {
#if DRESSER_OUTFIT
                            Log.Warning("End PawnOutfitTracker.GetOutfit -> " + o.Label);
#endif
                            return o;
                        }
                    }
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.GetOutfit -> null");
#endif
            return null;
        }

        private bool ChangeToOutfitType(OutfitType type)
        {
#if DRESSER_OUTFIT
            Log.Warning("Start PawnOutfitTracker.ChangeToOutfitType(OutfitType: " + type + ")");
            Log.Message("    Search CustomOutfits Count " + CustomOutfits.Count);
#endif
            foreach (IDresserOutfit outfit in this.CustomOutfits)
            {
#if DRESSER_OUTFIT
                Log.Message("        " + outfit.Label);
#endif
                if (outfit.OutfitType == type)
                {
#if DRESSER_OUTFIT
                    Log.Message("            -> ChangeTo");
#endif
                    this.ChangeTo(outfit);
#if DRESSER_OUTFIT
                    Log.Warning("End PawnOutfitTracker.ChangeToOutfitType -> True");
#endif
                    return true;
                }
            }
#if DRESSER_OUTFIT
            Log.Message("    Search DefinedOutfits Count " + DefinedOutfits.Count);
#endif
            foreach (IDresserOutfit outfit in this.DefinedOutfits)
            {
#if DRESSER_OUTFIT
                Log.Message("        " + outfit.Label);
#endif
                if (outfit.OutfitType == type)
                {
#if DRESSER_OUTFIT
                    Log.Message("            -> ChangeTo");
#endif
                    this.ChangeTo(outfit);
#if DRESSER_OUTFIT
                    Log.Warning("End PawnOutfitTracker.ChangeToOutfitType -> True");
#endif
                    return true;
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.ChangeToOutfitType -> False");
#endif
            return false;
        }

        public void InitApparelColor()
        {
            if (this.ApparelColors == null || this.ApparelColors.Count == 0)
            {
                int size = Util.LayerCount;
                this.ApparelColors = new List<SlotColor>(size);
                for (int i = 0; i < size; ++i)
                {
                    this.ApparelColors.Add(new SlotColor());
                }
            }
        }

        public ApparelLayerDef GetOuterMostLayer(Apparel a)
        {
            int layer = Util.ToInt(ApparelLayerDefOf.OnSkin);
            foreach (ApparelLayerDef l in a.def.apparel.layers)
            {
                int i = Util.ToInt(l);
                if (i >= layer)
                {
                    layer = i;
                }
            }
            return Util.ToLayer(layer);
        }

        public void ApplyApparelColor(Apparel a)
        {
#if DEBUG || DEBUG_APPAREL_COLOR
            Log.Warning("Start PawnOutfitTracker.ApplyApparelColor (Apparel: " + a.Label + ")");
#endif
            int layer = Util.ToInt(this.GetOuterMostLayer(a));
            if (this.ApparelColors != null)
            {
                if (this.ApparelColors.Count > layer)
                {
                    SlotColor slotColor = this.ApparelColors[layer];
                    CompColorableUtility.SetColor(a, slotColor.Color, true);
                }
                else if (this.ApparelColors.Count <= layer)
                {
                    Log.Warning($"[Change Dresser] unable to find top layer for apparel for layer " + this.GetOuterMostLayer(a).defName);
                }
            }
#if DEBUG || DEBUG_APPAREL_COLOR
            Log.Warning("End PawnOutfitTracker.ApplyApparelColor (Apparel: " + a.Label + ")");
#endif
        }

        public void SetApparelColor(Apparel a, Color color)
        {
#if DEBUG || DEBUG_APPAREL_COLOR
            Log.Warning("Start PawnOutfitTracker.SetApparelColor (Apparel: " + a.Label + " Color: " + color + ")");
#endif
            this.InitApparelColor();
            int layer = Util.ToInt(this.GetOuterMostLayer(a));
            SlotColor slotColor = this.ApparelColors[(int)layer];
            slotColor.IsAssigned = true;
            slotColor.Color = color;
            this.ApparelColors[(int)layer] = slotColor;
            /*foreach (ApparelLayerDef layer in a.def.apparel.layers)
            {
#if DEBUG || DEBUG_APPAREL_COLOR
                Log.Warning("    Setting layer " + layer);
#endif
                SlotColor slotColor = this.ApparelColors[(int)layer];
                slotColor.IsAssigned = true;
                slotColor.Color = color;
                this.ApparelColors[(int)layer] = slotColor;
            }*/
#if DEBUG || DEBUG_APPAREL_COLOR
            Log.Warning("End PawnOutfitTracker.SetApparelColor (Apparel: " + a.Label + " Color: " + color + ")");
#endif
        }

        public bool Contains(Outfit outfit)
        {
            foreach (DefinedOutfit o in this.DefinedOutfits)
            {
                if (o.Outfit == outfit)
                    return true;
            }
            return false;
        }

        public void Remove(Outfit outfit)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.Remove(Outfit: " + outfit.label + ")");
#endif
            for (int i = 0; i < this.DefinedOutfits.Count; ++i)
            {
                if (this.DefinedOutfits[i].Outfit == outfit)
                {
                    this.DefinedOutfits.RemoveAt(i);
                    return;
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.Remove");
#endif
        }

        public bool Remove(IDresserOutfit outfit)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.Remove(IDresserOutfit: " + outfit.Label + " " + outfit.UniqueId + ")");
#endif
            if (outfit is DefinedOutfit)
            {
                foreach (DefinedOutfit o in this.DefinedOutfits)
                {
                    if (o.UniqueId.Equals(outfit.UniqueId))
                    {
                        this.DefinedOutfits.Remove(o);
#if DRESSER_OUTFIT
                        Log.Warning("End PawnOutfitTracker.Remove -- True (DefinedOutfit removed)");
#endif
                        return true;
                    }
                }
            }
            else
            {
                foreach (CustomOutfit o in this.CustomOutfits)
                {
                    if (o.UniqueId.Equals(outfit.UniqueId))
                    {
                        this.CustomOutfits.Remove(o);
#if DRESSER_OUTFIT
                        Log.Warning("End PawnOutfitTracker.Remove -- True (CustomOutfits removed)");
#endif
                        return true;
                    }
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.Remove -- False");
#endif
            return false;
        }

        public void AddCustomApparel(Apparel apparel)
        {
            if (!this.customApparel.Contains(apparel))
                this.customApparel.Add(apparel);
        }

        public bool ContainsCustomApparel(Apparel apparel)
        {
            return this.customApparel.Contains(apparel);
        }

        public void RemoveCustomApparel(Apparel apparel)
        {
            this.customApparel.Remove(apparel);
        }

        public void UpdateOutfitType(Outfit outfit, OutfitType outfitType)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.UpdateOutfitType(Outfit: " + outfit.label + " OutfitType: " + outfitType + ")");
#endif
            foreach (DefinedOutfit o in this.DefinedOutfits)
            {
#if DRESSER_OUTFIT
                Log.Warning("    Outfit: " + o.Label);
#endif
                if (o.Outfit == outfit)
                {
#if DRESSER_OUTFIT
                    Log.Warning("        Found - Updating OutfitType");
#endif
                    o.OutfitType = outfitType;
                    break;
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.UpdateOutfitType");
#endif
        }

        public void UpdateCustomApparel(Building_Dresser dresser)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin PawnOutfitTracker.UpdateCustomApparel(Dresser: " + ((dresser == null) ? "<null>" : dresser.Label) + ")");
#endif
            List<Apparel> stored = new List<Apparel>(this.customApparel);
            this.customApparel.Clear();
            
            foreach (CustomOutfit o in this.CustomOutfits)
            {
                foreach (Apparel a in o.Apparel)
                {
                    if (!this.customApparel.Contains(a))
                    {
#if DRESSER_OUTFIT
                        Log.Message("    Add CustomApparel: " + a.Label);
#endif
                        this.customApparel.Add(a);
                    }
                    stored.Remove(a);
                }
            }

            foreach(Apparel a in stored)
            {
#if DRESSER_OUTFIT
                Log.Message("    No Longer Used: " + a.Label);
#endif
                if (!WorldComp.AddApparel(a))
                {
                    if (dresser == null)
                    {
                        Log.Error("Unable to drop " + a.Label + " on ground.");
                    }
                    else
                    {
                        BuildingUtil.DropThing(a, dresser, dresser.Map, false);
                    }
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End PawnOutfitTracker.UpdateCustomApparel");
#endif
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (Apparel a in this.Pawn.apparel.WornApparel)
                    this.customApparel.Remove(a);

				this.Clean();
			}

			Scribe_References.Look(ref this.Pawn, "pawn");
            Scribe_Collections.Look(ref this.DefinedOutfits, "definedOutfits", LookMode.Deep, new object[0]);
            Scribe_Collections.Look(ref this.CustomOutfits, "customOutfits", LookMode.Deep, new object[0]);
            Scribe_Collections.Look(ref this.ApparelColors, "apparelColors", LookMode.Deep, new object[0]);
            Scribe_Collections.Look(ref this.customApparel, false, "customApparel", LookMode.Deep, new object[0]);

            Scribe_Values.Look(ref this.currentlyWorn, "currentlyWorn");
            Scribe_Values.Look(ref this.lastBattleOutfit, "lastBattleOutfit");
            Scribe_Values.Look(ref this.lastCivilianOutfit, "lastCivilianOutfit");
			
            if (Scribe.mode == LoadSaveMode.Saving || 
                Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.UpdateCustomApparel(null);
            }

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.Clean();
			}

		}

        public IEnumerable<IDresserOutfit> AllOutfits
        {
            get
            {
                foreach (IDresserOutfit o in this.DefinedOutfits)
                {
                    yield return o;
                }
                foreach (IDresserOutfit o in this.CustomOutfits)
                {
                    yield return o;
                }
            }
        }

        public IEnumerable<IDresserOutfit> BattleOutfits
        {
            get
            {
                foreach (IDresserOutfit o in this.DefinedOutfits)
                {
                    if (o.OutfitType == OutfitType.Battle)
                        yield return o;
                }
                foreach (IDresserOutfit o in this.CustomOutfits)
                {
                    if (o.OutfitType == OutfitType.Battle)
                        yield return o;
                }
            }
        }

        public IEnumerable<IDresserOutfit> CivilianOutfits
        {
            get
            {
                foreach (IDresserOutfit o in this.DefinedOutfits)
                {
                    if (o.OutfitType == OutfitType.Civilian)
                        yield return o;
                }
                foreach (IDresserOutfit o in this.CustomOutfits)
                {
                    if (o.OutfitType == OutfitType.Civilian)
                        yield return o;
                }
            }
        }

        public IDresserOutfit CurrentOutfit
        {
            get
            {
                return this.GetOutfit(this.currentlyWorn);
            }
        }

        public void ClearApparelColors()
        {
            if (this.ApparelColors != null)
            {
                this.ApparelColors.Clear();
                this.ApparelColors = null;
            }
        }

        public bool HasApparelColors()
        {
            return this.ApparelColors != null && this.ApparelColors.Count > 0;
        }
    }
}