using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParcelPrepGov.Reports.Models
{
    public class JobDataset : Dataset
	{
		[StringLength(100)]
		public string JobBarcode { get; set; } // [Index]
		[StringLength(30)]
		public string SubClientName { get; set; }

		public DateTime ManifestDate { get; set; }
		[StringLength(24)]
		public string MarkUpType { get; set; }
		[StringLength(24)] 
		public string MarkUp { get; set; }
		[StringLength(16)]
		public string Product { get; set; }
		[StringLength(16)]
		public string PackageType { get; set; }
		[StringLength(100)]
		public string PackageDescription { get; set; }
		public decimal Length { get; set; }
		public decimal Width { get; set; }
		public decimal Depth { get; set; }
		[StringLength(16)]
		public string MailTypeCode { get; set; }

		[StringLength(32)]
		public string Username { get; set; }
		[StringLength(32)]
		public string MachineId { get; set; }
		public List<JobContainerDataset> JobContainers { get; set; } = new List<JobContainerDataset>();
	}
}
