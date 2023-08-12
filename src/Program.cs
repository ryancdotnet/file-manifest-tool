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
        string source = args.Length > 0 && Path.Exists(args[0]) ? args[0] : null;
        string manifestFile = args.Length > 1 ? args[1] : null;

        if (source == null)
        {
            Console.WriteLine("Enter source directory:");
            source = Console.ReadLine();
        }

        if (manifestFile == null)
        {
            Console.WriteLine("Enter manifest output directory (press enter for default) (DEFAULT: {source-directory}\\.manifest):");
            manifestFile = Console.ReadLine();
            manifestFile = String.IsNullOrWhiteSpace(manifestFile) ? $@"{source}\.manifest" : manifestFile;
        }

        IndexKeeper indexKeeper = new IndexKeeper();

        List<FileData> fileDatas = GetFiles(source, indexKeeper, false);

        //Write out
        StringBuilder fileBuilder = new StringBuilder();
        fileBuilder.AppendLine("RyanC.net File Manifest Tool Manifest");
        fileBuilder.AppendLine("0.1.0");

        fileDatas.ForEach(fd => fileBuilder.AppendLine($"{fd.BasicMetaHash} | {fd.FullMetaHash} | {fd.FileNumber} | {Path.GetFileName(fd.File)} | {Path.GetDirectoryName(fd.File)} | {fd.Size} | {fd.Created} | {fd.LastModified} | {fd.DateTaken}"));

        File.WriteAllText(manifestFile, fileBuilder.ToString());
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


        //TODO: Enable disabling EXIF reads
        if (true && (file.ToLower().EndsWith(".heic") || file.ToLower().EndsWith(".jpg")))
        {
            IReadOnlyList<MetadataExtractor.Directory> metadata = MetadataExtractor.ImageMetadataReader.ReadMetadata(file);

            //MetadataExtractor.Formats.Exif.ExifIfd0Directory.TagDateTime is something...

            //EXIF Date/Time Tag format = "2023:01:01 23:01:01"
            string? dateTimeTagValue = metadata.SingleOrDefault(d => d.Name == "Exif IFD0")?.Tags.SingleOrDefault(t => t.HasName && t.Name == "Date/Time")?.Description;

            if (dateTimeTagValue != null)
            {
                string[] split = dateTimeTagValue.Split(" ")!;
                split[0] = split[0].Replace(":", "/");
                dateTimeTagValue = $"{split[0]} {split[1]}";

                fileData.DateTaken = DateTime.Parse(dateTimeTagValue);
            }
        }

        fileData.BasicMetaHash = CalculateBasicMetaHash(fileData);
        fileData.FullMetaHash = CalculateFullMetaHash(fileData);

        return fileData;
    }

    private static string CalculateBasicMetaHash(FileData fileData)
        => MD5Hash.Hash.GetMD5($"{Path.GetFileName(fileData.File.ToLower())}|{fileData.Size}");

    private static string CalculateFullMetaHash(FileData fileData)
        => MD5Hash.Hash.GetMD5($"{Path.GetFileName(fileData.File.ToLower())}|{fileData.LastModified.Ticks}|{fileData.Size}|{fileData.DateTaken}");
}
