using System;
using System.Collections.Generic;
using System.Text;
using NeoGeo.Library.SMB.Provider;
namespace lokkju.iPx.iPhoneDrive
{
    class AuthHandler : NeoGeo.Library.SMB.Provider.AuthenticationProvider
    {
        private string authtype = "local";
        public override string AuthenticationProviderType
        {
            get
            {
                return authtype;
            }
        }
        public override bool Logon(string UserName, AuthenticationType Type, out byte[] PasswordHash, out UserContext UserContext)
        {
            UserContext = new UserContext(UserName,"");
            PasswordHash = null;
            return true;        // Es wird jeder andere Username akzeptiert, nur für DEBUG Zwecke !!!!
        }
    }
}
