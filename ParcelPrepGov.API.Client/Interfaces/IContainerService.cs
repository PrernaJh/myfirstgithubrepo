using ParcelPrepGov.API.Client.Data;

namespace ParcelPrepGov.API.Client.Interfaces
{
    public interface IContainerService
    {
        void PostAssignActiveContainer(ref AssignContainer container, AccountLogin loggedInAccount);
        void PostAssignNewContainer(ref AssignContainer container, AccountLogin loggedInAccount);
        void PostReplaceContainer(ref ReplaceContainer container, AccountLogin loggedInAccount);
    }
}
