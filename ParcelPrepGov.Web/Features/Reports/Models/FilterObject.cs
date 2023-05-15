using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ParcelPrepGov.Web.Features.Reports.Models
{
    public static class FilterObject
    {
        private static void UpdateFilterDictionary(IDictionary<string, string> filter, string columnName, string value)
        {
            if (filter.TryGetValue(columnName, out var values))
            {
                values += "|" + value;
            }
            else
            {
                values = value;
            }
            filter[columnName] = values.Replace("||", "|");
        }

        /// <summary>
        /// takes a filter from devexpress javascript method; var combinedFilter = $("#reportsGrid").dxDataGrid("getCombinedFilter", true);
        /// and returns a dictionary of key column and value a comma separated list
        /// </summary>
        /// <param name="json">
        ///     Tested Example: "[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"]" 
        ///     Tested Example: [[[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ABINGDON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"AIKEN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"BARDSTOWN\"], \"and\",[\"CARRIER\",\"=\",\"REGIONAL CARRIER\"]]]
        ///     Tested Example: [[["ENTRY_UNIT_NAME","=",null],"or",["ENTRY_UNIT_NAME","=",""]],"or",["ENTRY_UNIT_NAME","=","ABERDEEN"]]
        ///     Tested Example: [[[\"USPS_AREA\",\"=\",\"Atlantic\"],\"and\",[[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ABINGDON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ARLINGTON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"CARROLL\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"CHARLESTON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"CLIFTON EAST END\"]]]]
        ///     </param>
        /// <returns></returns>
        public static IDictionary<string, string> ParseFilterString(string theString)
        {
            IDictionary<string, string> results = null;
            if (!string.IsNullOrEmpty(theString) && theString != "undefined" && theString.StartsWith("["))
            {
                results = new Dictionary<string, string>();
                if (!theString.StartsWith("[["))
                    theString = "[" + theString + "]";
                var filterObject = JArray.Parse(theString);
                foreach (var item in filterObject.Children())
                {
                    if (item.Type == JTokenType.Array)
                    {
                        if (item.First.Type == JTokenType.Array)
                        {
                            foreach (var subItem in item.Children())
                            {
                                if (subItem.Type == JTokenType.Array)
                                {
                                    UpdateFilterDictionary(results, subItem.First.ToString(), subItem.Last.ToString());
                                }
                            }
                        }
                        else
                        {
                            UpdateFilterDictionary(results, item.First.ToString(), item.Last.ToString());
                        }
                    }
                }
            }
            return results;
        }
    }
}