using DevExpress.Compatibility.System.Web;
using DevExtreme.AspNet.Mvc.FileManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PackageTracker.Data.Constants;
using ParcelPrepGov.Web.Features.Bulletin.Models;
using ParcelPrepGov.Web.Features.Common;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;

namespace ParcelPrepGov.Web.Features.Bulletin
{
    [Authorize]
    public class BulletinProviderApiController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<BulletinProviderApiController> _logger;
        private readonly AzureFileBuilder _azureBuilder = AzureFileBuilder.GetFileBuilder();

        public BulletinProviderApiController([FromServices] IConfiguration config, ILogger<BulletinProviderApiController> logger)
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
        [Route("api/bulletin", Name = "BulletinProviderApi")]
        public object FileSystemDynamic(FileSystemCommand command, string arguments)
        {
            int.TryParse(_config.GetSection("NumberOfDaysToFilter").Value, out int days);
       
            var azureProvider = new BulletinContainer(_config);
            var uploadConfig = new UploadConfiguration();
            uploadConfig.ChunkSize = 200000;
            uploadConfig.MaxFileSize = 1103741824;

            FileSystemConfiguration localConfig = new FileSystemConfiguration
            {
                Request = Request,
                FileSystemProvider = azureProvider.AzureFileProvider,
                AllowDownload = true,
                AllowUpload = true,
                AllowDelete = true,
                UploadConfiguration = uploadConfig
                //AllowedFileExtensions = new[] { ".txt", ".csv", ".xlsx", "" }
            };

            var processor = new AzureFileSystemCommandProcessor(localConfig);
            var result = processor.Execute(command, arguments);
            var cmdResult = result.GetClientCommandResult();


            if (command == FileSystemCommand.UploadChunk || command == FileSystemCommand.Download || command == FileSystemCommand.Remove)
            {
                // return this immediately for upload
                return cmdResult;
            }

            #region todo be sure to doublecheck role access to folders vs files
            // folder view
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(cmdResult);
            var c = serializer.Deserialize<CommandResult>(json);

            List<CommandResult> commandResult = new List<CommandResult>();

            RoleBasedCheckList(commandResult, azureProvider, c, command);

            dynamic[] arr = _azureBuilder.RebuildArrayOfCommandResults(commandResult);

            var newCommand = new CommandResult()
            {
                result = arr,
                success = true,
                errorid = null
            };

            // serialize
            object output = JsonConvert.SerializeObject(newCommand);
            return output;

            #endregion         
        }

        /// <summary>
        /// business logic to parse the array by role
        /// </summary>
        /// <param name="commandResult"></param>
        /// <param name="provider"></param>
        /// <param name="c"></param>
        private void RoleBasedCheckList(List<CommandResult> commandResult, AzureCustomContainer provider, CommandResult c, FileSystemCommand command)
        {
            if (c != null)
            {
                if (User.IsClientWebFinancialUser() || User.IsClientWebUser() || User.IsSubClientWebUser())
                {
                    GrantFolderAccess(c);
                }
                else if (User.IsAdministrator() || User.IsGeneralManager())
                {
                    // We have to separate CMOP from seeing FSC Operations 
                    if (User.GetClient().ToString() == ClientSubClientConstants.CmopClientName)
                    {
                        GrantFolderAccess(c);
                    }                
                }
                else if (User.IsQualityAssurance())
                {
                    foreach (dynamic row in c.result)
                    {
                        // check if it is a directory
                        if (!row.isDirectory)
                        {
                            // folder access check
                            var items = (from r in c.result
                                        select r).Where(f => f.name == "Daily Reports" || f.name == "Damaged" || f.name == "Recalled").ToArray();
                            if (items.Count() > 0)
                            {
                                // ! important, here we are resetting the array to only show CMOP
                                c.result = items;
                            }

                            // can we hide penguins file? todo research why folders disappear without a file in them in azure
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
        }

        private void GrantFolderAccess(CommandResult command)
        {
            foreach (dynamic row in command.result)
            {
                if ((bool)row.isDirectory)
                {
                    var client = User.GetClient();
                    dynamic items = new List<ExpandoObject>();

                    if (client == ClientSubClientConstants.AllClients)
                    {
                        items = (from r in command.result
                                 select r).ToArray();
                    }
                    else if (client == ClientSubClientConstants.CmopClientName)
                    {
                        items = (from r in command.result
                                 select r)
                                 .Where(c => c.name != "FSC Operations")
                                 .ToArray();
                    }
                    else
                    {
                        items = (from r in command.result
                                 select r)
                                 .Where(f => f.name == User.GetClient().ToString())
                                 .ToArray();
                    }

                    if (items.Length > 0)
                    {
                        // ! important, here we are resetting the array to only show the Users' Client
                        command.result = items;
                    }
                }
            }
        }
    }
}
