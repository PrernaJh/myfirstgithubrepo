using System;
using System.Collections.Generic;
using System.Text;

namespace ManifestBuilder
{
    public class CreateManifestResponse
    {
        public Dictionary<string, int> EFNEndSequenceByMID;

        public List<string> EvsRecords;

        public bool IsSuccessful;

        public string errorMessage;
    }
}
