using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    class BuildingUtil
    {
        public static List<Thing> FindThingsNextTo(Map map, IntVec3 position, int distance)
        {
            int minX = Math.Max(0, position.x - distance);
            int maxX = Math.Min(map.info.Size.x, position.x + distance);
            int minZ = Math.Max(0, position.z - distance);
            int maxZ = Math.Min(map.info.Size.z, position.z + distance);

            List<Thing> list = new List<Thing>();
            for (int x = minX - 1; x <= maxX; ++x)
            {
                for (int z = minZ - 1; z <= maxZ; ++z)
                {
                    foreach (Thing t in map.thingGrid.ThingsAt(new IntVec3(x, position.y, z)))
                    {
                        list.Add(t);
                    }
                }
            }
            return list;
        }

        public static List<T> FindThingsOfTypeNextTo<T>(Map map, IntVec3 position, int distance) where T : Thing
        {
            int minX = Math.Max(0, position.x - distance);
            int maxX = Math.Min(map.info.Size.x, position.x + distance);
            int minZ = Math.Max(0, position.z - distance);
            int maxZ = Math.Min(map.info.Size.z, position.z + distance);

            List<T> list = new List<T>();
            for (int x = minX - 1; x <= maxX; ++x)
            {
                for (int z = minZ - 1; z <= maxZ; ++z)
                {
                    foreach (Thing t in map.thingGrid.ThingsAt(new IntVec3(x, position.y, z)))
                    {
                        if (t.GetType() == typeof(T))
                        {
                            list.Add((T)t);
                        }
                    }
                }
            }
            return list;
        }

        private static Random random = null;
        public static bool DropThing(Thing toDrop, Building from, Map map, bool makeForbidden = true)
        {
            try
            {
                Thing t;
                if (!toDrop.Spawned)
                {
                    GenThing.TryDropAndSetForbidden(toDrop, from.Position, map, ThingPlaceMode.Near, out t, makeForbidden);
                    if (!toDrop.Spawned)
                    {
                        GenPlace.TryPlaceThing(toDrop, from.Position, map, ThingPlaceMode.Near);
                    }
                }

                if (toDrop.Position.Equals(from.Position))
                {
                    IntVec3 pos = toDrop.Position;
                    if (random == null)
                        random = new Random();
                    int dir = random.Next(2);
                    int amount = random.Next(2);
                    if (amount == 0)
                        amount = -1;
                    if (dir == 0)
                        pos.x = pos.x + amount;
                    else
                        pos.z = pos.z + amount;
                    toDrop.Position = pos;
                }

                return toDrop.Spawned;
            }
            catch (Exception e)
            {
                Log.Error(
                    "ChangeDresser:BuildingUtil.DropApparel\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }
            return false;
        }
    }
}
