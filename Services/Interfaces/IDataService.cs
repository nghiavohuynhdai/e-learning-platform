namespace Cursus.Services.Interfaces;

public interface IDataService
{
    Task FindAndUpdateExpiredStatusOrders();
}