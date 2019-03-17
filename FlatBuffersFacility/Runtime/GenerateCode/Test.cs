using System.Collections.Generic;
using FlatBuffers;

namespace SkillData
{
    public class Enemy
    {
        public int id;
        public string name;
        public bool isLock;
        public float hp;
        public List<int> inventoryIds;
        public Car drivenCar;
        public List<Car> ownCars;
        public List<string> all_names;
    }

    public class Car
    {
        public int id;
        public float speed;
    }

}
