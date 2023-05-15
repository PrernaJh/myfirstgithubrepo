using System;

namespace PackageTracker.Data.Models
{
	public abstract class Entity
	{
        public string Id { get; set; }
		public string PartitionKey { get; set; }
		public DateTime CreateDate { get; set; }
		public bool IsDatasetProcessed { get; set; }
	}
}
