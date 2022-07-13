using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Authorization
{
    public interface ISignManager
    {
        public LoginResult SignInMicrosoft(string login, string password);
        public LoginResult SignIn();
        public LoginResult SignInMojang(string login, string password);
        public bool SessionCheck(string uuid, string accesstoken, string serverhash);
    }
}
