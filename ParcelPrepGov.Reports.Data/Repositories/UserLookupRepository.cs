using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
    public class UserLookupRepository : DatasetRepository, IUserLookupRepository
    {
        private readonly ILogger<UserLookupRepository> logger;
        private readonly IPpgReportsDbContextFactory factory;

        public UserLookupRepository(ILogger<UserLookupRepository> logger,
            IConfiguration configuration,
            IDbConnection connection, 
            IPpgReportsDbContextFactory factory) : base(configuration, connection, factory)
        {
            this.logger = logger;
            this.factory = factory;
        }

		public async Task UpsertUser(UserLookup user)
        {
			using (var context = factory.CreateDbContext())
			{
				var existingUser = context.UserLookups.Where(x=> x.Username.ToLower() == user.Username.ToLower()).FirstOrDefault();
				if(existingUser == null)
                {
					await AddUser(user);
                }
                else
                {
					if (existingUser.FirstName != user.FirstName || existingUser.LastName != user.LastName) 
					{
						existingUser.FirstName = user.FirstName;
						existingUser.LastName = user.LastName;
                        try
                        {
							context.Update(existingUser);
							await context.SaveChangesAsync();
						}
                        catch (Exception ex)
                        {
							logger.LogError($"Exception on UserLookup upsert: {ex}");							
                        }						
					}
                }
			}
		}

		private async Task<bool> AddUser(UserLookup user)
		{
			using (var context = factory.CreateDbContext())
			{
				try
				{
					await context.UserLookups.AddAsync(user);
					await context.SaveChangesAsync();
					return true;
				}
				catch (System.Exception ex)
				{
					logger.LogError($"Exception on UserLookup add: {ex}");
					return false;
				}
			}
		}		
	}
}
