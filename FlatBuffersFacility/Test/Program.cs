using System.Diagnostics;
using System.IO;
using FlatBuffers;
using FlatBuffersFacility.Parser;
using TestNameSpace;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            TestFlatbuffers();
        }

        private static void TestFlatbuffers()
        {
            MemoryStream ms = new MemoryStream();
            FlatBufferBuilder fbb = new FlatBufferBuilder(1024);

            //non scalar
            Car.StartCar(fbb);
            Car.AddId(fbb, 100);
            Car.AddSpeed(fbb, 200.3f);
            var carOffset = Car.EndCar(fbb);

            //scalar array
            Enemy.StartInventoryIdsVector(fbb, 10);
            for (int i = 10; i >= 0; i--)
            {
                fbb.AddInt(i);
            }

            var inventoryOffset = fbb.EndVector();

            StringOffset stringOffset = fbb.CreateString("jfoiajwseofjaw");
            Enemy.StartAllNamesVector(fbb, 10);
            fbb.AddOffset(stringOffset.Value);
            VectorOffset allNamesOffset = fbb.EndVector();


            Enemy.StartEnemy(fbb);
            Enemy.AddHp(fbb, 3203);
            Enemy.AddDrivenCar(fbb, carOffset);
            Enemy.AddInventoryIds(fbb, inventoryOffset);
            Enemy.AddAllNames(fbb, allNamesOffset);
            var enemyOffset = Enemy.EndEnemy(fbb);

            fbb.Finish(enemyOffset.Value);
            byte[] encodeBytes = fbb.SizedByteArray();
            Debug.WriteLine("encode bytes length: " + encodeBytes.Length);
            fbb.DataBuffer.CopyToStream(ms);

            fbb.Clear();
            fbb.DataBuffer.CopyFromStream(ms);

            Enemy e = Enemy.GetRootAsEnemy(fbb.DataBuffer);
            Debug.WriteLine(e.AllNames(0));
            Debug.WriteLine(e.Hp);
        }


        private static void TestParseFbsFile()
        {
            const string testFilePath = @"D:\CSharpProjects\FlatBuffersFacility\Runtime\FbsFiles\Test.fbs";
            if (!File.Exists(testFilePath))
            {
                Debug.WriteLine($"找不到文件{testFilePath}");
                return;
            }

            string[] allLines = File.ReadAllLines(testFilePath);
            FbsParser parser = new FbsParser();
            parser.ParseFbsFileLines(allLines);
        }
    }
}