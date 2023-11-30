using Cursus.Constants;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Services;

public class DataService : IDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public DataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task FindAndUpdateExpiredStatusOrders()
    {
        try
        {
            var expiredOrders = await _unitOfWork.OrderRepository
                .GetAllAsync(order => order.Status == Enum.GetName(OrderStatus.Pending) &&
                                      (int)(DateTime.UtcNow - order.CreatedDate).TotalMinutes > 30);

            if (expiredOrders.Count() != 0)
            {
                foreach (var order in expiredOrders)
                {
                    order.Status = Enum.GetName(OrderStatus.Expired);
                }

                await _unitOfWork.CommitAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        Console.WriteLine($"Clean Expired Orders At {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
    }
}