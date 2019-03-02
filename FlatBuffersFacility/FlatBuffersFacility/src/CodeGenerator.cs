using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Eto.Forms;
using FlatBuffersFacility.Parser;

namespace FlatBuffersFacility
{
    public static class CodeGenerator
    {
        private static FbsParser fbsParser;
        private static CodeFormatWriter formatWriter;

        public static void Generate(string targetNamespace, string[] selectFbsFileNames)
        {
            try
            {
                if (string.IsNullOrEmpty(targetNamespace))
                {
                    throw new GenerateCodeException {errorMessage = "命名空间未填写"};
                }

                if (!Directory.Exists(AppData.FbsDirectory))
                {
                    throw new GenerateCodeException {errorMessage = $"{AppData.FbsDirectory}文件夹不存在"};
                }

                if (!Directory.Exists(AppData.CsOutputDirectory))
                {
                    throw new GenerateCodeException {errorMessage = $"{AppData.CsOutputDirectory}文件夹不存在"};
                }

                if (!File.Exists(AppData.CompilerPath))
                {
                    throw new GenerateCodeException {errorMessage = $"{AppData.CompilerPath}文件不存在"};
                }

                if (selectFbsFileNames == null || selectFbsFileNames.Length == 0)
                {
                    return;
                }

                if (fbsParser == null)
                {
                    fbsParser = new FbsParser();
                }

                if (formatWriter == null)
                {
                    formatWriter = new CodeFormatWriter();
                }

                //写batch 文件
                RunFlatbuffersCompiler(selectFbsFileNames);

                //生成cs文件
                GenerateCustomCSharpFiles(targetNamespace, selectFbsFileNames);

                MessageBox.Show("完成", "生成完成");
            }
            catch (GenerateCodeException exception)
            {
                Console.WriteLine(exception.errorMessage);
                MessageBox.Show(exception.errorMessage, "生成文件出错", MessageBoxType.Error);
            }
            catch (ParseFileException exception)
            {
                Console.WriteLine(exception.errorMessage);
                MessageBox.Show(exception.errorMessage, "解析fbs文件出错", MessageBoxType.Error);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                MessageBox.Show(e.Message, "未知错误", MessageBoxType.Error);
            }
        }

        private static void RunFlatbuffersCompiler(string[] selectFbsFileNames)
        {
            StringBuilder cmdStringBuilder = new StringBuilder();
            //flatbuffers compiler生成的文件放到FlatbuffersCompilerGenerated子文件夹中
            cmdStringBuilder.Append($"/C {AppData.CompilerPath} --csharp --gen-onefile -o {AppData.CsOutputDirectory}\\FlatbuffersCompilerGenerated ");
            for (int i = 0; i < selectFbsFileNames.Length; i++)
            {
                string fileName = selectFbsFileNames[i];

                cmdStringBuilder.Append($"{AppData.FbsDirectory}\\{fileName}");
            }
//            Debug.WriteLine(cmdStringBuilder.ToString());

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = cmdStringBuilder.ToString(),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            StringBuilder outputStringBuilder = new StringBuilder();
            process.OutputDataReceived += (sender, args) =>
            {
                string output = args.Data;
                if (string.IsNullOrWhiteSpace(output))
                {
                    return;
                }

                outputStringBuilder.AppendLine(args.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();

            string batchOutput = outputStringBuilder.ToString();
            if (!FlatbuffersCompilerOutputValidate(batchOutput))
            {
                Console.WriteLine($"flatbuffers compiler has an error: {batchOutput}");
                throw new GenerateCodeException {errorMessage = $"flatbuffers compiler has an error: {batchOutput}"};
            }
        }

        private static bool FlatbuffersCompilerOutputValidate(string batchOutput)
        {
            //flatbuffers compiler生成文件没有错误的话不会输出任何消息
            return string.IsNullOrWhiteSpace(batchOutput);
        }

        private static void GenerateCustomCSharpFiles(string targetNameSpace, string[] selectFbsFileNames)
        {
            string filesPath = AppData.FbsDirectory;
            //每一个fbs文件生成一份cs代码
            for (int i = 0; i < selectFbsFileNames.Length; i++)
            {
                string fbsFilePath = $"{filesPath}/{selectFbsFileNames[i]}";
                string fbsFileNameWithoutExtension = Path.GetFileNameWithoutExtension(fbsFilePath);
//                Debug.WriteLine(filePath);

                if (!File.Exists(fbsFilePath))
                {
                    throw new ParseFileException {errorMessage = $"无法解析文件 {fbsFilePath}. 该文件不存在"};
                }

                string[] allLines = File.ReadAllLines(fbsFilePath);
                FbsStructure fbsStruct = fbsParser.ParseFbsFileLines(allLines);
                if (fbsStruct.tableStructures.Length == 0)
                {
                    throw new GenerateCodeException {errorMessage = $"文件 {fbsFilePath} 中没有读取到有效的table"};
                }

                formatWriter.Clear();

                //write using
                formatWriter.WriteLine("using System.Collections.Generic;");
                formatWriter.WriteLine("using FlatBuffers;");
                formatWriter.NewLine();

                //write namespace
                formatWriter.WriteLine($"namespace {targetNameSpace}");
                formatWriter.BeginBlock();
                //write classes
                for (int j = 0; j < fbsStruct.tableStructures.Length; j++)
                {
                    WriteClassCode(fbsStruct.tableStructures[j]);
                    formatWriter.NewLine();
                }

                formatWriter.EndBlock();

                //save generate csharp file
                string csharpFilePath = $"{AppData.CsOutputDirectory}/{fbsFileNameWithoutExtension}.cs";
                File.WriteAllText(csharpFilePath, formatWriter.ToString());
            }
        }

        private static void WriteClassCode(TableStructure tableStructure)
        {
            formatWriter.WriteLine($"public class {tableStructure.tableName}");
            formatWriter.BeginBlock();

            for (int i = 0; i < tableStructure.fieldInfos.Length; i++)
            {
                TableFieldInfo structFieldInfo = tableStructure.fieldInfos[i];
                WriteFieldCode(structFieldInfo);
            }

            formatWriter.EndBlock();
        }

        private static void WriteFieldCode(TableFieldInfo structFieldInfo)
        {
            if (structFieldInfo.isArray)
            {
                formatWriter.WriteLine($"public List<{structFieldInfo.fieldCSharpTypeName}> {structFieldInfo.fieldName};");
                if (structFieldInfo.isScalarType)
                {
                    formatWriter.WriteLine(
                        $"private List<Offset<{structFieldInfo.fieldCSharpTypeName}>> {structFieldInfo.fieldName}OffsetList;");
                }
            }
            else
            {
                formatWriter.WriteLine($"public {structFieldInfo.fieldCSharpTypeName} {structFieldInfo.fieldName};");
            }
        }
    }

    public class GenerateCodeException : Exception
    {
        public string errorMessage;
    }
}