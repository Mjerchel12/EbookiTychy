namespace Tychy.Components.Services
{
    public class PlatformService
    {
        internal readonly AppDbContext _context;

        public PlatformService(AppDbContext context) {
            _context = context;
        }

        public async Task<bool> AddPlatform(string platName, string instruct)
        {
            _context.Platforms.Add(new EbookPlatform
            {
                Name = platName,
                Instructions = instruct
            });

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> AddCode(string code)
        {
            _context.Codes.Add(new EbookCode
            {
                Code = code
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
