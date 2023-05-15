
namespace PackageTracker.Data.Constants
{
	public static class AddressConstants
	{
		public const string AllStateAndTerritoryCodes = "AL,AK,AZ,AR,CA,CO,CT,DE,FL,GA,HI,ID,IL,IN,IA,KS,KY,LA,ME,MD,MA,MI,MN,MS,MO,MT,NE,NV,NH,NJ,NM,NY,NC,ND,OH,OK,OR,PA,RI,SC,SD,TN,TX,UT,VT,VA,WA,WV,WI,WY,AS,DC,FM,GU,MH,MP,PW,PR,VI";		
		public const string ContiguousStateCodes = "DC,AL,AZ,AR,CA,CO,CT,DE,FL,GA,ID,IL,IN,IA,KS,KY,LA,ME,MD,MA,MI,MN,MS,MO,MT,NE,NV,NH,NJ,NM,NY,NC,ND,OH,OK,OR,PA,RI,SC,SD,TN,TX,UT,VT,VA,WA,WV,WI,WY";
		public const string PoBoxRegex = @".*([p](\s|\.|\-)*[O](\s|\.|\-)*|post office\s*)b(ox|\.).*";
	}
}
