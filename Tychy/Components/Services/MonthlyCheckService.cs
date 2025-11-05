
using System.Net.Mail;
using System.Net;

namespace Tychy.Components.Services
{
    public class MonthlyCheckService:BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public MonthlyCheckService(AppDbContext context, IConfiguration configuration) {
            _configuration = configuration;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Oblicz czas do następnego pierwszego dnia miesiąca
                    var nextRun = GetNextFirstDayOfMonth();
                    var delay = nextRun - DateTime.Now;

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await ExecuteMonthlyTask();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex + " Błąd w MonthlyCheckService");
                }
            }
        }
        private DateTime GetNextFirstDayOfMonth()
        {
            var now = DateTime.Now;
            var nextMonth = now.Month == 12 ? new DateTime(now.Year + 1, 1, 1)
                                            : new DateTime(now.Year, now.Month + 1, 1);
            return nextMonth;
        }

        private async Task ExecuteMonthlyTask()
        {
            foreach (EbookCode c in _context.Codes)
            { 
                if(c.Deadline != null && c.Deadline <= DateTime.UtcNow)
                {
                    c.IsValid = false;
                    c.Reader.IsBlocked = true;
                }
            }
            foreach (Reader r in _context.Readers)
            {
                if (!r.IsBlocked)
                {
                    try
                    {
                        var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
                        var port = int.Parse(_configuration["Email:Port"] ?? "587");
                        var username = _configuration["Email:Username"] ?? "";
                        var password = _configuration["Email:Password"] ?? "";
                        var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

                        using var client = new SmtpClient(smtpServer, port)
                        {
                            EnableSsl = enableSsl,
                            Credentials = new NetworkCredential(username, password)
                        };

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(username),
                            Subject = "Kod ebook",
                            Body = "Twój kod to: " + r.Requests[0].AssignedCode.Code + ". " + r.Requests[0].Platform.Instructions,
                            IsBodyHtml = false
                        };

                        mailMessage.To.Add(r.Email);

                        await client.SendMailAsync(mailMessage);
                        Console.WriteLine("Mail wysłany");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex + "Błąd podczas wysyłania emaila do: " + r.Email);
                    }
                    await Task.CompletedTask;
                }
            }
        }
    }
}
