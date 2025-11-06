
using System.Net.Mail;
using System.Net;

namespace Tychy.Components.Services
{
    public class MonthlyCheckService:BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        public MonthlyCheckService(IServiceScopeFactory scopeFactory, IConfiguration configuration) {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Oblicz czas do następnego pierwszego dnia miesiąca
                    var nextRun = GetNextFirstDayOfMonth();
                    var delay = nextRun - DateTime.UtcNow;

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        await ExecuteMonthlyTask(context);
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
            var now = DateTime.UtcNow;
            var nextMonth = now.Month == 12 ? new DateTime(now.Year + 1, 1, 1)
                                            : new DateTime(now.Year, now.Month + 1, 1);
            return nextMonth;
        }

        private async Task ExecuteMonthlyTask(AppDbContext context)
        {
            foreach(Reader r in context.Readers)
            {
                if (r.HasUnusedCodeLastMonth)
                {
                    r.HasUnusedCodeLastMonth = false;
                    await context.SaveChangesAsync();
                }
            }
            foreach (EbookCode c in context.Codes)
            { 
                if(c.Deadline != null && c.Deadline <= DateTime.UtcNow)
                {
                    c.IsValid = false;
                    c.Reader.HasUnusedCodeLastMonth = true;
                    await context.SaveChangesAsync();
                }
            }
            foreach (Reader r in context.Readers)
            {
                if (!r.IsBlocked && !r.HasUnusedCodeLastMonth)
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
                            Body = "Twój kod to: " + r.Request.AssignedCode.Code + ". " + r.Request.Platform.Instructions,
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
