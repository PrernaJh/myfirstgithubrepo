using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Models.JobOptions;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMS.API.Domain.Processors
{
    public class JobScanProcessor : IJobScanProcessor
    {
        private readonly ILogger<JobScanProcessor> logger;
        private readonly IJobRepository jobRepository;
        private readonly IJobOptionRepository jobOptionRepository;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly ISequenceProcessor sequenceProcessor;
        private readonly ISiteProcessor siteProcessor;

        public JobScanProcessor(ILogger<JobScanProcessor> logger,
                IJobRepository jobRepository,
                IJobOptionRepository jobOptionRepository,
                ISubClientProcessor subClientProcessor,
                ISequenceProcessor sequenceProcessor,
                ISiteProcessor siteProcessor)
        {
            this.logger = logger;
            this.jobRepository = jobRepository;
            this.jobOptionRepository = jobOptionRepository;
            this.subClientProcessor = subClientProcessor;
            this.sequenceProcessor = sequenceProcessor;
            this.siteProcessor = siteProcessor;
        }

        public async Task<bool> GetJobDataForPackageScan(Package package)
        {
            var isJobAssigned = false;
            var job = await jobRepository.GetJobAsync(package.SiteName, package.JobBarcode);

            if (StringHelper.DoesNotExist(job.Id) || package.SubClientName != job.SubClientName)
            {
                logger.LogError($"Invalid job for job Barcode: {job.JobBarcode} and package ID: {package.PackageId}");
            }
            else
            {
                package.JobId = job.Id;
                package.Product = job.Product;
                package.BillOfLading = job.BillOfLading;
                package.IsMarkUpTypeCompany = job.MarkUpType == JobConstants.MarkUpTypeCompany;
                package.MarkUpType = job.MarkUpType == "COMPANY" ? "FSC" :
                        (job.MarkUpType == "CUSTOMER" ? "CUST" : job.MarkUpType);
                AssignPackageSizes(package, job);

                if (StringHelper.Exists(package.Product) && package.Product != ServiceConstants.Irregular)
                {
                    OverrideMailCodeWithJobMailCode(package, job);
                }

                isJobAssigned = true;
            }

            return isJobAssigned;
        }

        public async Task<bool> GetJobDataForCreatePackageScan(Package package)
        {
            var isJobAssigned = false;
            var job = await jobRepository.GetJobAsync(package.SiteName, package.JobBarcode);

            if (StringHelper.DoesNotExist(job.Id) || package.SubClientName != job.SubClientName)
            {
                logger.LogError($"Invalid job for job Id: {package.JobBarcode} and package ID: {package.PackageId}");
            }
            else
            {
                var shouldAssignJobDimensions = package.Length == 0 || package.Width == 0 || package.Depth == 0;

                if (shouldAssignJobDimensions)
                {
                    package.Length = job.Length;
                    package.Width = job.Width;
                    package.Depth = job.Depth;
                    package.TotalDimensions = package.Width * package.Length * package.Depth;
                }

                package.JobId = job.Id;
                isJobAssigned = true;
            }

            return isJobAssigned;
        }

        public async Task<GetJobOptionResponse> GetJobOptionAsync(string siteName)
        {
            var response = new GetJobOptionResponse();
            var jobOption = await jobOptionRepository.GetJobOptionsBySiteAsync(siteName);

            response.CustomerLocations.AddRange(jobOption.CustomerLocations);
            response.MarkUpTypes.AddRange(jobOption.MarkUpTypes);
            response.MarkUps.AddRange(jobOption.MarkUps);
            response.Products.AddRange(jobOption.Products);
            response.PackageTypes.AddRange(jobOption.PackageTypes);
            response.PackageDescriptions.AddRange(jobOption.PackageDescriptions);
            response.JobContainerTypes.AddRange(jobOption.JobContainerTypes);

            return response;
        }

        public async Task<AddJobResponse> AddJobAsync(AddJobRequest request)
        {
            var response = new AddJobResponse();
            var site = await siteProcessor.GetSiteBySiteNameAsync(request.SiteName);
            var subclient = await subClientProcessor.GetSubClientByNameAsync(request.CustomerLocation.Value);
            var sequence = await sequenceProcessor.ExecuteGetSequenceProcedure(site.SiteName, SequenceTypeConstants.Job, SequenceTypeConstants.SixDigitMaxSequence);
            var paddedSequence = sequence.Number.ToString().PadLeft(6, '0');
            var dateString = DateTime.Now.ToString("yyyyMMddHHmm");
            var jobBarcode = $"{request.CustomerLocation.CustomerType}{dateString}000{paddedSequence}";

            var lengthInput = StringHelper.Exists(request.PackageDescription.Length) ? request.PackageDescription.Length : "0";
            var widthInput = StringHelper.Exists(request.PackageDescription.Width) ? request.PackageDescription.Width : "0";
            var depthInput = StringHelper.Exists(request.PackageDescription.Depth) ? request.PackageDescription.Depth : "0";

            decimal.TryParse(lengthInput, out var length);
            decimal.TryParse(widthInput, out var width);
            decimal.TryParse(depthInput, out var depth);

            var jobToAdd = new Job
            {
                SiteName = request.SiteName,
                JobBarcode = jobBarcode,
                ManifestDate = request.ManifestDate,
                ClientName = subclient.ClientName,
                SubClientName = subclient.Name,
                SubClientDescription = request.CustomerLocation.ValueDescription,
                MarkUpType = request.MarkUpType.Value,
                MarkUp = request.MarkUp.Value,
                Product = request.Product.Value,
                PackageType = request.PackageType.Value,
                PackageDescription = request.PackageDescription.Value,
                Length = length,
                Width = width,
                Depth = depth,
                MailTypeCode = request.Product.MailTypeCode,
                Reference = request.Reference,
                BillOfLading = request.BillOfLading,
                SerialNumber = request.SerialNumber,
                Username = request.Username,
                MachineId = request.MachineId,
                JobContainers = request.JobContainers,
                CreateDate = DateTime.Now
            };

            GetJobLabelData(jobToAdd, site);
            var job = await jobRepository.AddItemAsync(jobToAdd, jobBarcode);

            if (StringHelper.Exists(job.Id))
            {
                response.JobBarcode = job.JobBarcode;
                response.LabelTypeId = job.LabelTypeId;
                response.LabelFieldValues.AddRange(job.LabelFieldValues);
            }

            return response;
        }

        public async Task<StartJobResponse> GetStartJob(StartJobRequest request)
        {
            var response = new StartJobResponse();
            var job = await jobRepository.GetJobAsync(request.SiteName, request.JobBarcode);

            if (StringHelper.Exists(job.Id))
            {
                var eventCount = job.JobEvents?.Count() ?? 0;
                job.JobEvents.Add(new Event
                {
                    EventId = eventCount += 1,
                    EventType = EventConstants.JobScan,
                    EventStatus = EventConstants.JobStarted,
                    Description = "Job Started by System",
                    Username = request.Username,
                    MachineId = request.MachineId,
                    EventDate = DateTime.Now
                });

                await jobRepository.UpdateItemAsync(job);

                response = new StartJobResponse
                {
                    JobBarcode = job.JobBarcode,
                    Product = job.Product,
                    MarkUp = job.MarkUp,
                    ManifestDate = job.ManifestDate,
                    BillOfLading = job.BillOfLading,
                    Reference = job.Reference,
                    SerialNumber = job.SerialNumber,
                    ReceivedAt = job.CreateDate.ToString(),
                    PackageDimensions = GenerateDisplayPackageDimensions(job),
                    IsSuccessful = true
                };
            }
            else
            {
                response.IsSuccessful = false;
                logger.LogError($"Job not found. SiteName {request.SiteName} JobBarcode {request.JobBarcode}");
            }

            return response;
        }

        private Job GetJobLabelData(Job job, Site site)
        {
            job.LabelTypeId = LabelTypeIdConstants.JobReceivingTicket;

            var containerQuantity = 0;
            var containerWeight = 0m;
            job.JobContainers.ForEach(x => containerQuantity += x.NumberOfContainers);
            job.JobContainers.ForEach(x => containerWeight += ParseContainerWeight(x));

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 0,
                FieldValue = site.Description
            });

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 1,
                FieldValue = job.JobBarcode
            });

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 2,
                FieldValue = job.Product
            });

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 3,
                FieldValue = job.SubClientDescription
            });

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 4,
                FieldValue = job.ManifestDate
            });

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 5,
                FieldValue = containerQuantity.ToString()
            });

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 6,
                FieldValue = containerWeight.ToString()
            });

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 7,
                FieldValue = job.BillOfLading
            });

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 8,
                FieldValue = $"{ job.Length } x { job.Width } x { job.Depth }"
            });

            job.LabelFieldValues.Add(new LabelFieldValue
            {
                Position = 9,
                FieldValue = $"{job.PackageType} - {job.PackageDescription}"
            });

            return job;
        }

        private static void OverrideMailCodeWithJobMailCode(Package package, Job job)
        {
            package.OverrideMailCode = package.MailCode;
            package.MailCode = job.MailTypeCode;
        }

        private void AssignPackageSizes(Package package, Job job)
        {
            package.Length = job.Length;
            package.Width = job.Width;
            package.Depth = job.Depth;
            package.TotalDimensions = job.Length + job.Width + job.Depth;
        }

        private static string GenerateDisplayPackageDimensions(Job job)
        {
            var response = $"{job.Length} x {job.Width}";

            if (job.Depth != 0)
            {
                response += $" x {job.Depth}";
            }

            return response;
        }

        private static decimal ParseContainerWeight(JobContainer container)
        {
            decimal.TryParse(container.Weight, out var response);
            return response;
        }
    }
}
