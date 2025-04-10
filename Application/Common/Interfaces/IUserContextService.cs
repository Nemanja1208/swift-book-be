namespace Application.Common.Interfaces
{
    public interface IUserContextService
    {
        string? UserId { get; }
        string? IpAddress { get; }
        string? RequestPath { get; }
    }
}
