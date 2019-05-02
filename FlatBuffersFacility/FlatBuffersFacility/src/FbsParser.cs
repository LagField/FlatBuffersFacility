using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FlatBuffersFacility.Parser
{
    public class FbsParser
    {
        public FbsStructure ParseFbsFileLines(string[] lines)
        {
            if (lines == null || lines.Length == 0)
            {
                throw new ParseFileException {errorMessage = "fbs文件解析失败：传入的文件没有内容"};
            }

            FbsStructure fbsStructure = new FbsStructure();
            int currentLineNum = 0;

            List<TableStructure> tableStructureList = new List<TableStructure>();
            while (currentLineNum < lines.Length)
            {
                if (string.IsNullOrEmpty(fbsStructure.namespaceName))
                {
                    ParseNamespace(lines[currentLineNum], ref currentLineNum, ref fbsStructure);
                }

                ParseObject(lines, ref currentLineNum, ref tableStructureList);
            }

            fbsStructure.tableStructures = tableStructureList.ToArray();
//            Debug.WriteLine(fbsStructure.ToString());
            ProcessTableAndFieldNamespace(fbsStructure);
            return fbsStructure;
        }

        private void ParseNamespace(string line, ref int currentLineNum, ref FbsStructure fbsStructure)
        {
            //namespace 不能以数字开头.只能包含英文字符数字和_
            const string validatePattern = @"namespace +[a-zA-Z_+][a-zA-Z0-9_]+ *;";
            MatchCollection matchCollection = Regex.Matches(line, validatePattern);
            if (matchCollection.Count == 0)
            {
                return;
            }

            if (matchCollection.Count != 1)
            {
                throw new ParseFileException {errorMessage = $"解析namespace出错，{line} , at {currentLineNum}"};
            }

            const string namespacePattern = @"[^namespace ][a-zA-Z_+][a-zA-Z0-9_]+";
            matchCollection = Regex.Matches(line, namespacePattern);
            if (matchCollection.Count != 1)
            {
                throw new ParseFileException {errorMessage = $"解析namespace出错，{line} , at {currentLineNum}"};
            }

            string namespaceName = matchCollection[0].Value.Trim();
            fbsStructure.namespaceName = namespaceName;
            currentLineNum++;
//            Debug.WriteLine($"namespace :{namespaceName}");
        }

        private void ParseObject(string[] lines, ref int currentLineNum, ref List<TableStructure> tableStructureList)
        {
            string line = lines[currentLineNum];
            //todo 支持struct类型
            const string tablePattern = @" *table *[a-zA-Z_+][a-zA-Z0-9_]+ *";
//            const string structPattern = @" *struct *[a-zA-Z_+][a-zA-Z0-9_]+ *";
            MatchCollection tableMatchCollection = Regex.Matches(line, tablePattern);
//            MatchCollection structMatchCollection = Regex.Matches(line, structPattern);
            if (tableMatchCollection.Count == 1)
            {
                string objectName = line;
                if (line.Contains("{"))
                {
                    int indexOfLeftCurlyBracket = line.IndexOf("{", StringComparison.Ordinal);
                    objectName = line.Slice(0, indexOfLeftCurlyBracket);
                }

                objectName = objectName.Replace("table", "").Replace(" ", "").Replace("{", "");

                TableStructure newTableStructure = new TableStructure {tableName = objectName};
                TraverseParseObject(lines, ref currentLineNum, ref newTableStructure);
                tableStructureList.Add(newTableStructure);
//                Debug.WriteLine(newTableStructure);
            }
            else
            {
                currentLineNum++;
            }
        }

        private void TraverseParseObject(string[] lines, ref int currentLineNum, ref TableStructure tableStructure)
        {
            List<TableFieldInfo> fieldInfoList = new List<TableFieldInfo>();
            while (currentLineNum < lines.Length)
            {
                string line = lines[currentLineNum];
                if (string.IsNullOrWhiteSpace(line))
                {
                    currentLineNum++;
                    continue;
                }

                bool isEndOfObject = line.Contains("}");

                //这里使用正则表达式寻找，因为fbs允许多个field写在一行
                //find pattern: fieldname:typename; or fieldname:[typename]; ;
                const string fieldPattern = @"[a-zA-Z_][a-zA-Z0-9_]+ *: *([a-zA-Z0-9_]+\.)*[a-zA-Z0-9_]+;";
                const string arrayFieldPattern = @"[a-zA-Z_][a-zA-Z0-9_]+ *: *\[([a-zA-Z0-9_]+\.)*[a-zA-Z0-9_]+ *\] *;";
                MatchCollection fieldMatchCollection = Regex.Matches(line, fieldPattern);
                MatchCollection arrayFieldMatchCollection = Regex.Matches(line, arrayFieldPattern);
                int fieldCount = fieldMatchCollection.Count;
                int arrayFieldCount = arrayFieldMatchCollection.Count;
                if (fieldCount == 0 && arrayFieldCount == 0)
                {
                    if (isEndOfObject)
                    {
                        currentLineNum++;
                        tableStructure.fieldInfos = fieldInfoList.ToArray();
                        return;
                    }

                    currentLineNum++;
                    continue;
                }

                for (int i = 0; i < fieldCount; i++)
                {
                    string matchString = fieldMatchCollection[i].Value;
                    matchString = matchString.Replace(" ", "");
                    //remove ;
                    matchString = matchString.Slice(0, -1);

                    string[] spliteStrings = matchString.Split(':');
                    string fieldName = spliteStrings[0];
                    string typeName = spliteStrings[1];

                    TableFieldInfo newFieldInfo = new TableFieldInfo
                    {
                        fieldName = fieldName,
                        fieldTypeName = typeName,
                        isArray = false,
                        isScalarType = CheckFlatbuffersTypeIsScalarType(typeName),
                        upperCamelCaseFieldName = ConvertToUpperCamelCase(fieldName),
                        fieldCSharpTypeName = ConvertToCSharpTypeName(typeName),
                    };
                    fieldInfoList.Add(newFieldInfo);
                }

                for (int i = 0; i < arrayFieldCount; i++)
                {
                    string matchString = arrayFieldMatchCollection[i].Value;
                    //remove ;
                    matchString = matchString.Replace(" ", "");
                    matchString = matchString.Slice(0, -1);

                    string[] spliteStrings = matchString.Split(':');
                    string fieldName = spliteStrings[0];
                    string typeName = spliteStrings[1];
                    //remove []
                    typeName = typeName.Slice(1, -1);

                    TableFieldInfo newFieldInfo = new TableFieldInfo
                    {
                        fieldName = fieldName,
                        fieldTypeName = typeName,
                        isArray = true,
                        //数组都不是scalar类型
                        isScalarType = false,
                        upperCamelCaseFieldName = ConvertToUpperCamelCase(fieldName),
                        fieldCSharpTypeName = ConvertToCSharpTypeName(typeName),
                        arrayTypeIsScalarType = CheckFlatbuffersTypeIsScalarType(typeName)
                    };
                    fieldInfoList.Add(newFieldInfo);
                }

                if (isEndOfObject)
                {
                    currentLineNum++;
                    tableStructure.fieldInfos = fieldInfoList.ToArray();
                    return;
                }

                currentLineNum++;
            }

            throw new ParseFileException {errorMessage = "解析文件出错，格式不正确"};
        }

        private void ProcessTableAndFieldNamespace(FbsStructure fbsStructure)
        {
            string targetNamespace = AppData.TargetNamespace;
            string namespaceName = fbsStructure.namespaceName;
            for (int i = 0; i < fbsStructure.tableStructures.Length; i++)
            {
                TableStructure tableStructure = fbsStructure.tableStructures[i];
                if (string.IsNullOrEmpty(namespaceName))
                {
                    tableStructure.tableNameWithNamespace = $"global::{tableStructure.tableName}";
                    tableStructure.tableNameWithCSharpNamespace = $"{targetNamespace}.{tableStructure.tableName}";
                }
                else
                {
                    tableStructure.tableNameWithNamespace = $"global::{namespaceName}.{tableStructure.tableName}";
                    tableStructure.tableNameWithCSharpNamespace = $"{targetNamespace}.{namespaceName}.{tableStructure.tableName}";
                }
                for (int j = 0; j < tableStructure.fieldInfos.Length; j++)
                {
                    TableFieldInfo fieldInfo = tableStructure.fieldInfos[j];
                    fieldInfo.fieldTypeNameWithNameSpace =
                        ConvertToCSharpTypeNameWithNamespaceName(fieldInfo.fieldTypeName, namespaceName);
                }
            }
        }

        public static readonly Dictionary<string, string> CsharpTypeNameConvertDic = new Dictionary<string, string>
        {
            {"int32", "int"},
            {"float32", "float"},
            {"byte", "sbyte"},
            {"int8", "sbyte"},
            {"uint8", "byte"},
            {"ubyte", "byte"},
            {"int16", "short"},
            {"uint16", "ushort"},
            {"uint32", "uint"},
            {"int64", "long"},
            {"uint64", "ulong"},
            {"float64", "double"},
        };

        private static readonly HashSet<string> ScalarTypeNameSet = new HashSet<string>
        {
            "byte",
            "int8",
            "ubyte",
            "uint8",
            "bool",
            "short",
            "int16",
            "int",
            "int32",
            "uint",
            "uint32",
            "float",
            "float32",
            "long",
            "int64",
            "ulong",
            "uint64",
            "double",
            "float64"
        };

        private string ConvertToCSharpTypeName(string originName)
        {
            return !CsharpTypeNameConvertDic.TryGetValue(originName, out string typeName) ? originName : typeName;
        }

        private string ConvertToCSharpTypeNameWithNamespaceName(string originTypeName, string namespaceName)
        {
            //如果类型带着.,则.之前的部分就是类型的命名空间
            if (originTypeName.Contains("."))
            {
                return originTypeName;
            }

            if (CheckFlatbuffersTypeIsScalarType(originTypeName))
            {
                return originTypeName;
            }

            if (string.IsNullOrEmpty(namespaceName))
            {
                return $"{ConvertToCSharpTypeName(originTypeName)}";
            }

            return $"{namespaceName}.{ConvertToCSharpTypeName(originTypeName)}";
        }

        private bool CheckFlatbuffersTypeIsScalarType(string typeName)
        {
            return ScalarTypeNameSet.Contains(typeName);
        }

        private string ConvertToUpperCamelCase(string originName)
        {
            string[] splitStrings = originName.Split('_');
            string result = "";
            for (int i = 0; i < splitStrings.Length; i++)
            {
                result += UpperFirstChar(splitStrings[i]);
            }

            return result;
        }

        private string UpperFirstChar(string s)
        {
            return s.First().ToString().ToUpper() + s.Substring(1);
        }
    }

    public class ParseFileException : Exception
    {
        public string errorMessage;
    }

    public class FbsStructure
    {
        public string namespaceName = "";

        public string ValidNameSpaceName
        {
            get { return $"global::{namespaceName}."; }
        }

        public TableStructure[] tableStructures;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace: {namespaceName}, has object: ");
            if (tableStructures != null)
            {
                for (int i = 0; i < tableStructures.Length; i++)
                {
                    sb.AppendLine(tableStructures[i] + ",");
                }
            }

            return sb.ToString();
        }
    }

    public class TableStructure
    {
        public string tableName;
        public string tableNameWithNamespace;//带着flatbuffers compiler生成代码的命名空间
        public string tableNameWithCSharpNamespace;//带着我们生成代码的命名空间
        public TableFieldInfo[] fieldInfos;

        public override string ToString()
        {
            string result = $"table {tableName}  field infos: ";
            if (fieldInfos != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    sb.AppendLine(fieldInfos[i] + ",");
                }

                result += sb.ToString();
            }

            return result;
        }
    }

    public class TableFieldInfo
    {
        public string fieldName;

        /// <summary>
        /// flatbuffers compiler会将任何field名称转换为upper camel case
        /// </summary>
        public string upperCamelCaseFieldName;

        /// <summary>
        /// field类型，如果是数组，则记录的是数组元素的类型
        /// </summary>
        public string fieldTypeName;

        /// <summary>
        /// <see cref="fieldTypeName"/>的csharp类型版本，主要就是处理了基础类型flatbuffers和c#名称不一致的问题
        /// </summary>
        public string fieldCSharpTypeName;

        /// <summary>
        /// c#类型，并带着自己的命名空间名字
        /// </summary>
        public string fieldTypeNameWithNameSpace;

        public bool isArray;
        public bool isScalarType;

        /// <summary>
        ///  如果field是数组类型，则肯定<see cref="isScalarType"/>是false，但是数组元素有可能是scalar类型。
        /// </summary>
        public bool arrayTypeIsScalarType;

        public bool IsString
        {
            get { return fieldCSharpTypeName == "string"; }
        }

        public override string ToString()
        {
            return
                $"field name: {fieldName}, upper camel case field name: {upperCamelCaseFieldName}, field type name: {fieldTypeName}," +
                $" field csharp type name: {fieldCSharpTypeName}, is array: {isArray}.";
        }
    }
}