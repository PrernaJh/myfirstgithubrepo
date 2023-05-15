using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PackageTracker.Identity.Data.Constants
{
	public static class PPGRole
	{
		public const string SystemAdministrator = "SYSTEMADMINISTRATOR";
		public const string Administrator = "ADMINISTRATOR";
		public const string GeneralManager = "GENERALMANAGER";
		public const string Supervisor = "SUPERVISOR";
		public const string Operator = "OPERATOR";
		public const string QualityAssurance = "QUALITYASSURANCE";
		public const string ClientWebAdministrator = "CLIENTWEBADMINISTRATOR";
		public const string ClientWebUser = "CLIENTWEBUSER";
		public const string ClientWebPackageSearchUser = "CLIENTWEBPACKAGESEARCHUSER";
		public const string ClientWebFinancialUser = "CLIENTWEBFINANCIALUSER";
		public const string FSCWebFinancialUser = "FSCWEBFINANCIALUSER";
		public const string SubClientWebAdministrator = "SUBCLIENTWEBADMINISTRATOR";
		public const string SubClientWebUser = "SUBCLIENTWEBUSER";
		public const string AutomationStation = "AUTOMATIONSTATION";
		public const string TransportationUser = "TRANSPORTATIONUSER";
		public const string CustomerService = "CUSTOMERSERVICE";
        public static IList<string> ToList()
		{
			return typeof(PPGRole)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
				.Select(x => (string)x.GetRawConstantValue())
				.ToList();
		}
	}
}
