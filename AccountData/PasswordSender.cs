using System;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using TyutyunnikovaAnna_Diplom.Models;
using BCrypt.Net;
using TyutyunnikovaAnna_Diplom.Context;
using MimeKit.Utils;
using System.IO;

namespace TyutyunnikovaAnna_Diplom.AccountData
{
    internal class PasswordSender
    {
        private readonly DiplomHorseClubContext _context;
        private const string SenderEmail = "equineex1899@gmail.com";
        private const string SenderPassword = "mxus weyx qvxb meyg"; 

        public PasswordSender(DiplomHorseClubContext context)
        {
            _context = context;
        }

        public async Task SendNewPasswordAsync(string recipientEmail)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == recipientEmail);

            string newPassword = GenerateTemporaryPassword();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            user.PasswordHash = hashedPassword;
            await _context.SaveChangesAsync();

            await SendEmailAsync(recipientEmail, newPassword);
        }

        private string GenerateTemporaryPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task SendEmailAsync(string recipientEmail, string newPassword)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Администратор конного клуба Espada", SenderEmail));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = "Восстановление пароля";

          


            var body = new BodyBuilder();

            // Embed the image
            var imagePath = @"C:\Users\Asus\Desktop\Фото диплома конверт\HorseMain.jpg"; // Replace with the actual path to your image
            var image = body.LinkedResources.Add(imagePath);
            image.ContentId = MimeUtils.GenerateMessageId(); // Generate a unique Content-ID

            body.HtmlBody = $@"
    <div style=""background-color: #FFFFFF; padding: 20px; border: 1px solid #DDDDDD;"">
        <h1 style=""color: #47A76A;"">Конный клуб Espada</h1>
        <img src=""cid:{image.ContentId}"" alt=""Embedded Image"" style=""max-width: 100%; height: auto;"" />
        <div style=""background-color: #47A76A; padding: 10px; color: #FFFFFF; border-radius: 10px;"">
            Новый пароль успешно сгенерирован: <strong>{newPassword}</strong>
        </div>
        <p>Мы напоминаем вам, что нужно хранить данный пароль в секрете и не разглашать свои учетные данные незнакомцам. Если вы всё-таки столкнулись с проблемой, мы ждем от вас ответное письмо.</p>
    </div>
";


            message.Body = body.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync("smtp.gmail.com", 465, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(SenderEmail, SenderPassword);
                    await client.SendAsync(message);
                    Console.WriteLine("Письмо успешно отправлено.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке письма: {ex.Message}");
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }

    }
}
