using System;

[Serializable]
public class SubstringReplacementsSet
{
	public StringReplacementData[] Entries;

	public string PerformFirstMatchingReplacement(string stringToFix)
	{
		StringReplacementData[] entries = Entries;
		foreach (StringReplacementData stringReplacementData in entries)
		{
			string text = stringToFix.Replace(stringReplacementData.FindSubstring, stringReplacementData.ReplaceWith);
			if (text != stringToFix)
			{
				return text;
			}
		}
		return stringToFix;
	}
}
