using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using MD5Hash;

namespace Ryancdotnet.FileManifestTool;

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Enter source directory:");
        string source = Console.ReadLine();

        IndexKeeper indexKeeper = new IndexKeeper();

        List<FileData> fileDatas = GetFiles(source, indexKeeper, false);

        //Write out
        StringBuilder fileBuilder = new StringBuilder();
        fileBuilder.AppendLine("RyanC.net File Manifest Tool Manifest");
        fileBuilder.AppendLine("0.1.0");

        fileDatas.ForEach(fd => fileBuilder.AppendLine($"{fd.BasicMetaHash} | {fd.FullMetaHash} | {fd.FileNumber} | {Path.GetFileName(fd.File)} | {Path.GetDirectoryName(fd.File)} | {fd.Size} | {fd.Created} | {fd.LastModified}"));

        File.WriteAllText($@"{source}\.manifest", fileBuilder.ToString());
    }

    private static List<FileData> GetFiles(string directory, IndexKeeper indexKeeper, bool shouldHash, bool recursive = true)
    {
        List<FileData> files = Directory.EnumerateFiles(directory).Select(f => GetFileData(f, ++indexKeeper.Index)).ToList();

        if (recursive)
        {
            foreach (string dir in Directory.EnumerateDirectories(directory))
            {
                files.AddRange(GetFiles(dir, indexKeeper, true));
            }
        }

        return files;
    }

    private static FileData GetFileData(string file, int index)
    {
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("Could not get FileData for file, as it does not exist.", file);
        }

        FileInfo fileInfo = new FileInfo(file);

        FileData fileData = new FileData
        {
            FileNumber = index,
            File = file,
            Created = fileInfo.CreationTimeUtc,
            LastModified = fileInfo.LastWriteTimeUtc,
            Size = fileInfo.Length
        };

        fileData.BasicMetaHash = CalculateBasicMetaHash(fileData);
        fileData.FullMetaHash = CalculateFullMetaHash(fileData);

        return fileData;
    }

    private static string CalculateBasicMetaHash(FileData fileData)
        => MD5Hash.Hash.GetMD5($"{Path.GetFileName(fileData.File.ToLower())}|{fileData.Size}");

    private static string CalculateFullMetaHash(FileData fileData)
        => MD5Hash.Hash.GetMD5($"{Path.GetFileName(fileData.File.ToLower())}|{fileData.LastModified.Ticks}|{fileData.Size}");
}
