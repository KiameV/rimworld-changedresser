using ChangeDresser.StoredApparel;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    partial class Building_Dresser
    {
        private List<StorageGroupDTO> groups = null;
        private List<StoredApparelSet> GetStoredApparelSetsFromStorageGroupDTOs()
        {
            List<StoredApparelSet> storedApparelSets = new List<StoredApparelSet>();
            if (groups != null && groups.Count > 0)
            {
                foreach (StorageGroupDTO group in groups)
                {
                    StoredApparelSet set = new StoredApparelSet(this, group.restrictToPawnId, group.isBeingWornById);
                    set.SetApparel(group.apparelList);
                    if (group.apparelList.Count == group.forcedApparel.Count)
                    {
                        List<string> forcedApparelIds = new List<string>();
                        for (int i = 0; i < forcedApparelIds.Count; ++i)
                        {
                            if (group.forcedApparel[i])
                            {
                                forcedApparelIds.Add(group.apparelList[i].ThingID);
                            }
                        }
                        set.SetForcedApparel(forcedApparelIds);
                    }
                    set.Name = group.name;
                    set.SwitchForBattle = group.forceSwitchBattle;
                    storedApparelSets.Add(set);
                }
            }
            return storedApparelSets;
        }

        private class StorageGroupDTO : IExposable
        {
            public List<Apparel> apparelList = new List<Apparel>();
            public List<bool> forcedApparel = new List<bool>();
            public string name = "";
            public string restrictToPawnId = "";
            public string restrictToPawnName = "";
            public bool forceSwitchBattle = false;
            public string isBeingWornById = "";
            public string isBeingWornByName = "";

            public void ExposeData()
            {
                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    Scribe_Values.Look<string>(ref this.name, "name", "", false);
                    Scribe_Values.Look<string>(ref this.restrictToPawnId, "restrictToPawnId", "", false);
                    Scribe_Values.Look<string>(ref this.restrictToPawnName, "restrictToPawnName", "", false);
                    Scribe_Values.Look<bool>(ref this.forceSwitchBattle, "forceSwitchBattle", false, false);
                    Scribe_Values.Look<string>(ref this.isBeingWornById, "isBeingWornById", "", false);
                    Scribe_Values.Look<string>(ref this.isBeingWornByName, "isBeingWornByName", "", false);
                    Scribe_Collections.Look(ref this.apparelList, "apparelList", LookMode.Deep, new object[0]);

                    List<bool> l = new List<bool>();
                    Scribe_Collections.Look(ref l, "forcedApparel", LookMode.Value, new object[0]);

                    if (l == null || l.Count < this.apparelList.Count)
                    {
                        this.forcedApparel = new List<bool>(this.apparelList.Count);
                        for (int i = 0; i < this.apparelList.Count; ++i)
                        {
                            this.forcedApparel.Add(false);
                        }
                    }
                    else
                    {
                        this.forcedApparel = new List<bool>(l);
                    }
                }
            }
        }
    }
}
