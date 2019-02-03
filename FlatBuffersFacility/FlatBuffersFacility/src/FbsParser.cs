using System;

namespace FlatBuffersFacility
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

            return fbsStructure;
        }
    }

    public class ParseFileException : Exception
    {
        public string errorMessage;
    }

    public class FbsStructure
    {
        public string namespaceName;
        public TableStructure tableStructures;
    }

    public class TableStructure
    {
        public string tableName;
        public TableFieldInfo fieldInfos;
    }

    public class TableFieldInfo
    {
        public string fieldName;
        /// <summary>
        /// flatbuffers compiler会将任何field名称转换为upper camel case
        /// </summary>
        public string upperCamelCaseFieldName;
        public string fieldTypeName;
        public string fieldCSharpTypeName;
        public bool isArray;
    }
}