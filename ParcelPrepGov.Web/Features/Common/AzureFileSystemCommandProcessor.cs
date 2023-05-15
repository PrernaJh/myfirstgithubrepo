using DevExtreme.AspNet.Mvc.FileManagement;
using System;

namespace ParcelPrepGov.Web.Features.Common
{
    public class AzureFileSystemCommandProcessor : FileSystemCommandProcessor
    {
        public FileSystemConfiguration _fileSystemConfig { get; set; }
        public AzureFileSystemCommandProcessor(FileSystemConfiguration fileSystemConfig) : base(fileSystemConfig)
        {
            _fileSystemConfig = fileSystemConfig;
        }

        public new FileSystemCommandResult Execute(FileSystemCommand cmd, string args)
        {
            try
            {
                var execute = base.Execute(cmd, args);
                return execute;
            }
            catch
            {
                throw;
            }
        }
    }
}
