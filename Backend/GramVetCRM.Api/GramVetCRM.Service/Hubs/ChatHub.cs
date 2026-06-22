using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GramVetCRM.Service.Hubs
{
    // Nombres de grupos para emisión dirigida por rol
    public static class ChatGroups
    {
        // Admin y Secretario: reciben todos los eventos
        public const string Staff = "staff";
        // Veterinario: solo eventos de sus conversaciones asignadas
        public static string Usuario(int usuarioId) => $"user-{usuarioId}";
    }

    [Authorize]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            var rol = (user?.FindFirst("rolNombre")?.Value ?? "").ToLower();
            var idStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (rol.Contains("veterinario") && idStr != null)
            {
                // Veterinario: solo su grupo personal
                await Groups.AddToGroupAsync(Context.ConnectionId, ChatGroups.Usuario(int.Parse(idStr)));
            }
            else
            {
                // Admin / Secretario (y cualquier no-veterinario): ven todo
                await Groups.AddToGroupAsync(Context.ConnectionId, ChatGroups.Staff);
            }

            await base.OnConnectedAsync();
        }
    }
}
