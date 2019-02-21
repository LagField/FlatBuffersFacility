using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Eto.Forms;

namespace FlatBuffersFacility
{
    public static class CodeGenerator
    {
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

                StringBuilder batchSB = new StringBuilder();
                batchSB.AppendLine("@echo off");
                batchSB.AppendLine($"set FbsFileDirectory={AppData.FbsDirectory}\\");
                batchSB.AppendLine($"set CSharpFileOutputDirectory={AppData.CsOutputDirectory}");
                batchSB.AppendLine($"{AppData.CompilerPath} --csharp -o %CSharpFileOutputDirectory%^");
                for (int i = 0; i < selectFbsFileNames.Length; i++)
                {
                    string fileName = selectFbsFileNames[i];
                    string filePath = $"{AppData.FbsDirectory}/{fileName}.fbs";
                    batchSB.Append($" %FbsFileDirectory%{fileName}");
                    if (i != selectFbsFileNames.Length - 1)
                    {
                        batchSB.Append("^");
                    }

                    batchSB.AppendLine();
                }

                //写入.bat文件
                string batFilePath = AppDomain.CurrentDomain.BaseDirectory + "compile.bat";
                File.WriteAllText(batFilePath, batchSB.ToString());
                Process process = new Process();
                ProcessStartInfo startInfo =
                    new ProcessStartInfo(batFilePath) {RedirectStandardOutput = true, UseShellExecute = false,CreateNoWindow = true};
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
                
//                Debug.WriteLine(outputStringBuilder.ToString());
//                Debug.WriteLine("validate: " + BatchOutputValidate(outputStringBuilder.ToString()));
                string batchOutput = outputStringBuilder.ToString();
                if (!BatchOutputValidate(batchOutput))
                {
                    Console.WriteLine($"flatbuffers compiler has an error: {batchOutput}");
                    throw new GenerateCodeException{errorMessage = $"flatbuffers compiler has an error: {batchOutput}"};
                }
            }
            catch (GenerateCodeException exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show(exception.errorMessage, "生成文件出错", MessageBoxType.Error);
            }
        }
        
        private static bool BatchOutputValidate(string batchOutput)
        {
            return string.IsNullOrWhiteSpace(batchOutput);
        }
    }

    public class GenerateCodeException : Exception
    {
        public string errorMessage;
    }
}