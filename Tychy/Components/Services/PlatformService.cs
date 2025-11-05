namespace Tychy.Components.Services
{
    public class PlatformService
    {
        private readonly AppDbContext _context;

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
    }
}
