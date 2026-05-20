namespace Clarence.Domain;

public class Client { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public string RentManagerAccountId { get; set; } = string.Empty; }
public class Property { public Guid Id { get; set; } public Guid ClientId { get; set; } public string AddressLine1 { get; set; } = string.Empty; public string City { get; set; } = string.Empty; public string State { get; set; } = "NJ"; public string PostalCode { get; set; } = string.Empty; }
public class Tenant { public Guid Id { get; set; } public string FullName { get; set; } = string.Empty; public string Unit { get; set; } = string.Empty; }
public class LegalCase
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid TenantId { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal BalanceOwed { get; set; }
    public decimal LateFees { get; set; }
    public decimal CourtCosts { get; set; }
    public DateOnly? NextDeadline { get; set; }
    public CaseStage Stage { get; set; } = CaseStage.NewSubmission;
}

public class RentManagerSubmission
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid TenantId { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal BalanceOwed { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public string VerificationStatus { get; set; } = "Verified";
    public bool Accepted { get; set; }
}

public class CaseTimelineEvent
{
    public Guid Id { get; set; }
    public Guid LegalCaseId { get; set; }
    public DateTime AtUtc { get; set; }
    public CaseStage Stage { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public enum CaseStage { NewSubmission, UnderReview, Accepted, FormsGenerated, SentForFiling, Filed, Scheduled, Served, CourtAppearance, OutcomeEntered, Settled, Dismissed, Judgment, Closed }
