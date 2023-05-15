using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PackageTracker.Domain.Utilities
{
	public class CreatePackageTimer
	{
		public CreatePackageTimer()
		{
			TotalWatch = new Stopwatch();
			SequenceWatch = new Stopwatch();
			ServiceWatch = new Stopwatch();
			ShippingWatch = new Stopwatch();
			AddQueryWatch = new Stopwatch();
		}

		public Stopwatch TotalWatch { get; set; }
		public Stopwatch SequenceWatch { get; set; }
		public Stopwatch ServiceWatch { get; set; }
		public Stopwatch ShippingWatch { get; set; }
		public Stopwatch AddQueryWatch { get; set; }
	}
}

