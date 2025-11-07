
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;

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
                    var nextRun = GetNextFirstDayOfMonth();
                    var delay = nextRun - DateTime.UtcNow;
                    Console.WriteLine("Delay: " + delay);
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
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex + " Błąd w MonthlyCheckService");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
        private DateTime GetNextFirstDayOfMonth()
        {
            var now = DateTime.UtcNow;
            var nextMonth = now.Month == 12 ? new DateTime(now.Year + 1, 1, 1)
                                            : new DateTime(now.Year, now.Month + 1, 1);
            return nextMonth;

            //return DateTime.UtcNow.AddSeconds(30);
        }

        private async Task ExecuteMonthlyTask(AppDbContext context)
        {
            Console.WriteLine("execute");
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine("execute-try");
                //Znosi UnusedCode po miesiącu
                var readersToReset = await context.Readers
                .Where(r => r.HasUnusedCodeLastMonth)
                .ToListAsync();

                foreach (var reader in readersToReset)
                {
                    reader.HasUnusedCodeLastMonth = false;
                }

                //Unieważnia przeterminowane kody
                var expiredCodes = await context.Codes
                .Include(c => c.Reader)
                .Where(c => c.Deadline != null && c.Deadline <= DateTime.UtcNow && c.IsValid)
                .ToListAsync();

                foreach (var code in expiredCodes)
                {
                    code.IsValid = false;
                    if (code.Reader != null)
                    {
                        code.Reader.HasUnusedCodeLastMonth = true;
                    }
                }

                await context.SaveChangesAsync();

                //Wysyła maila
                var readersToNotify = await context.Readers
                .Include(r => r.Request)
                    .ThenInclude(req => req.AssignedCode)
                .Include(r => r.Request)
                    .ThenInclude(req => req.Platform)
                .Where(r => !r.IsBlocked && !r.HasUnusedCodeLastMonth &&
                           r.Request != null && r.Request.AssignedCode != null)
                .ToListAsync();

                foreach (var reader in readersToNotify)
                {
                    await SendEmailToReader(reader);
                }
                //await SendEmailToReader(new Reader
                //{
                //    Request = new CodeRequest()
                //    {
                //        AssignedCode = new EbookCode
                //        {
                //            Code = "naleśniki"
                //        },
                //        Platform = new EbookPlatform()
                //        {
                //            Instructions = "Ugotuj naleśnika"
                //        }
                //    },
                //    Email ="m.jerchel12@gmail.com"
                //});

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
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

                using var client = new SmtpClient(smtpServer, port)
                {
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(username),
                    Subject = "Kod ebook",
                    Body = $"Twój kod to: {reader.Request.AssignedCode.Code}. {reader.Request.Platform?.Instructions ?? ""}",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(reader.Email);

                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"Mail wysłany do: {reader.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex} Błąd podczas wysyłania emaila do: {reader.Email}");
            }
        }
    }
}
