using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.ServiceOverride.Models
{
	public class ServiceOverridePost : IValidatableObject
	{
		[Required]
		public string CustomerName { get; set; }
		public bool IsEnabled { get; set; }
		[Required]
		public DateTime StartDate { get; set; }
		[Required]
		public DateTime EndDate { get; set; }
		[Required]
		public string OldShippingCarrier { get; set; }
		[Required]
		public string OldShippingMethod { get; set; }
		[Required]
		public string NewShippingCarrier { get; set; }
		[Required]
		public string NewShippingMethod { get; set; }

		IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
		{
			if (StartDate < DateTime.Now.Date)
			{
				yield return new ValidationResult("StartDate must be greater than or equal to today");
			}
			if (EndDate.Date < StartDate.Date)
			{
				yield return new ValidationResult("EndDate must be greater than StartDate");
			}
		}
	}
}
