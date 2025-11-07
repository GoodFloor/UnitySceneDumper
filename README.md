# Unity Scene Dumper
## Usage
`$ ./UnitySceneDumper.exe <unity_project_path> <output_folder_path>`

## Features
It is a C# console application that allows you to dump some basic information from Unity project files.
For each scene it creates a file in which it prints scene's objects hierarchy.
On top of that in a separate file it prints all Scripts from Scripts folder that aren't currently being used in any of the scenes.
