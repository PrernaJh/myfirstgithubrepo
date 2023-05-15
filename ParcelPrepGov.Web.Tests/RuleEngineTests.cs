using DevExpress.Compatibility.System.Web;
using ParcelPrepGov.Web.Features.Bulletin;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static ParcelPrepGov.Web.Features.Bulletin.BulletinProviderApiController;

namespace ParcelPrepGov.Web.Tests
{

    public partial class RuleEngineTests
    {
        [Fact]
        void Test_Rules()
        {
            // arrange
            var accessCmop = new AzureContainerAccess
            {
                ContainerName = "Bulletin",
                Folder = "CMOP",
                RoleAccess = "ClientFinancialWebUser"
            };


            var accessDalc = new AzureContainerAccess
            {
                ContainerName = "Bulletin",
                Folder = "DALC",
                RoleAccess = "ClientFinancialWebUser"
            };

            var accessOpus = new AzureContainerAccess
            {
                ContainerName = "Bulletin",
                Folder = "Opus",
                RoleAccess = "ClientFinancialWebUser"
            };

            var jsonArray = new List<AzureContainerAccess>() { accessCmop, accessDalc, accessOpus };
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(jsonArray);


            var rule = new Rule("Folder", "Equal", "CMOP");
            Func<AzureContainerAccess, bool> compiledRule = RuleManager.CompileRule<AzureContainerAccess>(rule);

            // act check rule
            bool returnTrue = compiledRule(accessOpus);

            // assert
            Assert.True(returnTrue);
        }

        /// <summary>
        ///  call method to get our default string from file
        /// </summary>
        [Fact]
        void Test_Container_Access_With_External_File_Json()
        {
            try
            {
                // arrange
                // all default container access at this moment
                var fileName = "./arrayOfContainers.txt";

                // act
                //var openFileArray = File.OpenRead(fileName);
                string fileContents;
                using (StreamReader reader = new StreamReader(fileName))
                {
                    fileContents = reader.ReadToEnd();
                }
                var serializer = new JavaScriptSerializer();
                var c = serializer.Deserialize<List<AzureContainerAccess>>(fileContents);

                // assert

                Assert.True(c != null);


            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Fact]
        void Test_Rule_With_External_File()
        {
            try
            {
                // arrange
                // all default rules, extend azure container rules by role?
                var fileName = "./arrayOfRules.txt";

                // act
                //var openFileArray = File.OpenRead(fileName);
                string fileContents;
                using (StreamReader reader = new StreamReader(fileName))
                {
                    fileContents = reader.ReadToEnd();
                }
                var serializer = new JavaScriptSerializer();
                var c = serializer.Deserialize<List<Rule>>(fileContents);

                // assert  
                Assert.True(c != null);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// works with a single rule at the moment
        /// </summary>
        [Fact]
        void Test_Complete_Access()
        {
            IRuleService service = new AzureRuleService();

            var truthTableOfAccess = service.GetContainerAccessList().GetAwaiter().GetResult();

            #region our code sample problem statement 
            /* 
            if (c != null)
            {
                if (User.IsClientWebFinancialUser())
                {
                    GrantCmopFolderAccess(c);
                }
                else if (User.IsClientWebUser())
                {
                    GrantCmopFolderAccess(c);
                }
                else if (User.IsAdministrator() || User.IsGeneralManager())
                {
                    // these 2 accounts have no limitations
                    // Adminstrator (RW), GENERALMANAGER (RW)                     
                }
                else if (User.IsQualityAssurance())
                {
                    foreach (dynamic row in c.result)
                    {
                        // check if it is a directory
                        if (row.isDirectory == false)
                        {
                            // do nothing to grant/revoke file access
                            // can we hide penguins file? todo research why folders disappear without a file in them in azure
                        }
                        else
                        {
                            // folder access check
                            var item = (from r in c.result
                                        select r).Where(f => f.name == "Daily Reports").ToArray();
                            if (item.Count() == 0)
                            {
                                // if there are results but CMOP is null we can assume they clicked CMOP
                                // do nothing
                            }
                            else
                            {
                                // ! important, here we are resetting the array to only show CMOP
                                c.result = item;
                            }
                        }
                    }
                }

                // set the key for each folder
                foreach (var item in c.result)
                {
                    item.key = provider.ContainerName + "|" + Guid.NewGuid();
                }

                // were adding the commands use later
                commandResult.Add(c);
            }
            */
            #endregion

            // this is the c.result item we are checking and assigning a specific folder
            Dictionary<string, string> cresult = new Dictionary<string, string>();

            string key = "CMOP";
            bool cmopFolderAccess;
            truthTableOfAccess.TryGetValue(key, out cmopFolderAccess);
            if (cmopFolderAccess)
            {
                // this means the user has access if the folder is cmop
                Assert.True(cmopFolderAccess);
            }

            string badKey = "DALC";
            bool dalcFolderAccess;
            truthTableOfAccess.TryGetValue(badKey, out dalcFolderAccess);
            if (!dalcFolderAccess)
            {
                // check if dalcFolderAccess is false, which it should be
                Assert.False(dalcFolderAccess);
            }            
        }

        public interface IRuleService
        {
            Task<Dictionary<string, bool>> GetContainerAccessList();
        }

        public class AzureRuleService : IRuleService
        {
            public async Task<Dictionary<string, bool>> GetContainerAccessList()
            {

                try
                {
                    // serializer
                    var serializer = new JavaScriptSerializer();

                    // GET the containers
                    var fileName = "./arrayOfContainers.txt";
                    string fileContents;
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        fileContents = await reader.ReadToEndAsync();
                    }

                    // deserialize text file to object
                    var containersFound = serializer.Deserialize<List<AzureContainerAccess>>(fileContents);

                    // GET the rules
                    var fileNameRules = "./Rule.txt";
                    string fileContentsRules;
                    using (StreamReader reader = new StreamReader(fileNameRules))
                    {
                        fileContentsRules = await reader.ReadToEndAsync();
                    }

                    // deserialize text file to object
                    var rulesFound = serializer.Deserialize<Rule>(fileContentsRules);

                    // compile the rules to see if rules match for role  
                    Func<AzureContainerAccess, bool> compiledRule = RuleManager.CompileRule<AzureContainerAccess>(rulesFound);

                    // check rule where the container matches container and store in dictionary to evaluate against
                    Dictionary<string, bool> matchList = new Dictionary<string, bool>();
                    foreach (var item in containersFound)
                    {
                        bool isMatch = compiledRule(item);
                        matchList.TryAdd(item.Folder, isMatch);
                    }
                    //bool retVal = compiledRule(containersFound);

                    return matchList;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }
}
