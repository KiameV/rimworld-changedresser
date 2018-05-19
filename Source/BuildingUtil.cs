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

        public static bool DropThing(Thing toDrop, Building from, Map map, bool makeForbidden = true)
        {
            return DropThing(toDrop, from.Position, map, makeForbidden);
        }

        public static bool DropThing(Thing toDrop, IntVec3 from, Map map, bool makeForbidden = true)
        {
            try
            {
                Thing t;
                if (!toDrop.Spawned)
                {
                    GenThing.TryDropAndSetForbidden(toDrop, from, map, ThingPlaceMode.Near, out t, makeForbidden);
                    if (!toDrop.Spawned)
                    {
                        GenPlace.TryPlaceThing(toDrop, from, map, ThingPlaceMode.Near);
                    }
                }

                IntVec3 pos = from;
                for (int i = 0; i < 4; ++i)
                {
                    pos = Transform(i, from);

                    bool canDropHere = true;
                    foreach (Thing temp in map.thingGrid.ThingsAt(pos))
                    {
                        if (temp.def.passability == Traversability.Impassable)
                        {
                            canDropHere = false;
                            break;
                        }
                    }

                    if (canDropHere)
                        break;
                }

                toDrop.Position = pos;

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

        private static IntVec3 Transform(int i, IntVec3 from)
        {
            IntVec3 result = from;
            switch (i)
            {
                case 0:
                    result.x = result.x + 1;
                    break;
                case 1:
                    result.x = result.x - 1;
                    break;
                case 2:
                    result.z = result.z + 1;
                    break;
                default:
                    result.z = result.z - 1;
                    break;
            }
            return result;
        }
    }
}
