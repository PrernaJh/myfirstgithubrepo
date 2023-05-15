using System;
using System.Linq;

namespace PackageTracker.Identity.Service
{
	public static class IdentityPasswordHelper
	{

		private readonly static Random _random = new Random(Environment.TickCount);

		private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static string GenerateRandomPassword()
		{
			return $"{RandomChars()}{RandomNumber():D4}";
		}

		private static string RandomChars()
		{
			int length = 4;
			char[] randomChars = new char[length];
			int prev = _random.Next(chars.Length);

			for (int i = 0; i < length; i++)
			{
				int rand = _random.Next(chars.Length - 1);


				int index = (rand < prev) ? rand : rand + 1;
				if (i < length / 2)
                {
					randomChars[i] = chars[index];
				} else
                {
					randomChars[i] = chars.ToLower()[index];
				}
				
				prev = index;

			}
			return new string(randomChars);
		}

		private static string RandomUpperChars(int length)
		{
			char[] randomChars = new char[length];
			int prev = _random.Next(chars.Length);

			for (int i =0; i < length; i++)
            {
				int rand = _random.Next(chars.Length-1);


				int index = (rand < prev)? rand  : rand + 1;
				randomChars[i] = chars[index];
				prev = index;

			}
			return new string(randomChars);
		}

		private static string RandomLowerChars(int length)
		{
			char[] randomChars = new char[length];
			int prev = _random.Next(chars.Length);

			for (int i = 0; i < length; i++)
			{
				int rand = _random.Next(chars.Length - 1);


				int index = (rand < prev) ? rand : rand + 1;
				randomChars[i] = chars.ToLower()[index];
				prev = index;

			}
			return new string(randomChars);
		}

		private static int RandomNumber()
		{
			// No consecutive numbers or sequential numbers
			var randomNumbers =
			from a in Enumerable.Range(0, 9)
			from b in Enumerable.Range(0, 9)
			from c in Enumerable.Range(0, 9)
			from d in Enumerable.Range(0, 9)
			where a != b && b != c && c != d && a + 1 != b && b+1 != c && c+1 != d
			select a*1000+b*100+c*10+d;

			int randomNumber = randomNumbers.ElementAt(_random.Next(randomNumbers.Count()));
					

			return randomNumber;
		}

		private static bool MoreThan2ConsecutiveSequentialNumbersExist(int number)
		{
			int[] digits = number.ToString().ToCharArray().Select(Convert.ToInt32).ToArray();

			for (int i = 0; i <= digits.Length - 1; i++)
			{
				if (i + 2 >= digits.Length)
					break;

				int firstDigit = digits[i];
				int secondDigit = digits[i + 1];
				int thirdDigit = digits[i + 2];

				if (firstDigit + 1 != secondDigit)
					continue;

				if (secondDigit + 1 == thirdDigit)
					return true;
			}

			return false;
		}
	}
}