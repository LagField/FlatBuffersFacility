using System;
using System.Collections.Generic;

namespace FlatBuffersFacility
{
    public static class Pool
    {
        private static Dictionary<Type, UniqueList<PoolObject>> poolDictionary = new Dictionary<Type, UniqueList<PoolObject>>();

        public static void Put<T>(T item) where T : PoolObject, new()
        {
            item.Release();
            
            Type type = typeof(T);
            if (poolDictionary.ContainsKey(type))
            {
                UniqueList<PoolObject> poolList = poolDictionary[type];
                if (poolList == null)
                {
                    poolList = new UniqueList<PoolObject>();
                    poolDictionary[type] = poolList;
                }

                poolList.Add(item);
            }
            else
            {
                UniqueList<PoolObject> poolList = new UniqueList<PoolObject> {item};
                poolDictionary.Add(type, poolList);
            }
        }

        public static T Get<T>() where T : PoolObject, new()
        {
            Type type = typeof(T);
            if (poolDictionary.ContainsKey(type))
            {
                UniqueList<PoolObject> poolList = poolDictionary[type];
                if (poolList == null)
                {
                    poolList = new UniqueList<PoolObject>();
                    poolDictionary[type] = poolList;
                }

                if (poolList.Count == 0)
                {
                    return new T();
                }

                return poolList.PopLast() as T;
            }

            return new T();
        }
    }
}