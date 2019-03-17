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
            cmdStringBuilder.Append(
                $"/C {AppData.CompilerPath} --csharp --gen-onefile -o {AppData.CsOutputDirectory}\\FlatbuffersCompilerGenerated ");
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
                WriteClassDefineCode(targetNameSpace, fbsStruct, fbsFileNameWithoutExtension);
                formatWriter.Clear();
                WriteFlatBuffersFacilityCode(targetNameSpace, fbsStruct, fbsFileNameWithoutExtension);
            }
        }

        private static void WriteClassDefineCode(string targetNameSpace, FbsStructure fbsStruct,
            string fbsFileNameWithoutExtension)
        {
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
                WriteClassCode(fbsStruct.tableStructures[j], fbsStruct.namespaceName);
                formatWriter.NewLine();
            }

            formatWriter.EndBlock();

            //save generate csharp file
            string csharpFilePath = $"{AppData.CsOutputDirectory}/{fbsFileNameWithoutExtension}.cs";
            File.WriteAllText(csharpFilePath, formatWriter.ToString());
        }

        private static void WriteClassCode(TableStructure tableStructure, string fbsNameSpace)
        {
            formatWriter.WriteLine($"public class {tableStructure.tableName}");
            formatWriter.BeginBlock();

            for (int i = 0; i < tableStructure.fieldInfos.Length; i++)
            {
                TableFieldInfo structFieldInfo = tableStructure.fieldInfos[i];
                WriteFieldCode(structFieldInfo, fbsNameSpace);
            }

            formatWriter.EndBlock();
        }

        private static void WriteFieldCode(TableFieldInfo structFieldInfo, string fbsNameSpace)
        {
            if (structFieldInfo.isScalarType)
            {
                formatWriter.WriteLine($"public {structFieldInfo.fieldCSharpTypeName} {structFieldInfo.fieldName};");
            }
            else
            {
                if (structFieldInfo.isArray)
                {
                    formatWriter.WriteLine($"public List<{structFieldInfo.fieldCSharpTypeName}> {structFieldInfo.fieldName};");
                }
                else
                {
                    formatWriter.WriteLine($"public {structFieldInfo.fieldCSharpTypeName} {structFieldInfo.fieldName};");
                }
            }
        }

        private static void WriteFlatBuffersFacilityCode(string targetNameSpace, FbsStructure fbsStruct,
            string fbsFileNameWithoutExtension)
        {
            //write using
            formatWriter.WriteLine("using System.Collections.Generic;");
            formatWriter.WriteLine("using FlatBuffers;");
            formatWriter.NewLine();

            //write namespace
            formatWriter.WriteLine($"namespace {targetNameSpace}");
            formatWriter.BeginBlock();

            //write class
            formatWriter.WriteLine($"public static class {targetNameSpace}ConvertMethods");
            formatWriter.BeginBlock();

            for (int j = 0; j < fbsStruct.tableStructures.Length; j++)
            {
                TableStructure tableStructure = fbsStruct.tableStructures[j];
                WriteTableEncodeCode(tableStructure, fbsStruct.namespaceName);
                WriteTableDecodeCode(tableStructure, fbsStruct.namespaceName);
                formatWriter.NewLine();
            }

            formatWriter.EndBlock();
            formatWriter.EndBlock();

            string csharpFilePath = $"{AppData.CsOutputDirectory}/{fbsFileNameWithoutExtension}ConvertMethods.cs";
            File.WriteAllText(csharpFilePath, formatWriter.ToString());
        }

        #region encode

        private static void WriteTableEncodeCode(TableStructure tableStructure, string fbsNameSpace)
        {
            string tableName = tableStructure.tableName;
            formatWriter.WriteLine(
                $"public static Offset<{fbsNameSpace}.{tableName}> Encode({tableName} source, FlatBufferBuilder fbb)");
            formatWriter.BeginBlock();

            //non scalar encode methods
            for (int i = 0; i < tableStructure.fieldInfos.Length; i++)
            {
                TableFieldInfo fieldInfo = tableStructure.fieldInfos[i];
                if (fieldInfo.isScalarType)
                {
                    continue;
                }

                GenerateNonScalarEncodeCode(tableStructure, fieldInfo, fbsNameSpace);
            }

            formatWriter.WriteLine($"{fbsNameSpace}.{tableName}.Start{tableName}(fbb);");
            for (int i = 0; i < tableStructure.fieldInfos.Length; i++)
            {
                TableFieldInfo fieldInfo = tableStructure.fieldInfos[i];

                if (fieldInfo.isScalarType)
                {
                    formatWriter.WriteLine(
                        $"{fbsNameSpace}.{tableName}.Add{fieldInfo.upperCamelCaseFieldName}(fbb,source.{fieldInfo.fieldName});");
                }
                else
                {
                    formatWriter.WriteLine(
                        $"{fbsNameSpace}.{tableName}.Add{fieldInfo.upperCamelCaseFieldName}(fbb,{fieldInfo.fieldName}Offset);");
                }
            }

            formatWriter.WriteLine($"return {fbsNameSpace}.{tableName}.End{tableName}(fbb);");

            formatWriter.EndBlock();
        }

        private static void GenerateNonScalarEncodeCode(TableStructure tableStructure, TableFieldInfo fieldInfo,
            string fbsNameSpace)
        {
            string fieldName = fieldInfo.fieldName;
            if (fieldInfo.isArray)
            {
                formatWriter.WriteLine(
                    $"{fbsNameSpace}.{tableStructure.tableName}.Start{fieldInfo.upperCamelCaseFieldName}Vector(fbb,source.{fieldName}.Count);");
                formatWriter.WriteLine($"for (int i = source.{fieldName}.Count - 1; i >= 0; i--)");
                formatWriter.BeginBlock();
                if (fieldInfo.arrayTypeIsScalarType)
                {
                    formatWriter.WriteLine($"fbb.Add{fieldInfo.fieldCSharpTypeName.UpperFirstChar()}(source.inventoryIds[i]);");
                }
                else
                {
                    if (fieldInfo.IsString)
                    {
                        formatWriter.WriteLine($"fbb.AddOffset(fbb.CreateString(source.{fieldInfo.fieldName}[i]).Value);");
                    }
                    else
                    {
                        formatWriter.WriteLine($"fbb.AddOffset(Encode(source.{fieldInfo.fieldName}[i],fbb).Value);");
                    }
                }

                formatWriter.EndBlock();
                formatWriter.WriteLine($"VectorOffset {fieldName}Offset = fbb.EndVector();");
            }
            else
            {
                if (fieldInfo.IsString)
                {
                    formatWriter.WriteLine($"StringOffset {fieldName}Offset = fbb.CreateString(source.name);");
                }
                else
                {
                    formatWriter.WriteLine(
                        $"Offset<{fbsNameSpace}.{fieldInfo.fieldCSharpTypeName}> {fieldName}Offset = Encode(source.{fieldName},fbb);");
                }
            }
        }

        #endregion

        #region decode

        private static void WriteTableDecodeCode(TableStructure tableStructure, string fbsNameSpace)
        {
            formatWriter.WriteLine(
                $" public static void Decode({tableStructure.tableName} destination, {fbsNameSpace}.{tableStructure.tableName} source)");
            formatWriter.BeginBlock();

            for (int i = 0; i < tableStructure.fieldInfos.Length; i++)
            {
                TableFieldInfo fieldInfo = tableStructure.fieldInfos[i];
                if (fieldInfo.isScalarType || fieldInfo.IsString && !fieldInfo.isArray)
                {
                    formatWriter.WriteLine($"destination.{fieldInfo.fieldName} = source.{fieldInfo.upperCamelCaseFieldName};");
                }
                else
                {
                    if (fieldInfo.isArray)
                    {
                        if (fieldInfo.arrayTypeIsScalarType || fieldInfo.IsString)
                        {
                            formatWriter.WriteLine($"for (int i = 0; i < source.{fieldInfo.upperCamelCaseFieldName}Length; i++)");
                            formatWriter.BeginBlock();
                            formatWriter.WriteLine(
                                $"destination.{fieldInfo.fieldName}.Add(source.{fieldInfo.upperCamelCaseFieldName}(i));");
                            formatWriter.EndBlock();
                        }
                        else
                        {
                            formatWriter.WriteLine($"for (int i = 0; i < source.{fieldInfo.upperCamelCaseFieldName}Length; i++)");
                            formatWriter.BeginBlock();
                            formatWriter.WriteLine(
                                $"{fieldInfo.fieldCSharpTypeName} new{fieldInfo.fieldCSharpTypeName} = new {fieldInfo.fieldCSharpTypeName}();");
                            formatWriter.WriteLine(
                                $"Decode(new{fieldInfo.fieldCSharpTypeName},source.{fieldInfo.upperCamelCaseFieldName}(i).Value);");
                            formatWriter.WriteLine(
                                $"destination.{fieldInfo.fieldName}.Add(new{fieldInfo.fieldCSharpTypeName});");
                            formatWriter.EndBlock();
                        }
                    }
                    else
                    {
                        formatWriter.WriteLine(
                            $"Decode(destination.{fieldInfo.fieldName},source.{fieldInfo.upperCamelCaseFieldName}.Value);");
                    }
                }
            }

            formatWriter.EndBlock();
        }

        #endregion
    }

    public class GenerateCodeException : Exception
    {
        public string errorMessage;
    }
}