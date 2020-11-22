namespace GSC.Shared
{
    public interface IJwtTokenAccesser
    {
        int UserId { get; }
        string UserName { get; }
        int CompanyId { get; }
        int RoleId { get; }
        string IpAddress { get; }
        string GetHeader(string key);
    }
}