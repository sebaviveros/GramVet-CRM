using GramVetCRM.Model.DTOs.Auth;
using GramVetCRM.Repository.Repositories;
using GramVetCRM.Service.Helpers;

namespace GramVetCRM.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _repository;
        private readonly JwtHelper _jwtHelper;

        public AuthService(IUsuarioRepository repository, JwtHelper jwtHelper)
        {
            _repository = repository;
            _jwtHelper = jwtHelper;
        }

        public async Task<string?> Login(LoginRequestDto request)
        {
            var usuario = await _repository.GetByUsername(request.Username);

            if (usuario == null)
                return null;

            var isValid = PasswordHelper.Verify(request.Password, usuario.PasswordHash);

            if (!isValid)
                return null;

            var token = _jwtHelper.GenerarToken(usuario);

            return token;
        }
    }
}