using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Data.Constants
{
    public static class ContainerErrorConstants
    {
        public const string PackageNotFound = "Scan001"; // Package not found	
        public const string ContainerAlreadyAssigned = "Scan002"; // Container already assigned.
        public const string ContainerNotFound = "Scan003"; // Container not found.
        public const string ContainerIncorrectState = "Scan004"; // Container in incorrect state
        public const string ActiveContainerNotFound = "Scan005"; // Active container not found.
        public const string ContainerEmpty = "Scan006"; // Container is empty
        public const string OldContainerNotFound = "Scan007"; // Old container not found.
        public const string OldContainerIncorrectState = "Scan008"; // Old container in incorrect state
        public const string NewContainerNotFound = "Scan009"; // New container not found.
        public const string NewContainerIncorrectState = "Scan010"; // New container in incorrect state
        public const string ContainerIncorrectOperation = "Scan011"; 
        public const string Exception = "Error001"; // Exception
    }
}
