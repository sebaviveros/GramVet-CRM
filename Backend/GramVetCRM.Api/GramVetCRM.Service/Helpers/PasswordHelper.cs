using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GramVetCRM.Service.Helpers
{
    internal class PasswordHelper
    {
    }
}
﻿using BCrypt.Net;

namespace GramVetCRM.Service.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
