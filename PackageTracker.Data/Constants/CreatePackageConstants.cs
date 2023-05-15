using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PackageTracker.Data.Constants
{
    public class CreatePackageConstants
    {
        public const string ClientRuleTypeConstant = "CLIENT";
        public const string SystemRuleTypeConstant = "SYSTEM";
        public const string DefaultMailCode = "0";

        public static readonly IEnumerable<string> ValidRuleTypes = new ReadOnlyCollection<string>(new List<string>
        {
            ClientRuleTypeConstant, SystemRuleTypeConstant
        });
    }
}
