using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Authorization
{
    public class UserSign
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public AccountType AccountType { get; set; }
    }
}
