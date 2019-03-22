using System.Diagnostics;
using MyProtocol;
using FlatBuffers;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Enemy enemy = new Enemy();
            enemy.id = 1;
            enemy.hp = 200;
            enemy.name = "jone";
            enemy.drivenCar = new Car {id = 12, speed = 300};
            enemy.inventoryIds.Add(1);
            enemy.inventoryIds.Add(32);
            enemy.inventoryIds.Add(24);

            FlatBufferBuilder fbb = new FlatBufferBuilder(1024);
            enemy.Encode(fbb);

            Debug.WriteLine(fbb.SizedByteArray().Length);

            Enemy newEnemy = new Enemy();
            newEnemy.Decode(fbb.DataBuffer);

            Debug.WriteLine(newEnemy.name);
            Debug.WriteLine(newEnemy.drivenCar.speed);
            Debug.WriteLine(newEnemy.ownCars.Count);
            for (int i = 0; i < newEnemy.inventoryIds.Count; i++)
            {
                Debug.WriteLine(newEnemy.inventoryIds[i]);
            }
        }

//        private static void TestFlatbuffers()
//        {
//            MemoryStream ms = new MemoryStream();
//            FlatBufferBuilder fbb = new FlatBufferBuilder(1024);
//
//            Enemy.StartOwnCarsVector(fbb,1);
//            Car.StartCar(fbb);
//            Car.AddId(fbb, 100);
//            Car.AddSpeed(fbb, 200.3f);
//            var carOffset = Car.EndCar(fbb);
//            fbb.AddOffset(carOffset.Value);
//            var ownCarOffset = fbb.EndVector();
//
//            //scalar array
//            Enemy.StartInventoryIdsVector(fbb, 10);
//            for (int i = 10; i >= 0; i--)
//            {
//                fbb.AddInt(i);
//            }
//
//            var inventoryOffset = fbb.EndVector();
//
//            Enemy.StartAllNamesVector(fbb, 1);
//            fbb.AddOffset(fbb.CreateString("jfoiajwseofjaw").Value);
//            VectorOffset allNamesOffset = fbb.EndVector();
//
//
//            Enemy.StartEnemy(fbb);
//            Enemy.AddHp(fbb, 3203);
//            Enemy.AddDrivenCar(fbb, carOffset);
//            Enemy.AddOwnCars(fbb,ownCarOffset);
//            Enemy.AddInventoryIds(fbb, inventoryOffset);
//            Enemy.AddAllNames(fbb, allNamesOffset);
//            var enemyOffset = Enemy.EndEnemy(fbb);
//
//            fbb.Finish(enemyOffset.Value);
//            byte[] encodeBytes = fbb.SizedByteArray();
//            Debug.WriteLine("encode bytes length: " + encodeBytes.Length);
//            fbb.DataBuffer.CopyToStream(ms);
//
//            fbb.Clear();
//            fbb.DataBuffer.CopyFromStream(ms);
//
//            Enemy e = Enemy.GetRootAsEnemy(fbb.DataBuffer);
//            Debug.WriteLine(e.OwnCars(0).Value.Speed);
//            Debug.WriteLine(e.Hp);
//        }
//
//
//        private static void TestParseFbsFile()
//        {
//            const string testFilePath = @"D:\CSharpProjects\FlatBuffersFacility\Runtime\FbsFiles\Test.fbs";
//            if (!File.Exists(testFilePath))
//            {
//                Debug.WriteLine($"找不到文件{testFilePath}");
//                return;
//            }
//
//            string[] allLines = File.ReadAllLines(testFilePath);
//            FbsParser parser = new FbsParser();
//            parser.ParseFbsFileLines(allLines);
//        }
    }
}