using System.IO;

public static class DirectoryInfoExtensions
{
    public static (bool, DirectoryInfo) TryNavigateDirectoryUp(this DirectoryInfo baseDir, string name)
    {
        if (baseDir.Parent?.Name == name)
        {
            return (true, baseDir.Parent);
        }
        return (false, baseDir);
    }

    public static (bool, DirectoryInfo) TryNavigateDirectoryDown(this DirectoryInfo baseDir, string name)
    {
        var list = baseDir.GetDirectories(name);
        if (list.Length == 1)
        {
            return (true, list[0]);
        }
        return (false, baseDir);
    }
}