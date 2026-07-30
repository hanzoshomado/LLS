using System.IO;

public class PathUtils
{
	public static string GetFolderName(string fullPath)
	{
		string directoryName = Path.GetDirectoryName(fullPath);
		string[] array = directoryName.Split("/".ToCharArray());
		return array[array.Length - 1];
	}
}
