using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ChangeDresser
{
    class Building_RepairChangeDresser : Building
    {
        private const int LOW_POWER_COST = 10;
        //private const int RARE_TICKS_PER_HP = 4;

        private static LinkedList<Apparel> AllApparelBeingRepaired = new LinkedList<Apparel>();
        private LinkedList<Building_Dresser> AttachedDressers = new LinkedList<Building_Dresser>();
        public CompPowerTrader compPowerTrader;

        private Apparel BeingRepaird = null;
        private Map CurrentMap;
        //private int rareTickCount = 0;

        public override string GetInspectString()
        {
            //this.Tick();
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            if (sb.Length > 0)
                sb.Append(Environment.NewLine);
            sb.Append("ChangeDresser.AttachedDressers".Translate());
            sb.Append(": ");
            sb.Append(this.AttachedDressers.Count);
            sb.Append(Environment.NewLine);
            sb.Append("ChangeDresser.IsMending".Translate());
            sb.Append(": ");
            if (BeingRepaird == null)
            {
                sb.Append(Boolean.FalseString);
            }
            else
            {
                sb.Append(BeingRepaird.Label);
                sb.Append(Environment.NewLine);
                sb.Append("    ");
                sb.Append(BeingRepaird.HitPoints.ToString());
                sb.Append("/");
                sb.Append(BeingRepaird.MaxHitPoints);
            }
            return sb.ToString();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compPowerTrader = base.GetComp<CompPowerTrader>();
            this.compPowerTrader.PowerOutput = -LOW_POWER_COST;

            this.CurrentMap = map;

            foreach (Building_Dresser d in BuildingUtil.FindThingsOfTypeNextTo<Building_Dresser>(base.Map, base.Position, Settings.RepairAttachmentDistance))
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
                this.StopRepairing();
                this.compPowerTrader.PowerOutput = 0;
            });
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            this.StopRepairing();
            this.AttachedDressers.Clear();
        }

        public override void Discard(bool silentlyRemoveReferences = false)
        {
            base.Discard(silentlyRemoveReferences);
            this.StopRepairing();
            this.AttachedDressers.Clear();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            this.StopRepairing();
            this.AttachedDressers.Clear();
        }

        public override void TickLong()
        {
            if (!this.compPowerTrader.PowerOn)
            {
                // Power is off
                if (BeingRepaird != null)
                {
                    this.StopRepairing();
                }
            }
            else if (this.BeingRepaird == null)
            {
                // Power is on and not repairing anything
                this.StartRepairing();
            }
            else if (
                this.BeingRepaird != null && 
                this.BeingRepaird.HitPoints == this.BeingRepaird.MaxHitPoints)
            {
                // Power is on
                // Repairing something
                // Apparel is fully repaired
                this.BeingRepaird.HitPoints = this.BeingRepaird.MaxHitPoints;
                this.StopRepairing();
                this.StartRepairing();
            }
            
            if (this.BeingRepaird != null)
            {
                this.BeingRepaird.HitPoints += Settings.MendingAttachmentMendingSpeed;
                if (this.BeingRepaird.HitPoints > this.BeingRepaird.MaxHitPoints)
                {
                    this.BeingRepaird.HitPoints = this.BeingRepaird.MaxHitPoints;
                }

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

        private void OrderAttachedDressers()
        {
            bool isSorted = true;
            LinkedListNode<Building_Dresser> n = this.AttachedDressers.First;
            while (n != null)
            {
                var next = n.Next;
                if (!n.Value.Spawned)
                {
                    this.AttachedDressers.Remove(n);
                }
                else if (
                    n.Next != null &&
                    n.Value.settings.Priority < n.Next.Value.settings.Priority)
                {
                    isSorted = false;
                }
                n = next;
            }

            if (!isSorted)
            {
                LinkedList<Building_Dresser> ordered = new LinkedList<Building_Dresser>();
                for (n = this.AttachedDressers.First; n != null; n = n.Next)
                {
                    Building_Dresser d = n.Value;
                    bool inserted = false;
                    for (LinkedListNode<Building_Dresser> o = ordered.First; o != null; o = o.Next)
                    {
                        if (d.settings.Priority > o.Value.settings.Priority)
                        {
                            ordered.AddBefore(o, d);
                            inserted = true;
                            break;
                        }
                    }
                    if (!inserted)
                    {
                        ordered.AddLast(d);
                    }
                }
                this.AttachedDressers.Clear();
                this.AttachedDressers = ordered;
#if DEBUG
                Log.Warning("CD New Order:");
                foreach (Building_Dresser d in this.AttachedDressers)
                {
                    Log.Warning(" " + d.Label + " " + d.settings.Priority);
                }
#endif
            }
        }

        private void StartRepairing()
        {
#if AUTO_MENDER
            Log.Warning("Begin RepairChangeDresser.StartRepairing");
            Log.Message("    Currently Being Repaired:");
            foreach(Apparel a in AllApparelBeingRepaired)
            {
                Log.Message("        " + a.Label);
            }
#endif
            this.OrderAttachedDressers();
            foreach (PawnOutfitTracker po in WorldComp.PawnOutfits.Values)
            {
                foreach (Apparel a in po.CustomApparel)
                {
                    if (a.HitPoints < a.MaxHitPoints &&
                        !AllApparelBeingRepaired.Contains(a))
                    {
                        this.BeingRepaird = a;
                        AllApparelBeingRepaired.AddLast(a);
#if AUTO_MENDER
                        Log.Warning("End RepairChangeDresser.StartRepairing -- " + a.Label);
#endif
                        return;
                    }
                }
            }
            for (LinkedListNode<Building_Dresser> n = this.AttachedDressers.First; n != null; n = n.Next)
            {
                Building_Dresser d = n.Value;
                foreach (LinkedList<Apparel> l in d.StoredApparel.StoredApparelLookup.Values)
                {
                    foreach (Apparel a in l)
                    {
                        if (a.HitPoints < a.MaxHitPoints &&
                            !AllApparelBeingRepaired.Contains(a))
                        {
                            this.BeingRepaird = a;
                            AllApparelBeingRepaired.AddLast(a);
#if AUTO_MENDER
                            Log.Warning("End RepairChangeDresser.StartRepairing -- " + a.Label);
#endif
                            return;
                        }
                    }
                }
            }
#if AUTO_MENDER
            Log.Warning("End RepairChangeDresser.StartRepairing -- No new repairs to start");
#endif
        }

        private void StopRepairing()
        {
            if (this.BeingRepaird != null)
            {
                AllApparelBeingRepaired.Remove(this.BeingRepaird);
                this.BeingRepaird = null;
            }
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
