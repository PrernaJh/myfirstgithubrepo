using Microsoft.Extensions.Logging;
using PackageTracker.Identity.Data.Models;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports
{
    public class UserLookupProcessor : IUserLookupProcessor
    {
        private readonly ILogger<UserLookupProcessor> logger;
        private IUserLookupRepository _userRepository;

        public UserLookupProcessor(ILogger<UserLookupProcessor> logger, IUserLookupRepository userRepository)
        {
            this.logger = logger;
            _userRepository = userRepository;
        }
   
        public async Task UpsertUser(ApplicationUser user)
        {
            var map = new UserLookup()
            {
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName                
            };

            await _userRepository.UpsertUser(map);
        }
    }
}
