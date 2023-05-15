namespace ParcelPrepGov.Web.Features.Models
{
    public interface IBusinessLogic<T>
    {
        public bool IsValid(T t);
    }
}