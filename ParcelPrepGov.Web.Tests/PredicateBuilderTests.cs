using DevExpress.Data.Filtering;
using DevExtreme.AspNet.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParcelPrepGov.Reports.Data;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Reports.Repositories;
using ParcelPrepGov.Web.Features.Bulletin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace ParcelPrepGov.Web.Tests
{
    public class PredicateBuilderTests
    {
        private ILogger<PackageDatasetRepository> logger;
        private IConfiguration configuration;
        private System.Data.IDbConnection connection;
        private IRecallStatusRepository recallStatusRepository;


        /// <summary>
        /// this test is for expression extension of linq queries
        /// </summary>
        [Fact]
        public void Test_Expression_For_Dynamic_Linq_Somewhat()
        {
            try
            {
                // arrange
                #region setup context
                PpgReportsDbContextFactory factory = new PpgReportsDbContextFactory();
                var builder = new DbContextOptionsBuilder<PpgReportsDbContext>();
                var context = factory.CreateDbContext();
                #endregion

                // build the expression as queryable, query is not executed at this point
                IQueryable<PackageDataset> buildFilterExpression = context.PackageDatasets
                    .IsSubClient("CMOPMURFREESBORO").ForProduct("SELECT ALL").ForArea("PELHAM");

                // arrange skip take for pagination
                int skip = 0;
                int take = 10;

                // act
                var results = buildFilterExpression
                                .Skip(skip)
                                .Take(take).ToList();

                //assert
                Assert.NotNull(results);


                // another way to do it


            }
            catch (FilterException filterException)
            {
                throw filterException;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        public void TestExpressionWhereClause()
        {
            // arrange
            #region setup context
            PpgReportsDbContextFactory factory = new PpgReportsDbContextFactory();
            var builder = new DbContextOptionsBuilder<PpgReportsDbContext>();
            var context = factory.CreateDbContext();
            #endregion

            int skip = 0;
            int take = 10;

            var queryable = context.PackageDatasets.Where(Filter("PELHAM", "CMOPMURFREESBORO", "PSLW"));
            var retVal = queryable.Skip(skip)
                            .Take(take).ToList();

            Assert.NotNull(retVal);
        }

        [Fact]
        public void Test_String_To_Arrays()
        {
            try
            {
                // problem statement
                var theString = "[[[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ABINGDON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"AIKEN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"BARDSTOWN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"BARTLETT\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"BARBOURSVILLE\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"BROOKLYN SOUTH\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"BRUNSWICK\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"BURLINGTON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"DESOTO\"]],\"and\",[\"CARRIER\",\"=\",\"REGIONAL CARRIER\"]]";
                var result = ParseFilterString(theString);
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                var str = result["ENTRY_UNIT_NAME"];
                Assert.NotNull(str);
                Assert.Equal("ABERDEEN|ABINGDON|AIKEN|BARDSTOWN|BARTLETT|BARBOURSVILLE|BROOKLYN SOUTH|BRUNSWICK|BURLINGTON|DESOTO", str);
                var str1 = result["CARRIER"];
                Assert.NotNull(str1);
                Assert.Equal("REGIONAL CARRIER", str1);

                var theString2 = "[[[\"ENTRY_UNIT_NAME\",\"=\",null],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"\"]],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"]]";
                var result2 = ParseFilterString(theString2);
                Assert.NotNull(result2);
                Assert.Equal(1, result2.Count());
                var str2 = result2["ENTRY_UNIT_NAME"];
                Assert.NotNull(str2);
                Assert.Equal("|ABERDEEN", str2);

                // test case is invalid as the data will not come in this way
                //var theString3 = "[[[\"ENTRY_UNIT_NAME\",\"=\",null],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"\"]]]";
                //var result3 = ParseFilterString(theString3);
                //Assert.NotNull(result3);
                //Assert.Equal(1, result3.Count());
                //var str3 = result3["ENTRY_UNIT_NAME"];
                //Assert.NotNull(str3);
                //Assert.Equal("", str3);

                var theString4 = "[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"]";
                var result4 = ParseFilterString(theString4);
                Assert.NotNull(result4);
                Assert.Equal(1, result4.Count());
                var str4 = result4["ENTRY_UNIT_NAME"];
                Assert.NotNull(str4);
                Assert.Equal("ABERDEEN", str4);

                var theString5 = "[[[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"MEMPHIS\"]]]";
                var result5 = ParseFilterString(theString5);
                Assert.NotNull(result5);
                Assert.Equal(1, result5.Count());
                var str5 = result5["ENTRY_UNIT_NAME"];
                Assert.NotNull(str5);
                Assert.Equal("ABERDEEN|MEMPHIS", str5);

                var theString6 = "[[[\"USPS_AREA\",\"=\",\"Atlantic\"],\"and\",[[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ABINGDON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ARLINGTON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"CARROLL\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"CHARLESTON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"CLIFTON EAST END\"]]]]";
                var result6 = ParseFilterString(theString6);
                Assert.NotNull(result6);
                Assert.Equal(2, result6.Count());

                var theString7 = "[[\"ENTRY_UNIT_TYPE\",\"=\",\"DDU\"],\"and\",[[\"USPS_AREA\",\"=\",\"Atlantic\"],\"or\",[\"USPS_AREA\",\"=\",\"Central\"]],\"and\",[[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ABINGDON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ANNSHIRE ANNEX\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"MID MISSOURI\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"SAINT ALBANS\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"WILLIAMSBURG\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"VINE GROVE\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"SPRINGFIELD\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"WALBROOK\"]]]";
                var result7 = ParseFilterString(theString7);
                Assert.NotNull(result7);
                Assert.Equal(3, result7.Count());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Fact]
        public void Test_PostalPerformanceSummar_Filter_String()
        {
            var theString = "[[[\"USPS_AREA\",\"=\",\"Atlantic\"],\"and\",[[\"ENTRY_UNIT_NAME\",\"=\",\"ABERDEEN\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ABINGDON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"ARLINGTON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"CARROLL\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"CHARLESTON\"],\"or\",[\"ENTRY_UNIT_NAME\",\"=\",\"CLIFTON EAST END\"]]]]";
            var result = ParseFilterString(theString);
            Assert.NotNull(result);
        }

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

        private static Expression<Func<PackageDataset, bool>> Filter(string area)
        {
            return x => x.City == area;
        }

        private static Expression<Func<PackageDataset, bool>> Filter(string area, string subClientName)
        {
            return x => x.City == area && x.SubClientName == subClientName;
        }
        private static Expression<Func<PackageDataset, bool>> Filter(string area, string subClientName, string productName)
        {
            return x => x.City == area && x.SubClientName == subClientName && x.ShippingMethod == productName;
        }
    }

    [DataContract]
    public class MyDetail
    {
        [DataMember]
        public string Column { get; set; }

        [DataMember]
        public string Operand { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
    public static class PackageFilters
    {
        public static IQueryable<PackageDataset> IsSubClient(this IQueryable<PackageDataset> packages, string subClientName)
        {
            return packages.Where(package => package.SubClientName == subClientName);
        }

        public static IQueryable<PackageDataset> ForProduct(this IQueryable<PackageDataset> packages, string productName)
        {
            if (productName == "SELECT ALL")
            {
                return null;
            }
            else
            {
                return packages.Where(package => package.ShippingMethod == productName);
            }
        }

        // area, unitName, unitCSZ, 
        public static IQueryable<PackageDataset> ForArea(this IQueryable<PackageDataset> packages, string area)
        {
            if (!string.IsNullOrEmpty(area))
            {
                return packages.Where(packages => packages.City == area);
            }
            return packages;
        }
    }
    public class FilterException : Exception
    {

    }
}
