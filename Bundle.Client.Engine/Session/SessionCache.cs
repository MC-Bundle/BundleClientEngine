using Bundle.Client.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Bundle.Client.Session
{
    public static class SessionCache
    {
        private const string SessionCacheFilePlaintext = "SessionCache.ini";
        private const string SessionCacheFileSerialized = "SessionCache.db";
        private static readonly string SessionCacheFileMinecraft = String.Concat(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Path.DirectorySeparatorChar,
            ".minecraft",
            Path.DirectorySeparatorChar,
            "launcher_profiles.json"
        );

        private static Dictionary<string, UserSession> sessions = new Dictionary<string, UserSession>();
        private static System.Timers.Timer updatetimer = new System.Timers.Timer(100);
        private static List<KeyValuePair<string, UserSession>> pendingadds = new List<KeyValuePair<string, UserSession>>();
        private static BinaryFormatter formatter = new BinaryFormatter();

        /// <summary>
        /// Retrieve whether SessionCache contains a session for the given login.
        /// </summary>
        /// <param name="login">User login used with Minecraft.net</param>
        /// <returns>TRUE if session is available</returns>
        public static bool Contains(string login)
        {
            return sessions.ContainsKey(login);
        }

        /// <summary>
        /// Store a session and save it to disk if required.
        /// </summary>
        /// <param name="login">User login used with Minecraft.net</param>
        /// <param name="session">User session token used with Minecraft.net</param>
        public static void Store(string login, UserSession session)
        {
            if (Contains(login))
            {
                sessions[login] = session;
            }
            else
            {
                sessions.Add(login, session);
            }

            if (updatetimer.Enabled == true)
            {
                pendingadds.Add(new KeyValuePair<string, UserSession>(login, session));
            }
            else
            {
                SaveToDisk();
            }
        }

        /// <summary>
        /// Retrieve a session token for the given login.
        /// </summary>
        /// <param name="login">User login used with Minecraft.net</param>
        /// <returns>SessionToken for given login</returns>
        public static UserSession Get(string login)
        {
            return sessions[login];
        }

        /// <summary>
        /// Initialize cache monitoring to keep cache updated with external changes.
        /// </summary>
        /// <returns>TRUE if session tokens are seeded from file</returns>
        public static bool InitializeDiskCache()
        {
            updatetimer.Elapsed += HandlePending;
            return LoadFromDisk();
        }

        /// <summary>
        /// Reloads cache on external cache file change.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event data</param>
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            updatetimer.Stop();
            updatetimer.Start();
        }

        /// <summary>
        /// Called after timer elapsed. Reads disk cache and adds new/modified sessions back.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event data</param>
        private static void HandlePending(object sender, ElapsedEventArgs e)
        {
            updatetimer.Stop();
            LoadFromDisk();

            foreach (KeyValuePair<string, UserSession> pending in pendingadds.ToArray())
            {
                Store(pending.Key, pending.Value);
                pendingadds.Remove(pending);
            }
        }

        /// <summary>
        /// Reads cache file and loads SessionTokens into SessionCache.
        /// </summary>
        /// <returns>True if data is successfully loaded</returns>
        private static bool LoadFromDisk()
        {
            //Grab sessions in the Minecraft directory
            if (File.Exists(SessionCacheFileMinecraft))
            {
                JsonConv.JSONData mcSession = new JsonConv.JSONData(JsonConv.JSONData.DataType.String);
                try
                {
                    mcSession = JsonConv.ParseJson(File.ReadAllText(SessionCacheFileMinecraft));
                }
                catch (IOException) { /* Failed to read file from disk -- ignoring */ }
                if (mcSession.Type == JsonConv.JSONData.DataType.Object
                    && mcSession.Properties.ContainsKey("clientToken")
                    && mcSession.Properties.ContainsKey("authenticationDatabase"))
                {
                    Guid temp;
                    string clientID = mcSession.Properties["clientToken"].StringValue.Replace("-", "");
                    Dictionary<string, JsonConv.JSONData> sessionItems = mcSession.Properties["authenticationDatabase"].Properties;
                    foreach (string key in sessionItems.Keys)
                    {
                        if (Guid.TryParseExact(key, "N", out temp))
                        {
                            Dictionary<string, JsonConv.JSONData> sessionItem = sessionItems[key].Properties;
                            if (sessionItem.ContainsKey("displayName")
                                && sessionItem.ContainsKey("accessToken")
                                && sessionItem.ContainsKey("username")
                                && sessionItem.ContainsKey("uuid"))
                            {
                                string login = sessionItem["username"].StringValue.ToLower();
                                try
                                {
                                    UserSession session = UserSession.FromToken(String.Join(",",
                                        sessionItem["accessToken"].StringValue,
                                        sessionItem["displayName"].StringValue,
                                        sessionItem["uuid"].StringValue.Replace("-", ""),
                                        clientID
                                    ));
                                    sessions[login] = session;
                                }
                                catch (InvalidDataException) { /* Not a valid session */ }
                            }
                        }
                    }
                }
            }

            //Serialized session cache file in binary format
            if (File.Exists(SessionCacheFileSerialized))
            {
                try
                {
                    using (FileStream fs = new FileStream(SessionCacheFileSerialized, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        Dictionary<string, UserSession> sessionsTemp = (Dictionary<string, UserSession>)formatter.Deserialize(fs);
                        foreach (KeyValuePair<string, UserSession> item in sessionsTemp)
                        {
                            sessions[item.Key] = item.Value;
                        }
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                }
                catch (SerializationException ex2)
                {
                    Console.WriteLine(ex2);
                }
            }

            //User-editable session cache file in text format
            if (File.Exists(SessionCacheFilePlaintext))
            {
                try
                {
                    foreach (string line in FileMonitor.ReadAllLinesWithRetries(SessionCacheFilePlaintext))
                    {
                        if (!line.Trim().StartsWith("#"))
                        {
                            string[] keyValue = line.Split('=');
                            if (keyValue.Length == 2)
                            {
                                try
                                {
                                    string login = keyValue[0].ToLower();
                                    UserSession session = UserSession.FromToken(keyValue[1]);
                                    sessions[login] = session;
                                }
                                catch (InvalidDataException e)
                                {
                                }
                            }
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                }
            }

            return sessions.Count > 0;
        }

        /// <summary>
        /// Saves SessionToken's from SessionCache into cache file.
        /// </summary>
        private static void SaveToDisk()
        {

            List<string> sessionCacheLines = new List<string>();
            sessionCacheLines.Add("# Login=SessionID,PlayerName,UUID,ClientID");
            foreach (KeyValuePair<string, UserSession> entry in sessions)
                sessionCacheLines.Add(entry.Key + '=' + entry.Value.ToString());

            try
            {
                FileMonitor.WriteAllLinesWithRetries(SessionCacheFilePlaintext, sessionCacheLines);
            }
            catch (IOException e)
            {
                Console.WriteLine("лучший текст в мире");
            }
        }
    }
}
