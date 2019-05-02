using System;
using System.Collections.Generic;
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
        private static bool generatePoolVersion;

        public static void Generate(string targetNamespace, string[] selectFbsFileNames, bool generatePoolVersion)
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

                CodeGenerator.generatePoolVersion = generatePoolVersion;

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

                cmdStringBuilder.Append($"{AppData.FbsDirectory}\\{fileName} ");
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
            List<FbsStructure> allFbsStructureList = new List<FbsStructure>();
            //每一个fbs文件生成一份cs代码
            for (int i = 0; i < selectFbsFileNames.Length; i++)
            {
                string fbsFilePath = $"{filesPath}\\{selectFbsFileNames[i]}";
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

                if (fbsStruct.namespaceName == targetNameSpace)
                {
                    throw new GenerateCodeException
                    {
                        errorMessage = $"文件 {fbsFilePath} 设定的namespace名称和工具输出的C#代码namespace相同了，确保命名空间不要冲突。然后删除刚才生成的文件，再试一次。"
                    };
                }

                WriteClassDefineCode(targetNameSpace, fbsStruct, fbsFileNameWithoutExtension);
                allFbsStructureList.Add(fbsStruct);
            }

            WriteFlatBuffersFacilityCode(targetNameSpace, allFbsStructureList);
        }

        private static void WriteClassDefineCode(string targetNameSpace, FbsStructure fbsStruct,
            string fbsFileNameWithoutExtension)
        {
            formatWriter.Clear();

            //write using
            formatWriter.WriteLine("using System.Collections.Generic;");
            formatWriter.WriteLine("using FlatBuffers;");
            formatWriter.NewLine();

            //write namespace
            if (string.IsNullOrEmpty(fbsStruct.namespaceName))
            {
                formatWriter.WriteLine($"namespace {targetNameSpace}");
            }
            else
            {
                formatWriter.WriteLine($"namespace {targetNameSpace}.{fbsStruct.namespaceName}");
            }

            formatWriter.BeginBlock();
            //write classes
            for (int j = 0; j < fbsStruct.tableStructures.Length; j++)
            {
                WriteClassCode(targetNameSpace, fbsStruct.tableStructures[j], fbsStruct);
                formatWriter.NewLine();
            }

            formatWriter.EndBlock();

            //save generate csharp file
            string csharpFilePath = $"{AppData.CsOutputDirectory}/{fbsFileNameWithoutExtension}.cs";
            File.WriteAllText(csharpFilePath, formatWriter.ToString());
        }

        private static void WriteClassCode(string targetNamespace, TableStructure tableStructure, FbsStructure fbsStructure)
        {
            if (generatePoolVersion)
            {
                formatWriter.WriteLine($"public class {tableStructure.tableName} : FlatBuffersFacility.PoolObject");
            }
            else
            {
                formatWriter.WriteLine($"public class {tableStructure.tableName}");
            }

            formatWriter.BeginBlock();

            for (int i = 0; i < tableStructure.fieldInfos.Length; i++)
            {
                TableFieldInfo structFieldInfo = tableStructure.fieldInfos[i];
                WriteFieldCode(structFieldInfo);
            }

            WriteClassEncodeAndDecode(tableStructure, targetNamespace);
            if (generatePoolVersion)
            {
                formatWriter.NewLine();
                WriteClassReleaseCode(tableStructure);
            }

            formatWriter.EndBlock();
        }

        private static void WriteFieldCode(TableFieldInfo structFieldInfo)
        {
            if (structFieldInfo.isScalarType)
            {
                formatWriter.WriteLine($"public {structFieldInfo.fieldCSharpTypeName} {structFieldInfo.fieldName};");
            }
            else
            {
                if (structFieldInfo.isArray)
                {
                    formatWriter.WriteLine(
                        $"public List<{structFieldInfo.fieldCSharpTypeName}> {structFieldInfo.fieldName} = new List<{structFieldInfo.fieldCSharpTypeName}>();");
                    if (generatePoolVersion)
                    {
                        if (structFieldInfo.IsString)
                        {
                            formatWriter.WriteLine($"internal List<StringOffset> {structFieldInfo.fieldName}OffsetList = " +
                                                   "new List<StringOffset>();");
                        }
                        else if (!structFieldInfo.arrayTypeIsScalarType)
                        {
                            formatWriter.WriteLine(
                                $"internal List<Offset<global::{structFieldInfo.fieldTypeNameWithNameSpace}>> {structFieldInfo.fieldName}OffsetList = " +
                                $"new List<Offset<global::{structFieldInfo.fieldTypeNameWithNameSpace}>>();");
                        }
                    }
                }
                else
                {
                    if (structFieldInfo.IsString)
                    {
                        formatWriter.WriteLine(
                            $"public {structFieldInfo.fieldCSharpTypeName} {structFieldInfo.fieldName} = \"\";");
                    }
                    else
                    {
                        formatWriter.WriteLine($"public {structFieldInfo.fieldCSharpTypeName} {structFieldInfo.fieldName};");
                    }
                }
            }
        }

        private static void WriteClassEncodeAndDecode(TableStructure tableStructure, string targetNamespace)
        {
            //encode
            formatWriter.NewLine();
            formatWriter.WriteLine("public void Encode(FlatBufferBuilder fbb)");
            formatWriter.BeginBlock();
            formatWriter.WriteLine(
                $"Offset<{tableStructure.tableNameWithNamespace}> offset = {targetNamespace}ConvertMethods.Encode(this, fbb);");
            formatWriter.WriteLine("fbb.Finish(offset.Value);");
            formatWriter.EndBlock();

            //decode
            formatWriter.NewLine();
            formatWriter.WriteLine("public void Decode(ByteBuffer bb)");
            formatWriter.BeginBlock();
            formatWriter.WriteLine(
                $"{tableStructure.tableNameWithNamespace} source = {tableStructure.tableNameWithNamespace}.GetRootAs{tableStructure.tableName}(bb);");
            formatWriter.WriteLine($"{targetNamespace}ConvertMethods.Decode(this, source);");

            formatWriter.EndBlock();
        }

        private static void WriteClassReleaseCode(TableStructure tableStructure)
        {
            formatWriter.WriteLine("public override void Release()");
            formatWriter.BeginBlock();

            for (int i = 0; i < tableStructure.fieldInfos.Length; i++)
            {
                TableFieldInfo fieldInfo = tableStructure.fieldInfos[i];
                if (fieldInfo.isArray)
                {
                    string fieldCSharpTypeName = fieldInfo.fieldCSharpTypeName;
                    if (IsNumericType(fieldCSharpTypeName) || fieldCSharpTypeName == "bool" || fieldCSharpTypeName == "string")
                    {
                        formatWriter.WriteLine($"{fieldInfo.fieldName}.Clear();");
                    }
                    else
                    {
                        formatWriter.WriteLine($"for (int i = 0; i < {fieldInfo.fieldName}.Count; i++)");
                        formatWriter.BeginBlock();
                        formatWriter.WriteLine($"{fieldInfo.fieldCSharpTypeName} item = {fieldInfo.fieldName}[i];");
                        formatWriter.WriteLine("FlatBuffersFacility.Pool.Put(item);");
                        formatWriter.EndBlock();
                        formatWriter.WriteLine($"{fieldInfo.fieldName}.Clear();");
                    }

                    if (fieldCSharpTypeName == "string" || !IsNumericType(fieldCSharpTypeName) && fieldCSharpTypeName != "bool")
                    {
                        formatWriter.WriteLine($"{fieldInfo.fieldName}OffsetList.Clear();");
                    }
                }
                else
                {
                    if (IsNumericType(fieldInfo.fieldCSharpTypeName))
                    {
                        formatWriter.WriteLine($"{fieldInfo.fieldName} = 0;");
                    }
                    else if (fieldInfo.fieldCSharpTypeName == "bool")
                    {
                        formatWriter.WriteLine($"{fieldInfo.fieldName} = false;");
                    }
                    else if (fieldInfo.fieldCSharpTypeName == "string")
                    {
                        formatWriter.WriteLine($"{fieldInfo.fieldName} = \"\";");
                    }
                    else
                    {
                        formatWriter.WriteLine($"if({fieldInfo.fieldName} != null)");
                        formatWriter.BeginBlock();
                        formatWriter.WriteLine($"FlatBuffersFacility.Pool.Put({fieldInfo.fieldName});");
                        formatWriter.WriteLine($"{fieldInfo.fieldName} = null;");
                        formatWriter.EndBlock();
                    }
                }
            }

            formatWriter.EndBlock();
        }

        private static void WriteFlatBuffersFacilityCode(string targetNameSpace, List<FbsStructure> fbsStructList)
        {
            formatWriter.Clear();

            //write using
            formatWriter.WriteLine("using System.Collections.Generic;");
            formatWriter.WriteLine("using FlatBuffers;");
            formatWriter.WriteLine($"using {targetNameSpace};");
            for (int i = 0; i < fbsStructList.Count; i++)
            {
                if (string.IsNullOrEmpty(fbsStructList[i].namespaceName))
                {
                    continue;
                }
                formatWriter.WriteLine($"using {targetNameSpace}.{fbsStructList[i].namespaceName};");
            }

            formatWriter.NewLine();

            //write namespace
            formatWriter.WriteLine($"namespace {targetNameSpace}");
            formatWriter.BeginBlock();

            //write class
            formatWriter.WriteLine($"public static class {targetNameSpace}ConvertMethods");
            formatWriter.BeginBlock();

            //所有fbs文件生成一个文件
            for (int i = 0; i < fbsStructList.Count; i++)
            {
                FbsStructure fbsStructure = fbsStructList[i];
                for (int j = 0; j < fbsStructure.tableStructures.Length; j++)
                {
                    TableStructure tableStructure = fbsStructure.tableStructures[j];
                    WriteTableEncodeCode(tableStructure);
                    WriteTableDecodeCode(tableStructure);
                    formatWriter.NewLine();
                }
            }

            formatWriter.EndBlock();
            formatWriter.EndBlock();

            string csharpFilePath = $"{AppData.CsOutputDirectory}/{targetNameSpace}ConvertMethods.cs";
            File.WriteAllText(csharpFilePath, formatWriter.ToString());
        }

        #region encode

        private static void WriteTableEncodeCode(TableStructure tableStructure)
        {
            formatWriter.WriteLine(
                $"public static Offset<{tableStructure.tableNameWithNamespace}> Encode({tableStructure.tableNameWithCSharpNamespace} source, FlatBufferBuilder fbb)");

            formatWriter.BeginBlock();

            //non scalar encode methods
            for (int i = 0; i < tableStructure.fieldInfos.Length; i++)
            {
                TableFieldInfo fieldInfo = tableStructure.fieldInfos[i];
                if (fieldInfo.isScalarType)
                {
                    continue;
                }

                GenerateNonScalarEncodeCode(tableStructure, fieldInfo);
            }

            formatWriter.WriteLine($"{tableStructure.tableNameWithNamespace}.Start{tableStructure.tableName}(fbb);");
            for (int i = 0; i < tableStructure.fieldInfos.Length; i++)
            {
                TableFieldInfo fieldInfo = tableStructure.fieldInfos[i];

                if (fieldInfo.isScalarType)
                {
                    formatWriter.WriteLine(
                        $"{tableStructure.tableNameWithNamespace}.Add{fieldInfo.upperCamelCaseFieldName}(fbb,source.{fieldInfo.fieldName});");
                }
                else
                {
                    formatWriter.WriteLine(
                        $"{tableStructure.tableNameWithNamespace}.Add{fieldInfo.upperCamelCaseFieldName}(fbb,{fieldInfo.fieldName}Offset);");
                }
            }

            formatWriter.WriteLine($"return {tableStructure.tableNameWithNamespace}.End{tableStructure.tableName}(fbb);");

            formatWriter.EndBlock();
        }

        private static void GenerateNonScalarEncodeCode(TableStructure tableStructure, TableFieldInfo fieldInfo)
        {
            string fieldName = fieldInfo.fieldName;
            if (fieldInfo.isArray)
            {
                if (fieldInfo.arrayTypeIsScalarType)
                {
                    formatWriter.WriteLine(
                        $"{tableStructure.tableNameWithNamespace}.Start{fieldInfo.upperCamelCaseFieldName}Vector(fbb,source.{fieldName}.Count);");

                    formatWriter.WriteLine($"for (int i = source.{fieldName}.Count - 1; i >= 0; i--)");
                    formatWriter.BeginBlock();
                    formatWriter.WriteLine(
                        $"fbb.Add{fieldInfo.fieldCSharpTypeName.UpperFirstChar()}(source.{fieldInfo.fieldName}[i]);");
                    formatWriter.EndBlock();
                    formatWriter.WriteLine($"VectorOffset {fieldName}Offset = fbb.EndVector();");
                }
                //其他table类型
                else
                {
                    if (generatePoolVersion)
                    {
                        formatWriter.WriteLine($"for (int i = 0; i < source.{fieldInfo.fieldName}.Count; i++)");
                        formatWriter.BeginBlock();
                        if (fieldInfo.IsString)
                        {
                            formatWriter.WriteLine(
                                $"source.{fieldInfo.fieldName}OffsetList.Add(fbb.CreateString(source.{fieldInfo.fieldName}[i]));");
                        }
                        else
                        {
                            formatWriter.WriteLine(
                                $"source.{fieldInfo.fieldName}OffsetList.Add(Encode(source.{fieldInfo.fieldName}[i],fbb));");
                        }

                        formatWriter.EndBlock();

                        formatWriter.WriteLine(
                            $"{tableStructure.tableNameWithNamespace}.Start{fieldInfo.upperCamelCaseFieldName}Vector(fbb,source.{fieldInfo.fieldName}.Count);");

                        formatWriter.WriteLine($"for (int i = source.{fieldInfo.fieldName}.Count - 1; i >= 0; i--)");
                        formatWriter.BeginBlock();
                        formatWriter.WriteLine($"fbb.AddOffset(source.{fieldInfo.fieldName}OffsetList[i].Value);");
                        formatWriter.EndBlock();

                        formatWriter.WriteLine($"VectorOffset {fieldInfo.fieldName}Offset = fbb.EndVector();");
                    }
                    //not generate pool version code
                    else
                    {
                        if (fieldInfo.IsString)
                        {
                            formatWriter.WriteLine($"StringOffset[] {fieldInfo.fieldName}Offsets = " +
                                                   $"new StringOffset[source.{fieldInfo.fieldName}.Count];");
                            formatWriter.WriteLine($"for (int i = source.{fieldInfo.fieldName}.Count - 1; i >= 0; i--)");
                            formatWriter.BeginBlock();
                            formatWriter.WriteLine(
                                $"{fieldInfo.fieldName}Offsets[i] = fbb.CreateString(source.{fieldInfo.fieldName}[i]);");
                            formatWriter.EndBlock();
                        }
                        else
                        {
                            formatWriter.WriteLine(
                                $"Offset<global::{fieldInfo.fieldTypeNameWithNameSpace}>[] {fieldInfo.fieldName}Offsets = " +
                                $"new Offset<global::{fieldInfo.fieldTypeNameWithNameSpace}>[source.{fieldInfo.fieldName}.Count];");

                            formatWriter.WriteLine($"for (int i = source.{fieldInfo.fieldName}.Count - 1; i >= 0; i--)");
                            formatWriter.BeginBlock();
                            formatWriter.WriteLine(
                                $"{fieldInfo.fieldName}Offsets[i] = Encode(source.{fieldInfo.fieldName}[i],fbb);");
                            formatWriter.EndBlock();
                        }

                        formatWriter.WriteLine($"VectorOffset {fieldInfo.fieldName}Offset = " +
                                               $"{tableStructure.tableNameWithNamespace}.Create{fieldInfo.upperCamelCaseFieldName}Vector(fbb, {fieldInfo.fieldName}Offsets);");
                    }
                }
            }
            else //not array
            {
                if (fieldInfo.IsString)
                {
                    formatWriter.WriteLine($"StringOffset {fieldName}Offset = fbb.CreateString(source.{fieldName});");
                }
                else
                {
                    formatWriter.WriteLine(
                        $"Offset<global::{fieldInfo.fieldTypeNameWithNameSpace}> {fieldName}Offset  = new Offset<global::{fieldInfo.fieldTypeNameWithNameSpace}>();");

                    formatWriter.WriteLine($"if(source.{fieldName} != null)");
                    formatWriter.BeginBlock();
                    formatWriter.WriteLine($"{fieldName}Offset = Encode(source.{fieldName},fbb);");
                    formatWriter.EndBlock();
                }
            }
        }

        #endregion

        #region decode

        private static void WriteTableDecodeCode(TableStructure tableStructure)
        {
            formatWriter.WriteLine(
                $" public static void Decode({tableStructure.tableNameWithCSharpNamespace} destination, {tableStructure.tableNameWithNamespace} source)");
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
                            if (generatePoolVersion)
                            {
                                formatWriter.WriteLine(
                                    $"{fieldInfo.fieldCSharpTypeName} new{fieldInfo.fieldCSharpTypeName} = FlatBuffersFacility.Pool.Get<{fieldInfo.fieldTypeNameWithNameSpace}>();");
                            }
                            else
                            {
                                formatWriter.WriteLine(
                                    $"{fieldInfo.fieldCSharpTypeName} new{fieldInfo.fieldCSharpTypeName} = new {fieldInfo.fieldTypeNameWithNameSpace}();");
                            }

                            formatWriter.WriteLine(
                                $"Decode(new{fieldInfo.fieldCSharpTypeName},source.{fieldInfo.upperCamelCaseFieldName}(i).Value);");
                            formatWriter.WriteLine($"destination.{fieldInfo.fieldName}.Add(new{fieldInfo.fieldCSharpTypeName});");
                            formatWriter.EndBlock();
                        }
                    }
                    else
                    {
                        formatWriter.WriteLine($"if (source.{fieldInfo.upperCamelCaseFieldName}.HasValue)");
                        formatWriter.BeginBlock();
                        if (generatePoolVersion)
                        {
                            formatWriter.WriteLine(
                                $"destination.{fieldInfo.fieldName} = FlatBuffersFacility.Pool.Get<{fieldInfo.fieldTypeNameWithNameSpace}>();");
                        }
                        else
                        {
                            formatWriter.WriteLine($"destination.{fieldInfo.fieldName} = new {fieldInfo.fieldTypeNameWithNameSpace}();");
                        }

                        formatWriter.WriteLine(
                            $"Decode(destination.{fieldInfo.fieldName},source.{fieldInfo.upperCamelCaseFieldName}.Value);");
                        formatWriter.EndBlock();
                    }
                }
            }

            formatWriter.EndBlock();
        }

        #endregion

        private static bool IsNumericType(string typeName)
        {
            foreach (var kv in FbsParser.CsharpTypeNameConvertDic)
            {
                if (kv.Value == typeName)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class GenerateCodeException : Exception
    {
        public string errorMessage;
    }
}