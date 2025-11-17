using Microsoft.EntityFrameworkCore;

namespace Tychy.Components.Services
{
    public class PlatformService
    {
        internal readonly AppDbContext _context;

        public PlatformService(AppDbContext context) {
            _context = context;
        }

        public async Task<bool> AddPlatform(string platName, int platLim, string instruct)
        {
            _context.Platforms.Add(new EbookPlatform
            {
                Name = platName,
                MonthlyLimit = platLim,
                Instructions = instruct,
                Codes = await _context.Codes
                    .Include(c => c.Platform)
                    .Where(item => item.Platform.Name == platName)
                    .ToListAsync()
            });

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> AddCode(string code, int platId)
        {
            _context.Codes.Add(new EbookCode
            {
                Code = code,
                Platform = _context.Platforms.First(item => item.Id == platId)
            });

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> AddReader(byte numb, string mail)
        {
            _context.Readers.Add(new Reader
            {
                ReaderNumber = numb,
                Email = mail
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
