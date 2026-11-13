using System.CommandLine;
using AtlasPacker.Packer;
namespace AtlasPacker;

class Program
{
    static int Main(string[] args)
    {
        RootCommand rootCommand = new("AtlasPacker CLI");

        Option<DirectoryInfo> folderOption = new("--folder")
        {
            Description = "Folder with images to combine into an atlas",
            Required = true,
            CustomParser = result =>
            {
                var folder = new DirectoryInfo(result.Tokens.Single().Value);
                if (!folder.Exists)
                {
                    result.AddError("Folder does not exist");
                }
                return folder;

            }
        };

        Option<int> paddingOption = new("--padding")
        {
            Description = "Controls the amount of pixels of space between each sprite. Default=4"
        };

        // Option<FileInfo> outputOption = new("--output")
        // {
        //     Description = "Output file",
        //     Required = true
        // };

        Command packCommand = new("pack", "Pack a folder")
        {
            folderOption,
            paddingOption
        };

        rootCommand.Add(packCommand);

        packCommand.SetAction(parseResult =>
        {
            DirectoryInfo? folder = parseResult.GetValue(folderOption);
            int padding = parseResult.GetValue(paddingOption);
            if (folder is not null && folder.Exists)
            {
                APacker packer;
                if (padding > 0)
                {
                    packer = new APacker(folder, padding);
                }
                else
                {
                    packer = new APacker(folder);
                }
                packer.AtlasPack();
            }
        });

        

        ParseResult parseResult = rootCommand.Parse(args);

        if (parseResult.Errors.Count > 0)
        {
            foreach (var error in parseResult.Errors)
            {
                Console.WriteLine(error.Message);
            }
            return 1;
        }
        return parseResult.Invoke();
    }

    // internal static void ReadFile(DirectoryInfo folder)
    // {
    //     if (!folder.Exists)
    //     {
    //         Console.WriteLine($"Folder does not exist: {folder.FullName}");
    //         return;
    //     }
    //     Console.WriteLine($"Reading directory: {folder.FullName}");

    // }

    // internal static void WriteFile(DirectoryInfo folder, string data)
    // {
    //     Console.WriteLine($"Writing file: {data}");
    //     DirectoryInfo atlasDirectory = Directory.CreateDirectory(Path.Combine(folder.FullName, "atlas"));
    //     File.WriteAllText(Path.Combine(atlasDirectory.FullName, "atlas.txt"), data);
    // }

    // internal static void DiscoverPNGs(DirectoryInfo folder)
    // {
    //     if (!folder.Exists)
    //     {
    //         Console.WriteLine($"Folder does not exist: {folder.FullName}");
    //         return;
    //     }
    //     FileInfo[] pngFiles = Directory.EnumerateFiles(Path.Combine(folder.FullName), "*.png", SearchOption.AllDirectories).Select(f => new FileInfo(f)).ToArray();
    //     Console.WriteLine($"Discovering PNGs in directory: {folder.FullName}");
    //     foreach (var pngFile in pngFiles)
    //     {
    //         Console.WriteLine($"Found PNG: {pngFile.FullName}");
    //     }
    // }
}
