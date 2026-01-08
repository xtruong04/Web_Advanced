using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
namespace ClothesShop.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Thông tin này PHẢI chính xác
            string fromMail = "xuantruong26082004@gmail.com";
            string appPassword = "eufcredggwzzxfba"; // 16 ký tự viết liền, không dấu cách

            var message = new MailMessage();
            message.From = new MailAddress(fromMail, "Clothes Shop");
            message.To.Add(new MailAddress(email));
            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;

            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true; // Phải bật SSL trước
                smtpClient.UseDefaultCredentials = false; // Phải set False trước khi gán Credentials
                smtpClient.Credentials = new NetworkCredential(fromMail, appPassword);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                // Quan trọng: Thêm Try-Catch để nếu lỗi mail, website không bị sập (lỗi trang vàng)
                try
                {
                    await smtpClient.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    // Ghi lỗi ra cửa sổ Output để bạn đọc, nhưng vẫn cho người dùng đi tiếp
                    System.Diagnostics.Debug.WriteLine("Lỗi gửi mail: " + ex.Message);
                }
            }
        }
    }
}
