using Bundle.Client.Json;
using Bundle.Client.Options;
using Bundle.Runtime;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Bundle.Client.Authorization
{
    public class MinecraftWithXbox
    {

        private readonly ITcpClientFactory _tcpClientFactory;
        private readonly MinecraftWithXboxOptions _minecraftWithXboxOptions;

        public MinecraftWithXbox(ITcpClientFactory tcpClientFactory, IOptions<MinecraftWithXboxOptions> minecraftWithXboxOptions)
        {
            _tcpClientFactory = tcpClientFactory;
            _minecraftWithXboxOptions = minecraftWithXboxOptions.Value;
        }

        public string LoginWithXbox(string userHash, string xstsToken)
        {
            var request = new ProxiedWebRequest(_minecraftWithXboxOptions.LoginWithXbox, _tcpClientFactory);
            request.Accept = "application/json";

            string payload = "{\"identityToken\": \"XBL3.0 x=" + userHash + ";" + xstsToken + "\"}";
            var response = request.Post("application/json", payload);

            string jsonString = response.Body;
            JsonConv.JSONData json = JsonConv.ParseJson(jsonString);
            return json.Properties["access_token"].StringValue;
        }


        public bool UserHasGame(string accessToken)
        {
            var request = new ProxiedWebRequest(_minecraftWithXboxOptions.Ownership, _tcpClientFactory);
            request.Headers.Add("Authorization", string.Format("Bearer {0}", accessToken));
            var response = request.Get();

            string jsonString = response.Body;
            JsonConv.JSONData json = JsonConv.ParseJson(jsonString);
            return json.Properties["items"].DataArray.Count > 0;
        }

        public UserProfile GetUserProfile(string accessToken)
        {
            var request = new ProxiedWebRequest(_minecraftWithXboxOptions.Profile, _tcpClientFactory);
            request.Headers.Add("Authorization", string.Format("Bearer {0}", accessToken));
            var response = request.Get();


            string jsonString = response.Body;
            JsonConv.JSONData json = JsonConv.ParseJson(jsonString);
            return new UserProfile()
            {
                UUID = json.Properties["id"].StringValue,
                UserName = json.Properties["name"].StringValue
            };
        }
    }


    public struct UserProfile
    {
        public string UUID;
        public string UserName;
    }
}
