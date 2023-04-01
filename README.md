# File Manifest Tool
Creates a .manifest file with details of files found in the directory. Useful for comparison and deduping tools. See other tools.

# Building
```
dotnet build src
```

# Running
```
.\src\bin\Debug\net7.0\FileManifestTool.exe
```
Optionally, you may pass in the following command line arguments:
| Arg Number | Use |
|-|-|
| arg[0] | Source directory to scan |
| arg[1] | Output manifest file to write |

## Outputs
Writes a `.manifest` file in the same directory (unless arg[1] is passed) you scanned.
