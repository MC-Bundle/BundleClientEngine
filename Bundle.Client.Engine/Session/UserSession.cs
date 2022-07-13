using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bundle.Client.Session
{
    [Serializable]
    public class UserSession
    {
        private static readonly Regex JwtRegex = new Regex("^[A-Za-z0-9-_]+\\.[A-Za-z0-9-_]+\\.[A-Za-z0-9-_]+$");

        public UserSession()
        {

        }

        public UserSession(string id, string userName, string uuid, string clientId, string refreshToken)
        {
            SessionId = id;
            UserName = userName;
            Uuid = uuid;
            ClientId = clientId;
            RefreshToken = refreshToken;
        }

        public virtual string SessionId { get; set; }
        public string UserName { get; set; }
        public string Uuid { get; set; }
        public string ClientId { get; set; }
        public string RefreshToken { get; set; }

        public override string ToString()
        {
            return String.Join(",", SessionId, UserName, Uuid, ClientId, RefreshToken);
        }

        public static UserSession FromToken(string tokenString)
        {
            string[] fields = tokenString.Split(',');
            if (fields.Length < 4)
                throw new InvalidDataException("Invalid string format");

            UserSession session = new UserSession();
            session.SessionId = fields[0];
            session.UserName = fields[1];
            session.Uuid = fields[2];
            session.ClientId = fields[3];
            if (fields.Length > 4)
                session.RefreshToken = fields[4];
            else
                session.RefreshToken = String.Empty;

            Guid temp;
            if (!JwtRegex.IsMatch(session.SessionId))
                throw new InvalidDataException("Invalid session ID");
            //if (!ChatBot.IsValidName(session.PlayerName))
            //    throw new InvalidDataException("Invalid player name");
            if (!Guid.TryParseExact(session.Uuid, "N", out temp))
                throw new InvalidDataException("Invalid player ID");
            if (!Guid.TryParseExact(session.ClientId, "N", out temp))
                throw new InvalidDataException("Invalid client ID");

            return session;
        }

        public static UserSession FromCache(string login)
        {
            UserSession session = new UserSession();
            bool cacheLoaded = SessionCache.InitializeDiskCache();
            if (cacheLoaded)
            {
                if (SessionCache.Contains(login.ToLower()))
                {
                    return SessionCache.Get(login.ToLower());
                }
            }

            return new UserSession();
        }
    }
}
