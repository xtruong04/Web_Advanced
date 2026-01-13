using System.Net.Http.Json;
using ClothesShop.Models;

namespace ClothesShop.Services
{
    public class MomoService : IMomoService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;

        public MomoService(IConfiguration config, HttpClient client)
        {
            _config = config;
            _client = client;
        }

        // 👉 SỬA: Truyền vào cả Model Order để lấy ID
        public async Task<MomoCreatePaymentResponse> CreatePaymentAsync(Order order)
        {
            var momo = _config.GetSection("MomoAPI");

            var orderId = order.Id.ToString() + "_" + DateTime.Now.Ticks.ToString();
            var requestId = Guid.NewGuid().ToString();
            long rawAmount = (long)order.TotalAmount;

            // Kiểm tra an toàn: Nếu nhỏ hơn 1000 thì set cứng thành 10.000đ
            if (rawAmount < 1000)
            {
                rawAmount = 10000;
            }

            // --- SỬA LỖI Ở ĐÂY: Chuyển long sang string để dùng cho hash và json ---
            var amountInt = rawAmount.ToString();

            var orderInfo = "Thanh toan don hang #" + orderId;

            // Tạo rawHash (Lưu ý: thứ tự tham số phải chuẩn a-z)
            string rawHash =
                $"accessKey={momo["AccessKey"]}" +
                $"&amount={amountInt}" +
                $"&extraData=" +
                $"&ipnUrl={momo["NotifyUrl"]}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&partnerCode={momo["PartnerCode"]}" +
                $"&redirectUrl={momo["ReturnUrl"]}" +
                $"&requestId={requestId}" +
                $"&requestType={momo["RequestType"]}";

            string signature = MomoHelper.HmacSHA256(rawHash, momo["SecretKey"]);

            var request = new
            {
                partnerCode = momo["PartnerCode"],
                accessKey = momo["AccessKey"],
                requestId = requestId,
                amount = amountInt,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = momo["ReturnUrl"],
                ipnUrl = momo["NotifyUrl"],
                extraData = "",
                requestType = momo["RequestType"],
                signature = signature,
                lang = "vi"
            };

            // Gọi API MoMo
            var response = await _client.PostAsJsonAsync(momo["MomoApiUrl"], request);

            // Đọc kết quả trả về
            return await response.Content.ReadFromJsonAsync<MomoCreatePaymentResponse>();
        }
        public async Task<MomoCreatePaymentResponse> RefundAsync(Order order)
        {
            var momo = _config.GetSection("MomoAPI");

            // Tạo mã requestId và orderId mới cho giao dịch hoàn tiền này
            var requestId = Guid.NewGuid().ToString();
            var refundOrderId = "Refund_" + order.Id + "_" + DateTime.Now.Ticks;

            // transId là mã giao dịch gốc cần hoàn tiền (đã lưu ở bước 1)
            var transId = long.Parse(order.MomoTransId);

            // Số tiền cần hoàn (có thể hoàn 1 phần hoặc toàn bộ)
            long amount = (long)order.TotalAmount;

            // 1. Tạo Raw Hash cho Refund (Thứ tự tham số khác với lúc thanh toán)
            // accessKey, amount, description, orderId, partnerCode, requestId, transId
            string rawHash =
                $"accessKey={momo["AccessKey"]}" +
                $"&amount={amount}" +
                $"&description=Hoan tien don hang #{order.Id}" +
                $"&orderId={refundOrderId}" +
                $"&partnerCode={momo["PartnerCode"]}" +
                $"&requestId={requestId}" +
                $"&transId={transId}";

            // 2. Ký tên (Signature)
            string signature = MomoHelper.HmacSHA256(rawHash, momo["SecretKey"]);

            // 3. Tạo Body Request
            var request = new
            {
                partnerCode = momo["PartnerCode"],
                orderId = refundOrderId,
                requestId = requestId,
                amount = amount,
                transId = transId,
                lang = "vi",
                description = $"Hoan tien don hang #{order.Id}",
                signature = signature
            };

            // 4. Gửi request lên endpoint Refund của MoMo
            // Lưu ý: URL refund thường là: https://test-payment.momo.vn/v2/gateway/api/refund
            var response = await _client.PostAsJsonAsync("https://test-payment.momo.vn/v2/gateway/api/refund", request);

            return await response.Content.ReadFromJsonAsync<MomoCreatePaymentResponse>();
        }
    }
}