using DevExpress.Compatibility.System.Web;
using DevExtreme.AspNet.Mvc.FileManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PackageTracker.Data.Constants;
using ParcelPrepGov.Web.Features.Common;
using ParcelPrepGov.Web.Features.Financials.Models.Containers;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace ParcelPrepGov.Web.Features.Financials
{
    [Authorize]
    public class FileManagerAzureProviderApiController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FileManagerAzureProviderApiController> _logger;
        private readonly AzureFileBuilder _azureBuilder = AzureFileBuilder.GetFileBuilder();

        public FileManagerAzureProviderApiController([FromServices] IConfiguration config, ILogger<FileManagerAzureProviderApiController> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// process the command  sent from file manage control
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <returns>a custom combined json array</returns>
        [Route("api/file-manager-azure", Name = "FileManagerAzureProviderApi")]
        public object FileSystemDynamic(FileSystemCommand command, string arguments)
        {

            var cameFrom = _azureBuilder.GetContainerName(arguments);
            arguments = _azureBuilder.FixArgs(arguments);

            List<CommandResult> commandResult = new List<CommandResult>();
            List<AzureCustomContainer> readonlyContainers = new List<AzureCustomContainer>();
            if (User.IsClientWebFinancialUser())
            {
                readonlyContainers.Add(new FinancialReturnAsnContainer(_config)); // CMOP FOLDER ONLY
                readonlyContainers.Add(new FinancialDataContainer(_config)); // CMOP FOLDER ONLY
            }
            else if (User.IsFscWebFinancialUser())
            {
                readonlyContainers.Add(new FinancialDataContainer(_config)); // ALL FOLDERS
                readonlyContainers.Add(new FinancialReturnAsnContainer(_config));
                readonlyContainers.Add(new FinancialExpenseInvoice(_config));
            }
            else if (User.IsSystemAdministrator() || User.IsAdministrator())
            {
                readonlyContainers.Add(new FinancialReturnAsnContainer(_config)); // CMOP FOLDER ONLY
                readonlyContainers.Add(new FinancialDataContainer(_config)); // CMOP FOLDER ONLY
                readonlyContainers.Add(new FinancialExpenseInvoice(_config));
            }


            object jsonOutput = null;
            // if we don't know where it came from, we can assume we are building it the first time
            if (String.IsNullOrEmpty(cameFrom))
            {
                jsonOutput = OutputContainerJson(command, arguments, commandResult, readonlyContainers, cameFrom);
            }
            else
            {
                jsonOutput = OutputSingleContainerJson(command, arguments, commandResult, readonlyContainers, cameFrom);
            }

            return jsonOutput;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <param name="commandResult"></param>
        /// <param name="readonlyContainers"></param>
        /// <param name="cameFrom">the container name</param>
        /// <returns></returns>
        private object OutputSingleContainerJson(FileSystemCommand command, string arguments, List<CommandResult> commandResult, List<AzureCustomContainer> readonlyContainers, string cameFrom)
        {
            try
            {               
                int.TryParse(_config.GetSection("NumberOfDaysToFilter").Value, out int days);
             
                // find the right container in the collection
                var azureProvider = (from r in readonlyContainers.AsQueryable()
                                     where r.ContainerName == cameFrom
                                     select r).FirstOrDefault();

                FileSystemConfiguration localConfig = new FileSystemConfiguration
                {
                    Request = Request,
                    FileSystemProvider = azureProvider.AzureFileProvider,
                    AllowDownload = true,
                    AllowCreate = true,
                    AllowUpload = true
                    //AllowedFileExtensions = new[] { ".txt", ".csv", ".xlsx", "" }
                };

                var processor = new AzureFileSystemCommandProcessor(localConfig);
                var result = processor.Execute(command, arguments);
                var cmdResult = result.GetClientCommandResult();

                if (command == FileSystemCommand.UploadChunk || command == FileSystemCommand.Download)
                {
                    // return this immediately for upload
                    return cmdResult;
                }

                var serializer = new JavaScriptSerializer();
                var json = serializer.Serialize(cmdResult);
                var c = serializer.Deserialize<CommandResult>(json);

                RoleBasedCheckList(commandResult, azureProvider, c, command);

                dynamic[] arr = _azureBuilder.RebuildArrayOfCommandResults(commandResult, days);

                var newCommand = new CommandResult()
                {
                    result = arr,
                    success = true,
                    errorid = null
                };
                // serialize
                object output = JsonConvert.SerializeObject(newCommand);
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OutputSingleContainerJson {ex.Message}");
            }

            // if we got this far we will log edge case
            return null;
        }

        /// <summary>
        /// iterate each custom container to call devexpress custom code that generates a json object
        /// the json object is then used to populate the file control        
        /// </summary>
        /// <param name="command">the command result is an object that holds the anonymous dynamic array</param>
        /// <param name="arguments"></param>
        /// <param name="commandResult"></param>
        /// <param name="customContainer"></param>
        /// <param name="cameFrom"></param>
        /// <returns>json</returns>
        private object OutputContainerJson(FileSystemCommand command, string arguments, List<CommandResult> commandResult,
            IEnumerable<AzureCustomContainer> customContainer, string cameFrom)
        {
            try
            {
                int.TryParse(_config.GetSection("NumberOfDaysToFilter").Value, out int days);
                                                
                foreach (var container in customContainer)
                {
                    // get this config by iterator 
                    var config = new FileSystemConfiguration
                    {
                        Request = Request,
                        FileSystemProvider = container.AzureFileProvider,
                        AllowDownload = true,
                    };

                    // setup processor (devexpress object)
                    var processor = new AzureFileSystemCommandProcessor(config);
                    var result = processor.Execute(command, arguments);
                    dynamic execute = result.GetClientCommandResult();

                    // bad container name 
                    if (execute == null)
                    {
                        throw new ApplicationException("Problem with execute container");
                    }
                    // serialize response into reverse command result object 
                    var serializer = new JavaScriptSerializer();
                    var json = serializer.Serialize(execute);
                    var c = serializer.Deserialize<CommandResult>(json);

                    if (command != FileSystemCommand.UploadChunk)
                    {
                        //reset array based on role
                        RoleBasedCheckList(commandResult, container, c, command);

                        // see if we have a key (parent nodes do not have key values)
                        var infoContainsKey = arguments.IndexOf("key");
                        if (infoContainsKey == -1)
                        {
                            _azureBuilder.AddParentNode(serializer, c, container.ContainerName);
                        }
                    }

                }

                // create a dynamic array because our commandResult[i].result is an anonymous type
                dynamic[] arr = _azureBuilder.RebuildArrayOfCommandResults(commandResult, days);

                // we return this type because devexpress file control expects this json response
                var newCommand = new CommandResult()
                {
                    result = arr,
                    success = true,
                    errorid = null
                };
                // serialize
                object output = JsonConvert.SerializeObject(newCommand);
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OutputContainerJson {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// business logic to parse the array by role
        /// </summary>
        /// <param name="commandResult"></param>
        /// <param name="provider"></param>
        /// <param name="commandResult"></param>
        private void RoleBasedCheckList(List<CommandResult> commandResults, AzureCustomContainer provider, CommandResult commandResult, FileSystemCommand command)
        {
            if (commandResult != null)
            { 
                if (User.IsClientWebFinancialUser() || User.IsAdministrator())
                {
                    foreach (dynamic row in commandResult.result)
                    {
                        if ((bool)row.isDirectory)
                        {
                            var usersClient = User.GetClient();

                            dynamic items = new List<ExpandoObject>();

                            if (usersClient == ClientSubClientConstants.AllClients)
                            {
                                items = (from r in commandResult.result
                                            select r).ToArray();                              
                            }
                            else
                            {
                                items = (from r in commandResult.result
                                            select r)
                                    .Where(c => c.name == usersClient)
                                    .ToArray();                                                                                                   
                            }

                            if (items.Length > 0)
                            {
                                // ! important, here we are resetting the array to only show the Users' Client
                                commandResult.result = items;
                            }
                        }
                    }
                }

                // set the key for each folder
                foreach (var item in commandResult.result)
                {
                    item.key = provider.ContainerName + "|" + Guid.NewGuid();
                }

                // were adding the commands use later
                commandResults.Add(commandResult);
            }
        }
    }
}
