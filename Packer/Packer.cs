using System.Xml.Linq;
using AtlasPacker.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AtlasPacker.Packer;

public class APacker(DirectoryInfo rawSprites, int padding = 4)
{
    private readonly DirectoryInfo RawSpritesFolder = rawSprites;
    public string DirectoryPath => RawSpritesFolder.FullName;

    private string? AtlasOutputPath;
    
    private readonly int Padding = padding; // default to 4 unless specified
    private const int MaxAtlasSize = 4096;


    // Scan
    private string AtlasPath => Path.Combine(Path.TrimEndingDirectorySeparator(RawSpritesFolder.FullName), "atlas");


    // Atlas Image
    //private Image FinalAtlasPng;

    public bool AtlasPack()
    {
        // Get All PNG files in RawSprites.
        List<FileInfo> files = FetchFiles();
        if (files.Count <= 0)
        {
            Console.WriteLine($"No PNG Files found at: {RawSpritesFolder.FullName}");
            return false;
        }

        // DEBUG
        //Console.WriteLine($"Directory Path: {RawSpritesFolder.FullName}");
        
        // Build List<Sprite> with the Width and Height of each Sprite
        List<Sprite> sprites = BuildAndMeasureSprites(files);
        if (sprites.Count <= 0)
        {
            Console.WriteLine("No Sprites were found");
            return false;
        }

        // Push Highest sprite to front of list, break height tie via width comparision
        sprites.Sort((a, b) =>
        {
            int result = b.Height.CompareTo(a.Height);
            if (result != 0)
            {
                return result;
            }
            return b.Width.CompareTo(a.Width);
        });
        
        var (canvasWidth, canvasHeight) = GetCanvasDimension(sprites);
        CreateAndSaveAtlas(canvasWidth, canvasHeight, sprites);

        // create xml definition for the sprite
        // create new 'atlas' folder in same directory
        // write the atlas to new folder
        SaveXmlDefinition(CreateXmlDefinition(sprites));
        //
        return true;
    }

    // Folder and Files
    private List<FileInfo> FetchFiles()
    {
        List<FileInfo> sprites = new();
        ScanFolder(RawSpritesFolder, sprites);
        Console.WriteLine($"DEBUG: PADDING = {Padding}");
        return sprites;
    }

