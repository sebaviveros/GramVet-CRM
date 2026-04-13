using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Repository.Context;

namespace GramVetCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioTestController : ControllerBase
    {
        private readonly GramVetDbContext _context;

        public UsuarioTestController(GramVetDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var usuarios = _context.Usuario.ToList();
            return Ok(usuarios);
        }
    }
}