using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace ChangeDresser
{
    class Building_RepairChangeDresser : Building
    {
        private const int LOW_POWER_COST = 6;
        //private const int RARE_TICKS_PER_HP = 4;

        public LinkedList<Building_Dresser> AttachedDressers = new LinkedList<Building_Dresser>();
        public CompPowerTrader compPowerTrader;

        private bool isRepairing = false;
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
            sb.Append(this.isRepairing);
            return sb.ToString();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compPowerTrader = base.GetComp<CompPowerTrader>();
            this.compPowerTrader.PowerOutput = -LOW_POWER_COST;

            this.AttachedDressers = BuildingUtil.FindThingsOfTypeNextTo<Building_Dresser>(base.Map, base.Position);
#if DEBUG_REPAIR
            Log.Warning(this.Label + " adding attached dressers:");
            foreach (Building_Dresser d in this.AttachedDressers)
            {
                Log.Warning(" " + d.Label);
            }
#endif
        }

        public override void DeSpawn()
        {
            base.DeSpawn();
            this.AttachedDressers.Clear();
        }

        public override void TickLong()
        {
            this.isRepairing = false;
            if (!this.compPowerTrader.PowerOn)
            {
                goto LEAVE_LOOP;
            }

            /*++this.rareTickCount;
            if (this.rareTickCount < RARE_TICKS_PER_HP)
            {
                Log.Warning(rareTickCount + " is less than " + RARE_TICKS_PER_HP + ", return");
                return;
            }
            this.rareTickCount = 0;*/

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
                                isRepairing = true;
                                a.HitPoints += 1;
                                goto LEAVE_LOOP;
                            }
                        }
                    }
                }
            }

            LEAVE_LOOP:
            if (this.isRepairing)
            {
                float generatedHeat = GenTemperature.ControlTemperatureTempChange(
                    base.Position, base.Map, 10, float.MaxValue);
                this.GetRoomGroup().Temperature += generatedHeat;
                
                this.compPowerTrader.PowerOutput = -this.compPowerTrader.Props.basePowerConsumption;
            }
            else
            {
                this.compPowerTrader.PowerOutput = this.compPowerTrader.Props.basePowerConsumption * -0.0285714285714286f;
            }
        }
    }
}
