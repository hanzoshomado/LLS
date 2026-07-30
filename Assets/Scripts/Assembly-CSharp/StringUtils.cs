using System;

public class StringUtils
{
	public static string IntegerToStringVerbalExpression(int number)
	{
		if (number == 0)
		{
			return "zero";
		}
		if (number < 0)
		{
			return "minus " + IntegerToStringVerbalExpression(Math.Abs(number));
		}
		string text = string.Empty;
		if (number / 1000000 > 0)
		{
			text = text + IntegerToStringVerbalExpression(number / 1000000) + " million ";
			number %= 1000000;
		}
		if (number / 1000 > 0)
		{
			text = text + IntegerToStringVerbalExpression(number / 1000) + " thousand ";
			number %= 1000;
		}
		if (number / 100 > 0)
		{
			text = text + IntegerToStringVerbalExpression(number / 100) + " hundred ";
			number %= 100;
		}
		if (number <= 0)
		{
			return text;
		}
		if (text != string.Empty)
		{
			text += "and ";
		}
		string[] array = new string[20]
		{
			string.Empty,
			"one",
			"two",
			"three",
			"four",
			"five",
			"six",
			"seven",
			"eight",
			"nine",
			"ten",
			"eleven",
			"twelve",
			"thirteen",
			"fourteen",
			"fifteen",
			"sixteen",
			"seventeen",
			"eighteen",
			"nineteen"
		};
		string[] array2 = new string[10]
		{
			string.Empty,
			"ten",
			"twenty",
			"thirty",
			"forty",
			"fifty",
			"sixty",
			"seventy",
			"eighty",
			"ninety"
		};
		if (number < 20)
		{
			text += array[number];
		}
		else
		{
			int num = number / 10;
			text += array2[num];
			if (number % 10 > 0)
			{
				text = text + "-" + array[number % 10];
			}
		}
		return text;
	}

	public static string AddSpacesToCamelCasedString(string camelCasedString)
	{
		string text = string.Empty;
		char[] array = camelCasedString.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0 && char.IsUpper(array[i]))
			{
				text += " ";
			}
			text += array[i];
		}
		return text;
	}

	public static string[] GetNonEmptySplitOfCommaSeparatedString(string commaSeparatedString)
	{
		if (string.IsNullOrEmpty(commaSeparatedString))
		{
			return new string[0];
		}
		return commaSeparatedString.Split(",".ToCharArray());
	}
}
