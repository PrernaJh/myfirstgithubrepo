using PackageTracker.Domain.Models;
using System;

namespace PackageTracker.Domain.Utilities
{
	public static class QueueUtility
	{
		public static EndOfDayQueueMessage ParseEodProcessQueueMessage(string message, bool dateOptional = false)
		{
			// queue message format example
			// siteName_username_dateToProcess
			// CHELMSFORD_tecadmin
			// CHELMSFORD_tecadmin_2021-2-25
			// CHELMSFORD_tecadmin_2021-2-25_EXTRA

			var response = new EndOfDayQueueMessage();
			var queueMessageArray = message.Split('_');
			if (queueMessageArray.Length > 2 && DateTime.TryParse(queueMessageArray[2], out var targetDate))
			{
				response.UseTargetDate = true;
				response.TargetDate = targetDate.Date;
			}
			else if (!dateOptional)
			{
				throw new ArgumentException($"Date is missing or malformed in end of day message: {message}");
			}

			response.SiteName = queueMessageArray[0];
			response.Username = queueMessageArray.Length > 1 ? queueMessageArray[1] : "System";
			response.Extra = queueMessageArray.Length > 3 ? queueMessageArray[3] : string.Empty;

			return response;
		}

		public static UpdateQueueMessage ParseUpdateQueueMessage(string message)
		{
			// queue message format examples:
			// LEAVENWORTH_7
			// DALCLEAVENWORTH_14

			var queueMessageArray = message.Split('_');
			var daysToLookback = 0;
			if (queueMessageArray.Length > 1)
				int.TryParse(queueMessageArray[1], out daysToLookback);

			return new UpdateQueueMessage
			{
				Name = queueMessageArray[0],
				DaysToLookback = daysToLookback
			};
		}
	}
}
