namespace PackageTracker.Domain.Utilities
{
	public static class NullUtility
	{
		public static string NullExists(object obj)
		{
			string strValue = string.Empty;
			if (obj != null)
			{
				strValue = obj.ToString().Trim();
			}
			return strValue;
		}
	}
}
