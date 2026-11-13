namespace AtlasPacker.Models;


public class Sprite(string rootPath, FileInfo spriteFile)
{
    public FileInfo SpriteFile = spriteFile;
    public string Name = Path.GetRelativePath(rootPath, Path.ChangeExtension(spriteFile.FullName, null));

    public int Height { get; private set;}

    public int Width { get; private set;}

    public int X { get; private set;}

    public int Y { get; private set;}

    public void SetDimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }
}