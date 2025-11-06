namespace Tychy.Components.Services
{
    public class RequestService
    {
        internal readonly AppDbContext _context;

        public RequestService(AppDbContext context) {
            _context = context;
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
    }
}
