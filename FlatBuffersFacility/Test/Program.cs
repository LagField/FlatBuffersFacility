//#define USE_POOL_VERSION

using System.Diagnostics;
using FlatBuffers;
using FlatBuffersFacility;
using Game_WebProtocol.FB_WebProtocol;
using Game_WebProtocol.FB_WebProtocol2;

namespace Test
{
    internal class Program
    {
#if USE_POOL_VERSION
        public static void Main(string[] args)
        {
            Test();
        }

        private static void Test()
        {
            Enemy newEnemy = Pool.Get<Enemy>();

            newEnemy.name = "jojo";
            newEnemy.hp = 100;
            newEnemy.id = 39;
            for (int i = 0; i < 3; i++)
            {
                newEnemy.all_names.Add(i + " john");
            }

            for (int i = 0; i < 100; i++)
            {
                newEnemy.inventoryIds.Add(i);
            }

            Car newCar = new Car {id = 10, speed = 200};
            newEnemy.drivenCar = newCar;

            for (int i = 0; i < 10; i++)
            {
                newEnemy.ownCars.Add(new Car {id = i * 3, speed = 10 * i});
            }

            newEnemy.weapon = new Weapon {id = 10, name = "M416", ammo_capacity = 30};

            FlatBufferBuilder fbb = new FlatBufferBuilder(1024);
            newEnemy.Encode(fbb);
            Pool.Put(newEnemy);

            Enemy anotherEnemy = Pool.Get<Enemy>();
            anotherEnemy.Decode(fbb.DataBuffer);

            Debug.WriteLine(anotherEnemy.hp);
            Debug.WriteLine(anotherEnemy.id);
            Debug.WriteLine(anotherEnemy.all_names[2]);
            Debug.WriteLine(anotherEnemy.inventoryIds[20]);
            Debug.WriteLine(anotherEnemy.drivenCar.id);
            Debug.WriteLine(anotherEnemy.drivenCar.speed);
            Debug.WriteLine(anotherEnemy.ownCars[7].id);
            Debug.WriteLine(anotherEnemy.ownCars[7].speed);
            Debug.WriteLine(anotherEnemy.weapon.name);
            Debug.WriteLine(anotherEnemy.weapon.ammo_capacity);
            Pool.Put(anotherEnemy);
        }
#else
        public static void Main(string[] args)
        {
            Test();
        }
        
        private static void Test()
        {
            Enemy newEnemy = new Enemy();
            newEnemy.name = "jojo";
            newEnemy.hp = 100;
            newEnemy.id = 39;
            for (int i = 0; i < 3; i++)
            {
                newEnemy.all_names.Add(i + " john");
            }

            for (int i = 0; i < 100; i++)
            {
                newEnemy.inventoryIds.Add(i);
            }

            Car newCar = new Car {id = 10, speed = 200};
            newEnemy.drivenCar = newCar;

            for (int i = 0; i < 10; i++)
            {
                newEnemy.ownCars.Add(new Car {id = i * 3, speed = 10 * i});
            }

            newEnemy.weapon = new Game_WebProtocol.FB_WebProtocol2.Weapon {id = 10, name = "M416", ammo_capacity = 30};

            FlatBufferBuilder fbb = new FlatBufferBuilder(1024);
            newEnemy.Encode(fbb);

            Enemy anotherEnemy = new Enemy();
            anotherEnemy.Decode(fbb.DataBuffer);

            Debug.WriteLine(anotherEnemy.hp);
            Debug.WriteLine(anotherEnemy.id);
            Debug.WriteLine(anotherEnemy.all_names[2]);
            Debug.WriteLine(anotherEnemy.inventoryIds[20]);
            Debug.WriteLine(anotherEnemy.drivenCar.id);
            Debug.WriteLine(anotherEnemy.drivenCar.speed);
            Debug.WriteLine(anotherEnemy.ownCars[7].id);
            Debug.WriteLine(anotherEnemy.ownCars[7].speed);
            Debug.WriteLine(anotherEnemy.weapon.name);
            Debug.WriteLine(anotherEnemy.weapon.ammo_capacity);
        }
#endif
    }
}