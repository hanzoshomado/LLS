using System;

public class NumberUtils
{
	public static int AddWithoutOverflowing(int number1, int number2)
	{
		try
		{
			int val = checked(number1 + number2);
			return Math.Max(0, val);
		}
		catch (OverflowException)
		{
			return int.MaxValue;
		}
	}

	public static int FloatCeilToPositiveInt(float f)
	{
		return Math.Max(0, FloatCeilToIntWithoutOverflowing(f));
	}

	public static int FloatCeilToIntWithoutOverflowing(float f)
	{
		double num = Math.Ceiling(f);
		if (num < 2147483647.0)
		{
			return (int)num;
		}
		return int.MaxValue;
	}
}
