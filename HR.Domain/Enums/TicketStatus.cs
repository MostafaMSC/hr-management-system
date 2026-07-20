namespace HR.Domain.Enums;

public enum TicketStatus
{
    Open,
    WaitingManagerApproval,
    WaitingITAdminVerification,
    InProgress,
    WaitingUserVerification,
    Resolved,
    Closed
}
