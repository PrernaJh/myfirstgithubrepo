using System;
using System.Collections.Generic;

namespace PackageTracker.Data.Utilities
{
	public static class BatchUtility
	{
		public static IEnumerable<IEnumerable<T>> BatchList<T>(this IEnumerable<T> source, int size)
		{
			using (var iteration = source.GetEnumerator())
			{
				while (iteration.MoveNext())
				{
					var batch = new T[size];
					var count = 1;
					batch[0] = iteration.Current;
					for (var i = 1; i < size && iteration.MoveNext(); i++)
					{
						batch[i] = iteration.Current;
						count++;
					}
					if (count < size)
					{
						Array.Resize(ref batch, count);
					}
					yield return batch;
				}
			}
		}

		public static int GetBatchSizeByTens(int count)
		{
			// generate a batch size 
			if (count < 1000)
			{
				return 1;
			}
			else if (count < 10000)
			{
				return count / 10;
			}
			else
			{
				return count / 100;
			}
		}
	}
}
