using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace ChangeDresser
{
    class Building_RepairChangeDresser : Building
    {
        private const int LOW_POWER_COST = 10;
        //private const int RARE_TICKS_PER_HP = 4;

        private LinkedList<Building_Dresser> AttachedDressers = new LinkedList<Building_Dresser>();
        public CompPowerTrader compPowerTrader;

        private Apparel BeingRepaird = null;
        private Map CurrentMap;
        //private int rareTickCount = 0;

        public override string GetInspectString()
        {
            //this.Tick();
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.Append("\n");
            sb.Append("ChangeDresser.AttachedDressers".Translate());
            sb.Append(": ");
            sb.Append(this.AttachedDressers.Count);
            sb.Append("\n");
            sb.Append("ChangeDresser.IsMending".Translate());
            sb.Append(": ");
            if (BeingRepaird == null)
                sb.Append(Boolean.FalseString);
            else
                sb.Append(BeingRepaird.Label);
            return sb.ToString();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compPowerTrader = base.GetComp<CompPowerTrader>();
            this.compPowerTrader.PowerOutput = -LOW_POWER_COST;

            this.CurrentMap = map;

            foreach(Building_Dresser d in BuildingUtil.FindThingsOfTypeNextTo<Building_Dresser>(base.Map, base.Position, Settings.RepairAttachmentDistance))
            {
                this.AddDresser(d);
            }

#if DEBUG_REPAIR
            Log.Warning(this.Label + " adding attached dressers:");
            foreach (Building_Dresser d in this.AttachedDressers)
            {
                Log.Warning(" " + d.Label);
            }
#endif

            this.compPowerTrader.powerStartedAction = new Action(delegate ()
            {
                this.compPowerTrader.PowerOutput = LOW_POWER_COST;
            });

            this.compPowerTrader.powerStoppedAction = new Action(delegate ()
            {
                this.PlaceApparelInDresser();
                this.compPowerTrader.PowerOutput = 0;
            });
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            this.PlaceApparelInDresser();
            this.AttachedDressers.Clear();
        }

        public override void Discard(bool silentlyRemoveReferences = false)
        {
            base.Discard(silentlyRemoveReferences);
            this.PlaceApparelInDresser();
            this.AttachedDressers.Clear();
        }

        public override void DeSpawn()
        {
            base.DeSpawn();
            this.PlaceApparelInDresser();
            this.AttachedDressers.Clear();
        }

        public override void TickLong()
        {
            if (!this.compPowerTrader.PowerOn)
            {
                // Power is off
                if (BeingRepaird != null)
                {
                    this.PlaceApparelInDresser();
                }
            }
            else if (this.BeingRepaird == null)
            {
                // Power is on and not repairing anything
                this.BeingRepaird = this.FindApparelToRepair();
            }
            else if (
                this.BeingRepaird != null && 
                this.BeingRepaird.HitPoints == this.BeingRepaird.MaxHitPoints)
            {
                // Power is on
                // Repairing something
                // Apparel is fully repaired
                this.PlaceApparelInDresser();
                this.BeingRepaird = this.FindApparelToRepair();
            }
            
            if (this.BeingRepaird != null)
            {
                this.BeingRepaird.HitPoints += 1;

                float generatedHeat = GenTemperature.ControlTemperatureTempChange(
                    base.Position, base.Map, 10, float.MaxValue);
                this.GetRoomGroup().Temperature += generatedHeat;
                
                this.compPowerTrader.PowerOutput = -this.compPowerTrader.Props.basePowerConsumption;
            }
            else
            {
                this.compPowerTrader.PowerOutput = LOW_POWER_COST;
            }
        }

        private Apparel FindApparelToRepair()
        {
            for (LinkedListNode<Building_Dresser> n = this.AttachedDressers.First; n != null; n = n.Next)
            {
                Building_Dresser d = n.Value;
                if (!d.Spawned)
                {
                    this.AttachedDressers.Remove(n);
                }
                else
                {
                    foreach (LinkedList<Apparel> l in d.StoredApparel.StoredApparelLookup.Values)
                    {
                        foreach (Apparel a in l)
                        {
                            if (a.HitPoints < a.MaxHitPoints)
                            {
                                d.RemoveNoDrop(a);
                                return a;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void PlaceApparelInDresser()
        {
            if (this.BeingRepaird == null)
            {
                return;
            }

            Building_Dresser dresserToUse = null;
            for (LinkedListNode<Building_Dresser> n = this.AttachedDressers.First; n != null; n = n.Next)
            {
                Building_Dresser d = n.Value;
                if (!d.Spawned)
                {
                    this.AttachedDressers.Remove(n);
                }
                else
                {
                    if (d.settings.AllowedToAccept(this.BeingRepaird))
                    {
                        if (dresserToUse == null ||
                            dresserToUse.settings.Priority < d.settings.Priority)
                        {
                            dresserToUse = d;
                        }
                    }
                }
            }

            if (dresserToUse != null)
            {
                dresserToUse.AddApparel(this.BeingRepaird);
            }
            else
            {
                BuildingUtil.DropThing(this.BeingRepaird, this, this.CurrentMap, false);
            }
            this.BeingRepaird = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Deep.Look(ref this.BeingRepaird, "beingRepaired", new object[0]);
        }

        public void AddDresser(Building_Dresser dresser)
        {
            if (this.AttachedDressers.Contains(dresser))
            {
                return;
            }
            this.AttachedDressers.AddLast(dresser);
        }

        public void RemoveDresser(Building_Dresser dresser)
        {
            this.AttachedDressers.Remove(dresser);
        }
    }
}
