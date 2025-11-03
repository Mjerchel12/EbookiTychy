using System.ComponentModel.DataAnnotations.Schema;

namespace Tychy.Components
{
    public enum CodeStatus
    {
        Available,
        Reserved,
        Sent,
        Returned
    }
    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected,
        EmailSent
    }
    public class EbookPlatform
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmailTemplate { get; set; }
        public string Instructions { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<EbookCode>? Codes { get; set; }
        public int MonthlyLimit { get; set; }

        [NotMapped]
        public int AvailableCodes => Codes?.Count(c => c.Status == CodeStatus.Available) ?? 0;
        [NotMapped]
        public int ReservedCodes => Codes?.Count(c => c.Status == CodeStatus.Reserved) ?? 0;
    }

    public class EbookCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int EbookPlatformId { get; set; }
        public EbookPlatform Platform { get; set; }
        public CodeStatus Status { get; set; } = CodeStatus.Available;
        public DateTime? ReservedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? UsedDate { get; set; }
        public DateTime? Deadline { get; set; }
        public Reader Reader { get; set; }
        public string ReservedForReaderId { get; set; }
        public string SentToEmail { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public bool IsValid => Deadline == null || Deadline > DateTime.UtcNow;

    }

    public class CodeRequest
    {
        public int Id { get; set; }
        public Reader Reader { get; set; }
        public string ReaderId { get; set; }
        public int EbookPlatformId { get; set; }
        public EbookPlatform Platform { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public int AssignedCodeId { get; set; }
        public EbookCode AssignedCode { get; set; }
        public string Email { get; set; }
        public string ValidationMessage { get; set; }
        public string RejectionReason { get; set; }
        public bool IsDuplicate { get; set; }
        public bool IsBannedThisMonth { get; set; }
    }

    public class Reader
    {
        public int Id { get; set; }
        public string ReaderId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockReason { get; set; }
        public DateTime BlockDate { get; set; }
        public DateTime BlockedUntil { get; set; }
        public bool HasUnusedCodeLastMonth { get; set; }

        public ICollection<CodeRequest> Requests { get; set; } = new List<CodeRequest>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActivity { get; set; }
    }
}
