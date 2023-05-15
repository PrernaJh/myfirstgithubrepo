using System.Diagnostics;

namespace MMS.API.Domain.Utilities
{
	public class PackageTimer
	{
		public PackageTimer()
		{
			TotalWatch = new Stopwatch();
			SelectQueryWatch = new Stopwatch();
			ServiceWatch = new Stopwatch();
			ContainerWatch = new Stopwatch();
			ShippingWatch = new Stopwatch();
			UpdateQueryWatch = new Stopwatch();
			AddQueryWatch = new Stopwatch();
			SequenceWatch = new Stopwatch();
			BinValidationTimer = new Stopwatch();
		}

		public Stopwatch TotalWatch { get; set; }
		public Stopwatch SelectQueryWatch { get; set; }
		public Stopwatch ServiceWatch { get; set; }
		public Stopwatch ContainerWatch { get; set; }
		public Stopwatch ShippingWatch { get; set; }
		public Stopwatch UpdateQueryWatch { get; set; }
		public Stopwatch AddQueryWatch { get; set; }
		public Stopwatch SequenceWatch { get; set; }
		public Stopwatch BinValidationTimer { get; set; }
	}
}
