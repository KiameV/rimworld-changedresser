using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    class BuildingUtil
    {
        public static LinkedList<T> FindThingsOfTypeNextTo<T>(Map map, IntVec3 position) where T : Thing
        {
            LinkedList<T> list = new LinkedList<T>();
            for (int x = position.x - 1; x <= position.x + 1; ++x)
            {
                for (int z = position.z - 1; z <= position.z + 1; ++z)
                {
                    foreach (Thing t in map.thingGrid.ThingsAt(new IntVec3(x, position.y, z)))
                    {
                        if (t.GetType() == typeof(T))
                        {
                            list.AddLast((T)t);
                        }
                    }
                }
            }
            return list;
        }
    }
}
