using MD5Hash;

namespace FileManifestTool;

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Enter source directory:");
        string source = Console.ReadLine();

        IndexKeeper indexKeeper = new IndexKeeper();

        List<FileData> fileDatas = GetFiles(source, indexKeeper, false);

        //Write out
        File.WriteAllLines($@"{source}\.manifest", fileDatas.Select(fd => $"{fd.BasicMetaHash} | {fd.FullMetaHash} | {fd.FileNumber} | {fd.File} | {fd.Size} | {fd.Created} | {fd.LastModified}").ToList());
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
