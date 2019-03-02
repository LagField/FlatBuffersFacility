using System.Diagnostics;
using System.IO;
using FlatBuffersFacility.Parser;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args)
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