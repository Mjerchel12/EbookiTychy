using Microsoft.EntityFrameworkCore;

namespace Tychy.Components.Services
{
    public class PlatformService
    {
        internal readonly AppDbContext _context;

        public PlatformService(AppDbContext context) {
            _context = context;
        }
        public async Task<List<EbookPlatform>> GetPlatformsAsync()
        {
            return await _context.Platforms
            .Include(u => u.Codes)
            .ToListAsync();
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
        public async Task<bool> AddCodeLegimi(string code)
        {
            _context.Codes.Add(new EbookCode
            {
                Code = code,
                Platform = _context.Platforms.First(item => item.Name == "Legimi")
            });

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> AddCodeEmpik(string code)
        {
            _context.Codes.Add(new EbookCode
            {
                Code = code,
                Platform = _context.Platforms.First(item => item.Name == "Empik GO")
            });

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> AddReader(byte numb, string readName, string mail)
        {
            _context.Readers.Add(new Reader
            {
                ReaderNumber = numb,
                FullName = readName,
                Email = mail
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
