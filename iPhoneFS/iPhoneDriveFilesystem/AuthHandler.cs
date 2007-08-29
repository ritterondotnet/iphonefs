using System;
using System.Collections.Generic;
using System.Text;
using Suchwerk.Interfaces;
namespace lokkju.iPx.iPhoneDrive
{
    class AuthHandler : IAuth
    {
        public bool Logon(string UserName, AuthenticaionType Type, out byte[] PasswordHash, out object UserContext)
        {
            UserContext = UserName;
            PasswordHash = null;
            return true;        // Es wird jeder andere Username akzeptiert, nur für DEBUG Zwecke !!!!
        }
    }
}
