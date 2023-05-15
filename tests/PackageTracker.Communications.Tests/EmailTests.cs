using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PackageTracker.Communications.Tests
{
    [TestClass]
    public class EmailTests
    {
        [TestMethod]
        public void SendSimple()
        {
            var json = File.ReadAllText("EmailConfiguration.json");
            var emailConfiguration = JsonUtility<EmailConfiguration>.Deserialize(json);
            var emailService = new EmailService(emailConfiguration);
            var email = new EmailMessage();
            emailConfiguration.ExceptionsEmailContactList.Where(x => StringHelper.Exists(x)).ToList()
                .ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
            if (email.ToAddresses.Any())
            {
                email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
                email.Subject = "Test";
                email.Content = "SendSimple";
                emailService.Send(email);
            }

        }

        [TestMethod]
        public void SendSimpleWithTextFile()
        {
            var json = File.ReadAllText("EmailConfiguration.json");
            var emailConfiguration = JsonUtility<EmailConfiguration>.Deserialize(json);
            var emailService = new EmailService(emailConfiguration);
            var email = new EmailMessage();
            emailConfiguration.ExceptionsEmailContactList.Where(x => StringHelper.Exists(x)).ToList()
                .ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
            if (email.ToAddresses.Any())
            {
                email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
                email.Subject = "Test with Attachment";
                email.Content = "See Attachment!";
                var attachments = new List<EmailAttachment>();
                attachments.Add(new EmailAttachment
                {
                    MimeType = MimeTypeConstants.PLAIN_TEXT,
                    FileName = "TextFile.txt",
                    FileContents = Encoding.ASCII.GetBytes("This is a text file!\n")
                });
                emailService.SendAsync(email, false, attachments).Wait();
            }

        }

        [TestMethod]
        public void SendSimpleWithSpreadsheet()
        {
            var json = File.ReadAllText("EmailConfiguration.json");
            var emailConfiguration = JsonUtility<EmailConfiguration>.Deserialize(json);
            var emailService = new EmailService(emailConfiguration);
            var email = new EmailMessage();
            emailConfiguration.ExceptionsEmailContactList.Where(x => StringHelper.Exists(x)).ToList()
                .ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
            if (email.ToAddresses.Any())
            {
                email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
                email.Subject = "Test with Attachment";
                email.Content = "See Attachment!";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var ws = new ExcelWorkSheet("Worksheet", new string [] {
                    "Column Header1", "Column Header2", "Column Header3", "Column Header4", 
                });
                for (var i = 0; i < 10; i++)
                {
                    ws.InsertRow(ws.RowCount + 1, new string[] {
                        Guid.NewGuid().ToString(),  Guid.NewGuid().ToString(),  Guid.NewGuid().ToString(),  Guid.NewGuid().ToString(),
                    });
                }
                var attachments = new List<EmailAttachment>();
                attachments.Add(new EmailAttachment
                {
                    MimeType = MimeTypeConstants.OPEN_OFFICE_SPREADSHEET,
                    FileName = "Spreadsheet.xlsx",
                    FileContents = ws.GetContentsAsync().Result
                });
                emailService.SendAsync(email, false, attachments).Wait();
            }
        }

        [TestMethod]
        public void SendSimpleWithSpreadsheetWithFormulas()
        {
            var json = File.ReadAllText("EmailConfiguration.json");
            var emailConfiguration = JsonUtility<EmailConfiguration>.Deserialize(json);
            var emailService = new EmailService(emailConfiguration);
            var email = new EmailMessage();
            emailConfiguration.ExceptionsEmailContactList.Where(x => StringHelper.Exists(x)).ToList()
                .ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
            if (email.ToAddresses.Any())
            {
                email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
                email.Subject = "Test with Attachment";
                email.Content = "See Attachment!";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var ws = new ExcelWorkSheet("Worksheet", new string[] {
                    "Column Header1", "Column Header2", "Column Header3", "Column Header4",
                });
                var dataTypes = new eDataTypes[] { eDataTypes.Number, eDataTypes.Number, eDataTypes.Number, eDataTypes.Number };
                for (var i = 0; i < 10; i++)
                {
                    var row = ws.RowCount + 1;
                    ws.InsertRow(row, new string[] {
                        row.ToString(),  (row*2).ToString(),  (row*3).ToString(),  (row*4).ToString(),
                    }, dataTypes);
                }
                var totalRow = ws.RowCount + 1;
                ws.InsertRow(totalRow, new string[] { "0", "0", "0", "0" }, dataTypes);
                ws.InsertFormula($"A{totalRow}", $"SUM(A2:A{totalRow - 1})");
                ws.InsertFormula($"B{totalRow}", $"SUM(B2:B{totalRow - 1})");
                ws.InsertFormula($"C{totalRow}", $"SUM(C2:C{totalRow - 1})");
                ws.InsertFormula($"D{totalRow}", $"SUM(D2:D{totalRow - 1})");
                var attachments = new List<EmailAttachment>();
                attachments.Add(new EmailAttachment
                {
                    MimeType = MimeTypeConstants.OPEN_OFFICE_SPREADSHEET,
                    FileName = "Spreadsheet.xlsx",
                    FileContents = ws.GetContentsAsync().Result
                });
                emailService.SendAsync(email, false, attachments).Wait();
            }
        }

        [TestMethod]
        public void SendSimpleWithSpreadsheetWithHyperlinks()
        {
            var json = File.ReadAllText("EmailConfiguration.json");
            var emailConfiguration = JsonUtility<EmailConfiguration>.Deserialize(json);
            var emailService = new EmailService(emailConfiguration);
            var email = new EmailMessage();
            emailConfiguration.ExceptionsEmailContactList.Where(x => StringHelper.Exists(x)).ToList()
                .ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
            if (email.ToAddresses.Any())
            {
                email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
                email.Subject = "Test with Attachment";
                email.Content = "See Attachment!";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var ws = new ExcelWorkSheet("Worksheet", new string[] {
                    "Column Header1",
                });
                var row = ws.RowCount + 1;
                ws.InsertHyperlink($"A{row}", "Google", "https://google.com");

                var attachments = new List<EmailAttachment>();
                attachments.Add(new EmailAttachment
                {
                    MimeType = MimeTypeConstants.OPEN_OFFICE_SPREADSHEET,
                    FileName = "Spreadsheet.xlsx",
                    FileContents = ws.GetContentsAsync().Result
                });
                emailService.SendAsync(email, false, attachments).Wait();
            }
        }

        [TestMethod]
        public void SendHtml()
        {
            var json = File.ReadAllText("EmailConfiguration.json");
            var emailConfiguration = JsonUtility<EmailConfiguration>.Deserialize(json);
            var emailService = new EmailService(emailConfiguration);
            var email = new EmailMessage();
            emailConfiguration.ExceptionsEmailContactList.Where(x => StringHelper.Exists(x)).ToList()
                .ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
            if (email.ToAddresses.Any())
            {
                email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
                email.Subject = "Test HTML";
                email.Content = File.ReadAllText("SimpleHtmlEmail.html");
                emailService.SendAsync(email, true).Wait();
            }
        }
    }
}
