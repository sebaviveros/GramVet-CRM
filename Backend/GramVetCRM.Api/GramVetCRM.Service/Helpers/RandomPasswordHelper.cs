using System.Security.Cryptography;

namespace GramVetCRM.Service.Helpers
{
    public static class RandomPasswordHelper
    {
        private const string Mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string Minusculas = "abcdefghijkmnpqrstuvwxyz";
        private const string Numeros = "23456789";
        private const string Simbolos = "!@#$%&*";

        public static string Generar(int longitud = 10)
        {
            const string todos = Mayusculas + Minusculas + Numeros + Simbolos;

            // Garantizar al menos uno de cada tipo
            var chars = new List<char>
            {
                Tomar(Mayusculas),
                Tomar(Minusculas),
                Tomar(Numeros),
                Tomar(Simbolos)
            };

            for (int i = chars.Count; i < longitud; i++)
                chars.Add(Tomar(todos));

            // Mezclar
            return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }

        private static char Tomar(string fuente)
        {
            return fuente[RandomNumberGenerator.GetInt32(fuente.Length)];
        }
    }
}
