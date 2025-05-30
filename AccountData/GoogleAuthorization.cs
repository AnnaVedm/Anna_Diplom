using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Oauth2.v2.Data;

namespace TyutyunnikovaAnna_Diplom.Services
{
    public class GoogleAuthenticator
    {
        private readonly string[] Scopes = { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/userinfo.profile" };
        private const string ApplicationName = "HorseClubApp";

        public async Task<Userinfo> AuthenticateAsync()
        {
            string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GoogleAPI", "Clientsecret.json");
            if (!File.Exists(credPath))
                throw new FileNotFoundException($"Файл client_secret.json не найден: {credPath}");

            string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GoogleAuth", "Token");
            if (Directory.Exists(tokenPath))
            {
                Directory.Delete(tokenPath, true);  // Удаляем старые токены
            }

            using var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read);
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true));

            var service = new Oauth2Service(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            return await service.Userinfo.Get().ExecuteAsync();
        }
    }
}
