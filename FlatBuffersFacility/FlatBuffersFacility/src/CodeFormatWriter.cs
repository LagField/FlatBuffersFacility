using System;
using System.Text;

namespace FlatBuffersFacility
{
    public class CodeFormatWriter
    {
        private StringBuilder sb;
        private int indent;

        public override string ToString()
        {
            return sb.ToString();
        }

        public CodeFormatWriter()
        {
            sb = new StringBuilder();
            indent = 0;
        }

        public void AddIndent(int value)
        {
            indent += value;
            indent = Math.Max(0, indent);
        }

        public void WriteLine(string line)
        {
            WriteIndent();
            sb.AppendLine(line);
        }

        public void Write(string content)
        {
            sb.Append(content);
        }

        public void NewLine()
        {
            sb.AppendLine("");
        }

        public void WriteCommentLine(string comment)
        {
            sb.Append("//");
            sb.AppendLine(comment);
        }

        public void BeginBlock()
        {
            WriteLine("{");
            AddIndent(4);
        }

        public void EndBlock()
        {
            AddIndent(-4);
            WriteLine("}");
        }

        public void Clear()
        {
            sb.Clear();
        }

        private void WriteIndent()
        {
            for (int i = 0; i < indent; i++)
            {
                sb.Append(" ");
            }
        }
    }
}