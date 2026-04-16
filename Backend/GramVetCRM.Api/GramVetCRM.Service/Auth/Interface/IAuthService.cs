using GramVetCRM.Model.DTOs.Auth;

namespace GramVetCRM.Service
{
    public interface IAuthService
    {
        Task<string?> Login(LoginRequestDto request);
    }
}