    private  void ScanFolder(DirectoryInfo currentFolder, List<FileInfo> sprites)
    {
        FileInfo[] currentFiles = currentFolder.GetFiles();
        foreach (FileInfo file in currentFiles)
        {
            if (file.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                sprites.Add(file);
            }
        }
        
        DirectoryInfo[] subDirs = currentFolder.GetDirectories();
        foreach (DirectoryInfo subDir in subDirs)
        {
            if (string.Equals(subDir.FullName, AtlasPath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            ScanFolder(subDir, sprites);
        }
    }

    // Sprite and Pixels

    private List<Sprite> BuildAndMeasureSprites(List<FileInfo> spriteFiles)
    {
        List<Sprite> sprites = new();
        foreach (var spriteFile in spriteFiles)
        {
            try
            {
                ImageInfo spriteInfo = Image.Identify(spriteFile.FullName);
                Sprite sprite = new Sprite(DirectoryPath, spriteFile);
                sprite.SetDimensions(spriteInfo.Width, spriteInfo.Height);
                sprites.Add(sprite);
            }
            catch(NotSupportedException)
            {
                Console.WriteLine($"File Type not supported; file will be skipped: {spriteFile.FullName}");
            }
            catch (ImageFormatException)
            {
                Console.WriteLine($"Unknown Image Type detected; file will be skipped: {spriteFile.FullName}");
            }
        }
        return sprites;
    }

    private int GetNextPowerOf2(int value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxAtlasSize);
        if (value <= 1)
        {
            return 1;
        }

        int power = 1;
        while (power < value)
        {
            power <<= 1;
        }
        return power;
    }

    private int GetCanvasWidth(List<Sprite> sprites)
    {
        int maxSpriteWidth = 0;
        long totalArea = 0;
        foreach (var sprite in sprites)
        {
            int spriteWidth = sprite.Width + (Padding * 2);
            maxSpriteWidth = Math.Max(maxSpriteWidth, spriteWidth);
            totalArea += (sprite.Width + Padding) * (sprite.Height + Padding);
        }

        // Turn totalArea to a 1D side length
        int sideLength = (int)Math.Ceiling(Math.Sqrt(totalArea));
        return GetNextPowerOf2(Math.Max(maxSpriteWidth, sideLength));
    }

    private (int Width, int Height) GetCanvasDimension(List<Sprite> sprites)
    {
        int canvasWidth = GetCanvasWidth(sprites);
        int cursorX = Padding;
        int cursorY = Padding;
        int shelfHeight = 0; // tracks the tallest sprite on the current row

        foreach (var sprite in sprites)
        {
            if (cursorX + sprite.Width + Padding > canvasWidth)
            {
                cursorY += shelfHeight + Padding;
                cursorX = Padding;
                shelfHeight = 0;
            }
            sprite.SetPosition(cursorX, cursorY);
            cursorX += sprite.Width + Padding;
            shelfHeight = Math.Max(shelfHeight, sprite.Height);
        }
        int rawHeight = cursorY + shelfHeight + Padding;
        int canvasHeight = GetNextPowerOf2(rawHeight);
        return (canvasWidth, canvasHeight);
    }

    private void CreateAndSaveAtlas(int canvasWidth, int canvasHeight, List<Sprite> sprites)
    {
        using (Image atlas = new Image<Rgba32>(canvasWidth, canvasHeight))
        {
            foreach (var sprite in sprites)
            {
                using (Image spriteImage = Image.Load(sprite.SpriteFile.FullName))
                {

                    atlas.Mutate(ctx => ctx.DrawImage(
                        spriteImage,
                        new Point(sprite.X, sprite.Y),
                        1f
                    ));
                }
            }
            // SAVE as PNG.
            SaveAsPng(atlas);
        }
    }

    private void SaveAsPng(Image atlas)
    {
        string atlasFolderPath = Path.Combine(DirectoryPath, "atlas");
        AtlasOutputPath = atlasFolderPath;

        try
        {
            Directory.CreateDirectory(atlasFolderPath);
            atlas.SaveAsPng(Path.Combine(atlasFolderPath, "atlas.png"));
            Console.WriteLine($"Atlas file saved at: {atlasFolderPath}");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Permission error when trying to create atlas at: {atlasFolderPath}");
        }
        catch (IOException)
        {
            Console.WriteLine($"Write error when trying to create atlas at: {atlasFolderPath}");
        }     
        
    }

    private XDocument CreateXmlDefinition(List<Sprite> sprites)
    {
        XElement regions = new XElement("Regions");
        foreach (var sprite in sprites)
        {
            XElement region = new XElement("Region",
            new XAttribute("name", Path.GetFileNameWithoutExtension(sprite.SpriteFile.FullName)),
            new XAttribute("x", sprite.X),
            new XAttribute("y", sprite.Y),
            new XAttribute("width", sprite.Width),
            new XAttribute("height", sprite.Height)
            );
            regions.Add(region);
        }
        
        XElement texture = new XElement("Texture", "images/atlas");
        XElement textureAtlas = new XElement("TextureAtlas",
        texture,
        regions
        );

        XDocument atlasDefinition = new XDocument(textureAtlas);
        return atlasDefinition;
    }

    private void SaveXmlDefinition(XDocument file)
    {
        if (AtlasOutputPath == "")
        {
            Console.WriteLine("SaveXML was called before CreateXML");
            return;
        }
        string xmlPath = Path.Combine(AtlasOutputPath, "atlas.xml");
        try
        {
            file.Save(xmlPath);
            Console.WriteLine($"Atlas XML Definition file saved at: {xmlPath}");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Permission error when trying to create atlas xml file at: {AtlasOutputPath}");
        }
        catch (IOException)
        {
            Console.WriteLine($"Write error when trying to create atlas at: {AtlasOutputPath}");
        }  
    }



}