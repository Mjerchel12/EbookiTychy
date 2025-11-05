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
        public string? Name { get; set; } = string.Empty;
        public string? Instructions { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<EbookCode>? Codes { get; set; } = new List<EbookCode>();
        public int MonthlyLimit { get; set; }

        [NotMapped]
        public int AvailableCodes => Codes?.Count(c => c.Status == CodeStatus.Available) ?? 0;
        [NotMapped]
        public int ReservedCodes => Codes?.Count(c => c.Status == CodeStatus.Reserved) ?? 0;
    }

    public class EbookCode
    {
        public int Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public EbookPlatform? Platform { get; set; }
        public CodeStatus Status { get; set; } = CodeStatus.Available;
        public DateTime? ReservedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? UsedDate { get; set; }
        public DateTime? Deadline { get; set; }
        public Reader? Reader { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        public bool IsValid { get; set; }

    }

    public class CodeRequest
    {
        public int Id { get; set; }
        public Reader? Reader { get; set; }
        public EbookPlatform? Platform { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public EbookCode? AssignedCode { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? ValidationMessage { get; set; } = string.Empty;
        public string? RejectionReason { get; set; } = string.Empty;
        public bool IsDuplicate { get; set; }
        public bool IsBannedThisMonth { get; set; }
    }

    public class Reader
    {
        public int Id { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? FullName { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; } = string.Empty;
        public DateTime BlockDate { get; set; }
        public DateTime BlockedUntil { get; set; }
        public bool HasUnusedCodeLastMonth { get; set; }

        public List<CodeRequest> Requests { get; set; } = new List<CodeRequest>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActivity { get; set; }
    }
}
