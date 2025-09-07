using HtmlAgilityPack; // NuGet: HtmlAgilityPack
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeebookConnector
{
    public class AulaConnector
    {
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer;
        private readonly string _aulaApiBaseUrl = "https://www.aula.dk/api/v22/";

        public AulaConnector()
        {
            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<bool> LoginAsync(string username, SecureString password, CancellationToken cancellationToken = default)
        {
            string url = "https://login.aula.dk/auth/login.php?type=unilogin";

            using HttpRequestMessage req = new(HttpMethod.Get, url);
            //AddBrowserHeaders(req, includeOrigin: true, originNull: true, fetchSite: "same-origin");
            Helpers.AddBrowserHeaders(req);

            var response = await _httpClient.SendAsync(req, cancellationToken);
            string html = await response.Content.ReadAsStringAsync();

            bool success = false;
            int counter = 0;

            while (!success && counter < 10)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var form = doc.DocumentNode.SelectSingleNode("//form");
                if (form == null) break;

                var action = form.GetAttributeValue("action", "");
                if (string.IsNullOrEmpty(action)) break;

                var inputs = form.SelectNodes(".//input");
                var data = new Dictionary<string, string>();

                if (inputs != null)
                {
                    foreach (var input in inputs)
                    {
                        var name = input.GetAttributeValue("name", null);
                        if (name == null) continue;

                        if (name == "username")
                            data[name] = username;
                        else if (name == "password")
                            data[name] = Helpers.ConvertSecureStringToString(password) ?? "";
                        else
                            data[name] = input.GetAttributeValue("value", "");
                    }
                }
                else
                {
                    if (counter == 0)
                    {
                        // Vi er på siden hvor man vælger unilogin.
                        data.TryAdd("selectedIdp", "uni_idp");
                    }
                }

                var postUrl = action.StartsWith("http") ? action : new Uri(new Uri(url), action).ToString();
                postUrl = WebUtility.HtmlDecode(postUrl);
                var content = new FormUrlEncodedContent(data);

                response = await _httpClient.PostAsync(postUrl, content);

                // Hvis vi ender på Aula-portalen, er vi logget ind
                if (response.RequestMessage?.RequestUri?.ToString().StartsWith("https://www.aula.dk/portal") == true)
                {
                    success = true;
                    break;
                }

                html = await response.Content.ReadAsStringAsync();
                counter++;
            }

            return success;
        }

        public async Task<TokenModel> GetJwtAsync()
        {
            TokenModel tokenModel = new();
            // Kald et API for at udløse cookies/session
            var response = await _httpClient.GetAsync($"{_aulaApiBaseUrl}?method=aulaToken.getAulaToken&widgetId=0119");//072
            if (!response.IsSuccessStatusCode) return null;

            // jwt token returneres i JSON-responsen
            var json = await response.Content.ReadAsStringAsync();

            // Loop through all cookies and find one named Csrfp-Token
            foreach (Cookie cookie in _cookieContainer.GetAllCookies())
            {
                if (cookie.Name.Equals("Csrfp-Token", StringComparison.OrdinalIgnoreCase))
                {
                    tokenModel.CsrfpToken = cookie.Value;
                }
            }

            var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
            if (jsonDoc.RootElement.TryGetProperty("data", out var jwtToken))
            {
                tokenModel.JwtToken = jwtToken.GetString();
            }

            return tokenModel;
        }

        public async Task<string?> GetProfilesByLogin()
        {
            var response = await _httpClient.GetAsync($"{_aulaApiBaseUrl}?method=profiles.getProfilesByLogin");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();

            return json;
        }

        public async Task<ProfileContext?> GetProfileContext()
        {
            var response = await _httpClient.GetAsync($"{_aulaApiBaseUrl}?method=profiles.getProfileContext");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();

            return AulaModels.ConvertProfileContext(json);
        }

        public async Task<List<CalenderEvent>?> GetImportantDates()
        {
            var response = await _httpClient.GetAsync($"{_aulaApiBaseUrl}?method=calendar.getImportantDates&limit=11&include_today=false");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            CalenderRoot? calenderRoot = System.Text.Json.JsonSerializer.Deserialize<CalenderRoot>(json, jsonSerializerOptions);

            return calenderRoot?.Data;
        }
    }
}

