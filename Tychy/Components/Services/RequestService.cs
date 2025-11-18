using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
namespace Tychy.Components.Services
{
    public class RequestService
    {
        internal readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public RequestService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> MakeRequest(Reader reader, EbookPlatform platform)
        {
            _context.Requests.Add(new CodeRequest
            {
                Reader = reader,
                Platform = platform,
                RequestDate = DateTime.Now,
                Status = RequestStatus.Pending,
                Email = reader.Email
            });
            await _context.SaveChangesAsync();
            return true;
        }
        public void Approve(int reqId)
        {
            CodeRequest cr = _context.Requests.First(item => item.Id == reqId);
            cr.Status = RequestStatus.Approved;
        }
        public void Reject(int reqId)
        {
            CodeRequest cr = _context.Requests.First(item => item.Id == reqId);
            cr.Status = RequestStatus.Rejected;
        }
        public async void Send(int reqId)
        {
            CodeRequest cr = _context.Requests.First(item => item.Id == reqId);
            cr.Status = RequestStatus.EmailSent;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var readersToNotify = _context.Readers
                    .Include(r => r.Request)
                        .ThenInclude(req => req.AssignedCode)
                    .Include(r => r.Request)
                        .ThenInclude(req => req.Platform)
                    .Where(r => !r.IsBlocked && !r.HasUnusedCodeLastMonth &&
                                r.Request != null && r.Request.AssignedCode != null)
                    .ToList();

                foreach (var reader in readersToNotify)
                {
                    SendEmailToReader(reader);
                }
                await SendEmailToReader(new Reader
                {
                    Request = new CodeRequest()
                    {
                        AssignedCode = new EbookCode
                        {
                            Code = "naleśniki"
                        },
                        Platform = new EbookPlatform()
                        {
                            Instructions = "Ugotuj naleśnika"
                        }
                    },
                    Email = "m.jerchel12@gmail.com"
                });

                transaction.Commit();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex} Błąd podczas wysyłania emaila");
                transaction.Rollback();
                throw;
            }
        }
        private async Task SendEmailToReader(Reader reader)
        {
            Console.WriteLine("send");
            try
            {
                Console.WriteLine("send-try");
                var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
                Console.WriteLine("Server: " + smtpServer);
                var port = int.Parse(_configuration["Email:Port"] ?? "587");
                Console.WriteLine("Port: " + port);
                var username = _configuration["Email:Username"] ?? "";
                Console.WriteLine("Username: " + username);
                var password = _configuration["Email:Password"] ?? "";
                Console.WriteLine("Password: " + password);
                var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");
                Console.WriteLine("SSL: " + enableSsl);

                using var client = new MailKit.Net.Smtp.SmtpClient();

                await client.ConnectAsync(smtpServer, port, enableSsl ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.None);
                await client.AuthenticateAsync(username, password);

                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("", username));
                mailMessage.To.Add(new MailboxAddress("", reader.Email));
                mailMessage.Subject = "Kod ebook";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = $"Twój kod to: {reader.Request.AssignedCode.Code}. {reader.Request.Platform?.Instructions ?? ""}";
                mailMessage.Body = bodyBuilder.ToMessageBody();

                await client.SendAsync(mailMessage);
                await client.DisconnectAsync(true);

                Console.WriteLine($"Mail wysłany do: {reader.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex} Błąd podczas wysyłania emaila do: {reader.Email}");
            }
        }
        public async Task<bool> RemoveCode(int id)
        {
            _context.Requests.Remove(_context.Requests.First(item => item.Id == id));
            _context.Codes.Remove(_context.Requests.First(item => item.Id == id).AssignedCode);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
