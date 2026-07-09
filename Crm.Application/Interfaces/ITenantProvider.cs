namespace Crm.Application.Interfaces
{
    public interface ITenantProvider
    {
        string GetTenantId();        
        string GetUserId();
    }
}