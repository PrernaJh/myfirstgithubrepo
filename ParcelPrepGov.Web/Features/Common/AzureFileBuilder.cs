using DevExpress.Compatibility.System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParcelPrepGov.Web.Features.Common
{
    public class AzureFileBuilder
    {
        static AzureFileBuilder instance;

        // lock synchronization object
        private static object locker = new object();

        public static AzureFileBuilder GetFileBuilder()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new AzureFileBuilder();
                    }
                }
            }
            return instance;
        }
        /// <summary>
        /// figuring out which container is sending the request for folders or files
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns>the container we are looking at</returns>
        public string GetContainerName(string arguments)
        {
            // when containers with same folders appear, the query is confused which container
            var guid = JObject.Parse(arguments);

            if (guid != null)
            {
                //pathinfo is for folders
                var item = guid["pathInfo"];
                if (item != null)
                {
                    if (item.Children().Count() > 1)
                    {
                        var name = guid["pathInfo"][0]["name"].ToString().TrimEnd(new[] { '/' });
                        return name;
                    }
                    else if (item.Children().Count() > 0)
                    {
                        var value = guid["pathInfo"][0];
                        if (value != null)
                        {
                            var key = guid["pathInfo"][0]["key"];
                            if (key != null)
                            {
                                var container = key.ToString().TrimEnd(new[] { '/' });
                                if (container != null)
                                {
                                    var splitNameGuid = container.Split("|");
                                    var containerName = splitNameGuid.FirstOrDefault();
                                    if (containerName != null)
                                    {
                                        return containerName;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (guid["pathInfoList"] != null)
                {
                    // pathInfoList is to download files
                    var infoListItem = guid["pathInfoList"];
                    if (infoListItem != null)
                    {
                        if (infoListItem.Children().Count() > 0)
                        {
                            var value = guid["pathInfoList"][0];
                            if (value != null)
                            {
                                var key = guid["pathInfoList"][0][0]["key"];
                                if (key != null)
                                {
                                    var container = key.ToString();
                                    if (container != null)
                                    {
                                        var splitNameGuid = container.Split("|");
                                        var containerName = splitNameGuid.FirstOrDefault();
                                        if (containerName != null)
                                        {
                                            return containerName;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (guid["destinationPathInfo"] != null)
                {
                    var destinationInfo = guid["destinationPathInfo"];
                    if (destinationInfo != null)
                    {
                        if (destinationInfo.Children().Count() > 0)
                        {
                            var value = guid["destinationPathInfo"][0];
                            if (value != null)
                            {
                                var key = guid["destinationPathInfo"][0]["key"];
                                if (key != null)
                                {
                                    var container = key.ToString();
                                    if (container != null)
                                    {
                                        var splitNameGuid = container.Split("|");
                                        var containerName = splitNameGuid.FirstOrDefault();
                                        if (containerName != null)
                                        {
                                            return containerName;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }

        public string FixArgs(string arguments)
        {
            try
            {
                Rootobject rootitem = JsonConvert.DeserializeObject<Rootobject>(arguments);
                if (rootitem.pathInfo != null)
                {
                    if (rootitem.pathInfo.Count() == 1)
                    {
                        // we hit root 
                        return "{\"pathInfo\":[]}";
                    }
                    else if (rootitem.pathInfo.Count() > 1)
                    {
                        var arrays = rootitem.pathInfo.ToList();
                        arrays.RemoveAt(0);
                        var serializeItem = JsonConvert.SerializeObject(arrays);
                        var setObjectToReturn = serializeItem;
                        return "{\"pathInfo\":" + setObjectToReturn + "}";
                    }
                }
                else if (rootitem.pathInfoList != null)
                {
                    var arrays = rootitem.pathInfoList.ToList();
                    arrays[0].RemoveAt(0);
                    var serializeItem = JsonConvert.SerializeObject(arrays);
                    var setObjectToReturn = serializeItem;
                    return "{\"pathInfoList\":" + setObjectToReturn + "}";
                }
                else if (rootitem.destinationPathInfo != null)
                {
                    var newDestination = rootitem.destinationPathInfo.ToList();
                    newDestination.RemoveAt(0);
                    rootitem.destinationPathInfo = newDestination.ToArray();
                    var deserialize = JsonConvert.SerializeObject(rootitem);
                    return deserialize;
                }

                return arguments;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// combine arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public T[] Combine<T>(params IEnumerable<T>[] items) =>
            items.SelectMany(i => i).ToArray();

        /// <summary>
        /// combine the array of json commands from the .result
        /// property of the CommandResult
        /// </summary>
        /// <param name="commandResult">a json object with dynamic[]</param>
        /// <returns></returns>
        public dynamic[] RebuildArrayOfCommandResults(List<CommandResult> commandResult, int? numberOfDays = null)
        {          
            dynamic[] arr = null;
            if (commandResult.Count > 1)
            {
                dynamic[] temp = null;
                // process and combine array into one list of anonymous types
                for (int i = 0; i < commandResult.Count - 1; i++)
                {
                    if (temp == null)
                    {
                        temp = Combine(commandResult[i].result, commandResult[i + 1].result);
                    }
                    else
                    {
                        temp = Combine(temp, commandResult[i + 1].result);
                    }
                }
                arr = temp;
            }
            else
            {
                if (numberOfDays.HasValue)
                {
                    arr = commandResult[0].result.Where(x => (((DateTime)x.dateModified) > DateTime.Now.AddDays(-numberOfDays.Value)
                        && (!(bool)x.isDirectory))
                        || ((bool)x.isDirectory)).ToArray();
                }
                else
                {
                    arr = commandResult[0].result.ToArray();
                }
            }

            return arr;
        }

        /// <summary>
        /// only add single parent node  
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="command"></param>
        public void AddParentNode(JavaScriptSerializer serializer, CommandResult command, string nameIdentifier)
        {
            dynamic fakeParentFolder =
                            new
                            {
                                key = nameIdentifier,
                                name = nameIdentifier,
                                dateModified = "2021-09-13T15:02:16.3744243Z",
                                isDirectory = true,
                                size = 0,
                                hasSubDirectories = true,
                                items = command.result.ToList()
                            };

            command.result = new dynamic[] { fakeParentFolder };
        }


    }
}
