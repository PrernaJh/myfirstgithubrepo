using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Utilities
{
    public class FtpArgs
    {
        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Folder { get; set; }
    }

    public class SftpHelper : IDisposable
    {
        private readonly FtpArgs ftpArgs;
        private readonly SftpClient ftpClient;
        public SftpHelper(FtpArgs ftpArgs)
        {
            this.ftpArgs = ftpArgs;
            ftpClient = new SftpClient(ftpArgs.Host, ftpArgs.User, ftpArgs.Password);
            ftpClient.Connect();
        }

        public void Dispose()
        {
            ftpClient.Dispose();
        }

        public IList<string> ListFiles()
        {
            return ftpClient.ListDirectory(ftpArgs.Folder).Where(f => f.IsRegularFile).Select(f => f.ToString()).ToList();
        }

        public void UploadListOfStringsToFile(List<string> input, string fileName)
        {
            var byteArray = input.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
            using (var stream = ftpClient.OpenWrite($"{ftpArgs.Folder}/{fileName}"))
            {
                stream.Write(byteArray);
                stream.Close();
            }
        }

        public async Task UploadStreamToFileAsync(Stream input, string fileName)
        {
            using (var stream = ftpClient.OpenWrite($"{ftpArgs.Folder}/{fileName}"))
            {
                var reader = new StreamReader(input);
                var buffer = new char[10000];
                while (!reader.EndOfStream)
                {
                    var count = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
                    var bytes = Encoding.ASCII.GetBytes(buffer, 0, count);
                    stream.Write(bytes, 0, bytes.Length);
                }
                stream.Close();
            }
        }    
    }
}
