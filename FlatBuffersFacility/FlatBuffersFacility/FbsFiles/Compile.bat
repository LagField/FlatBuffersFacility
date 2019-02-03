@echo off

cd D:\CSharpProjects\FlatBuffersFacility\FlatBuffersFacility\FbsFiles
set FbsFileDirectory=D:\CSharpProjects\FlatBuffersFacility\FlatBuffersFacility\FbsFiles\
set CSharpFileOutputDirectory=D:\CSharpProjects\FlatBuffersFacility\FlatBuffersFacility\GenerateCode

flatc.exe --csharp -o %CSharpFileOutputDirectory%^
 %FbsFileDirectory%test.fbs

echo finish!
@pause