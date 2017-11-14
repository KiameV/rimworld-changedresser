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
        public const long FIFTEEN_SECONDS = TimeSpan.TicksPerSecond * 15;

        public static float RepairRate = 0.1f;

        public LinkedList<Building_Dresser> AttachedDressers = new LinkedList<Building_Dresser>();
        public CompPowerTrader compPowerTrader;
        public Stopwatch stopwatch = new Stopwatch();

        private bool isRepairing = false;

        public override string GetInspectString()
        {
            //this.Tick();
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.Append("\n");
            sb.Append("ChangeDresser.AttachedDressers".Translate());
            sb.Append(": ");
            sb.Append(this.AttachedDressers.Count);
            return sb.ToString();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compPowerTrader = base.GetComp<CompPowerTrader>();

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

        public override void TickRare()
        {
            long dt;
            if (!this.stopwatch.IsRunning)
            {
                this.stopwatch.Start();
                return;
            }
            else if (!this.isRepairing &&
                     this.stopwatch.ElapsedTicks < FIFTEEN_SECONDS)
            {
                return;
            }
            else
            {
                if (!this.isRepairing)
                {
                    dt = 10;
                }
                else
                {
                    dt = stopwatch.ElapsedMilliseconds;
                }
                this.stopwatch.Reset();
            }

            Log.Warning("Repair Tick dt: " + dt + " Repair Amount: " + dt * RepairRate);

            this.isRepairing = false;
            float repairAmount = dt * RepairRate;
            if (this.compPowerTrader.PowerOn)
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
                                    isRepairing = true;
                                    a.HitPoints = (int)(a.HitPoints * repairAmount);
                                }
                            }
                        }
                    }
                }

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
}
