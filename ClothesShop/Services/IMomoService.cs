using ClothesShop.Models;

namespace ClothesShop.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponse> CreatePaymentAsync(Order model);
        Task<MomoCreatePaymentResponse> RefundAsync(Order order);
    }

}
