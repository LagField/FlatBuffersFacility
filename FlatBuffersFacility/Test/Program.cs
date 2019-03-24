using System.Diagnostics;
using MyProtocol;
using FlatBuffers;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            TestPoolVersion();
        }

        private static void Test()
        {
            Enemy enemy = new Enemy();
            enemy.id = 1;
            enemy.hp = 200;
            enemy.name = "jone";
            for (int i = 0; i < 2; i++)
            {
                enemy.ownCars.Add(new Car {id = i, speed = i * 100});
            }

            for (int i = 0; i < 3; i++)
            {
                enemy.all_names.Add("name + " + i);
            }

            FlatBufferBuilder fbb = new FlatBufferBuilder(1024);
            enemy.Encode(fbb);

            Enemy newEnemy = new Enemy();
            newEnemy.Decode(fbb.DataBuffer);

            Debug.WriteLine(newEnemy.ownCars[1].speed);
            Debug.WriteLine(newEnemy.all_names[1]);
        }

        private static void TestPoolVersion()
        {
            Enemy enemy = FlatBuffersFacility.Pool.Get<Enemy>();
            enemy.hp = 23;
            enemy.id = 1000;
            enemy.name = "shit";
            enemy.drivenCar = FlatBuffersFacility.Pool.Get<Car>();
            enemy.drivenCar.id = 123;
            enemy.drivenCar.speed = 2000;

            for (int i = 0; i < 3; i++)
            {
                Car car = FlatBuffersFacility.Pool.Get<Car>();
                car.id = i;
                car.speed = 200 * i;
                enemy.ownCars.Add(car);
            }

            for (int i = 0; i < 5; i++)
            {
                enemy.all_names.Add("my name " + i);
            }

            FlatBufferBuilder fbb = new FlatBufferBuilder(1024);
            enemy.Encode(fbb);
            FlatBuffersFacility.Pool.Put(enemy);

            Enemy anotherEnemy = FlatBuffersFacility.Pool.Get<Enemy>();
            anotherEnemy.Decode(fbb.DataBuffer);

            Debug.WriteLine(anotherEnemy.hp);
            Debug.WriteLine(anotherEnemy.name);
            Debug.WriteLine(anotherEnemy.drivenCar == null);
            Debug.WriteLine(anotherEnemy.ownCars[2].speed);
            Debug.WriteLine(anotherEnemy.ownCars.Count);
            Debug.WriteLine(anotherEnemy.all_names[4]);
            Debug.WriteLine(anotherEnemy.all_names.Count);
        }
    }
}