using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Domain.Utilities
{
	public static class CollectionHelper
	{
		public static void AddRangeToConcurrentBag<T>(ConcurrentBag<T> input, IEnumerable<T> toAdd)
		{
			foreach (var element in toAdd)
			{
				input.Add(element);
			}
		}
	}
}
