using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeebookConnector
{
    public class MeebookApiConnector
    {
        private HttpClient _httpClient;
        private CookieContainer _cookieContainer;
        private readonly string _apiBaseUrl = "https://app.meebook.com/";
        private TokenModel _tokenModel;

        public MeebookApiConnector(TokenModel tokenModel)
        {
            _tokenModel = tokenModel;
            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.GZip
                                      | DecompressionMethods.Deflate
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<bool> Connect(string username, string childUniloginName, int institutionCode, CancellationToken cancellationToken = default)
        {
            DateTime today = DateTime.UtcNow;

            // Bruger ISO8601 (uge starter mandag, første uge er den der har mindst 4 dage i det nye år)
            var cal = CultureInfo.InvariantCulture.Calendar;
            var weekRule = CalendarWeekRule.FirstFourDayWeek;
            var firstDayOfWeek = DayOfWeek.Monday;
            int weekNumber = cal.GetWeekOfYear(today, weekRule, firstDayOfWeek);

            string url = $"{_apiBaseUrl}aulaapi/relatednotifications?currentWeekNumber={today.Year.ToString()}-W{weekNumber}&userProfile=guardian&childFilter[]={childUniloginName}&institutionFilter[]=280166";

            using HttpRequestMessage req = new(HttpMethod.Get, url);
            // Tilføj standard browser headers via hjælper
            Helpers.AddBrowserHeaders(
                req,
                includeOrigin: true,
                originNull: false,
                origin: "https://www.aula.dk/",
                fetchSite: "cross-site"
            );

            // Autorisation
            req.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenModel.JwtToken);

            // Fjern unødvendige headers
            req.Headers.Remove("Sec-Fetch-Dest");
            req.Headers.Remove("Upgrade-Insecure-Requests");
            req.Headers.Remove("Sec-Fetch-User");
            req.Headers.Remove("Sec-Fetch-Mode");

            // Tilføj tilpassede headers
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            req.Headers.TryAddWithoutValidation("sessionUUID", username);
            req.Headers.TryAddWithoutValidation("X-Version", "1.0");

            // Accept + referer
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br, zstd");
            req.Headers.Referrer = new Uri("https://www.aula.dk/");

            var response = await _httpClient.SendAsync(req, cancellationToken);
            string html = await response.Content.ReadAsStringAsync();

            return true;
        }

        public async Task<bool> LoadAuth(string username, string childUniloginName, int institutionCode, CancellationToken cancellationToken = default)
        {
            string url = $"{_apiBaseUrl}aulaapi/relatednotificationslinkauth/";
            using HttpRequestMessage req = new(HttpMethod.Post, url);
            Helpers.AddBrowserHeaders(req, includeOrigin: true, originNull: false, origin: "https://www.aula.dk/", fetchSite: "cross-site");
            req.Headers.Add("Referer", $"https://www.aula.dk/");

            DateTime today = DateTime.UtcNow;
            var cal = CultureInfo.InvariantCulture.Calendar;
            var weekRule = CalendarWeekRule.FirstFourDayWeek;
            var firstDayOfWeek = DayOfWeek.Monday;
            int weekNumber = cal.GetWeekOfYear(today, weekRule, firstDayOfWeek);
            string currentWeekNumber = $"{today.Year.ToString()}-W{weekNumber}";
            
            string rawUrl = "https://app.meebook.com/foraeldre/dashboard/?aulaJWT=" + _tokenModel.JwtToken;
            string encodedOnce = Uri.EscapeDataString(rawUrl);

            var data = new Dictionary<string, string>();
            data.TryAdd("type", "linkauth");
            data.TryAdd("sessionUUID", username);
            data.TryAdd("userProfile", "guardian");
            data.TryAdd("returnUrl", encodedOnce);
            data.TryAdd("currentWeekNumber", currentWeekNumber);
            data.TryAdd("institutionFilter[]", institutionCode.ToString());
            data.TryAdd("childFilter[]", childUniloginName);
            data.TryAdd("aulaJWT", _tokenModel.JwtToken);
            data.TryAdd("Csrfp-Token", _tokenModel.CsrfpToken);

            var content = new FormUrlEncodedContent(data);
            req.Content = content;

            var response = await _httpClient.SendAsync(req, cancellationToken);
            string html = await response.Content.ReadAsStringAsync();

            return true;
        }

        public async Task<bool> LoadStudentIds(List<Student> students, StudentRepository studentRepository, CancellationToken cancellationToken = default)
        {
            if (students?.All(s => s.StudentId != 0) ?? false)
            {
                return true;
            }

            string url = $"{_apiBaseUrl}rest/related/students";

            using HttpRequestMessage req = new(HttpMethod.Get, url);
            SetMeebookGetRequestHeaders(req);

            var response = await _httpClient.SendAsync(req, cancellationToken);
            string json = await response.Content.ReadAsStringAsync();

            // Deserialize JSON
            dynamic itemArray = JObject.Parse(json);
            foreach (var item in itemArray.items)
            {
                int id = item.id;
                string name = item.name;
                var student = students.FirstOrDefault(s => s.FullName == name);
                if (student != null)
                {
                    if (student.StudentId == 0)
                    {
                        student.StudentId = id;
                        studentRepository.InsertOrUpdate(student);
                    }
                }
            }

            return true;
        }

        public async Task<AulaPlanResponse?> GetWeekPlan(int weekNumber = 0, CancellationToken cancellationToken = default)
        {
            DateTime today = DateTime.UtcNow;

            if (weekNumber <= 0)
            {
                weekNumber = Helpers.GetWeekNumber(today);
            }

            var startDate = Helpers.FirstDateOfWeek(today.Year, weekNumber);

            var endDate = startDate.AddDays(4); // Fredag

            string url = $"{_apiBaseUrl}rest/weekplan/events?startDate={startDate.ToString("yyyy-MM-dd")}&endDate={endDate.ToString("yyyy-MM-dd")}";
            
            using HttpRequestMessage req = new(HttpMethod.Get, url);
            SetMeebookGetRequestHeaders(req);
            
            var response = await _httpClient.SendAsync(req, cancellationToken);
            
            string jsonString = await response.Content.ReadAsStringAsync();

            var aulaPlanResponse = AulaModels.ConvertAulaPlanResponse(jsonString);

            return aulaPlanResponse;
        }

        private void SetMeebookGetRequestHeaders(HttpRequestMessage req)
        {
            // Grundlæggende browser headers
            Helpers.AddBrowserHeaders(req, includeOrigin: false, fetchSite: "same-origin");

            // Accept headers
            req.Headers.Accept.Clear();
            req.Headers.Accept.ParseAdd("*/*");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br, zstd");

            // Referer
            req.Headers.Referrer = new Uri(
                $"https://app.meebook.com/foraeldre/dashboard/?aulaJWT={_tokenModel.JwtToken}"
            );

            // Ryd op i uønskede headers
            req.Headers.Remove("Sec-Fetch-Dest");
            req.Headers.Remove("Upgrade-Insecure-Requests");
            req.Headers.Remove("Sec-Fetch-User");
            req.Headers.Remove("Sec-Fetch-Mode");

            // Fetch metadata (Meebook forventer disse værdier)
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");

            // Connection
            req.Headers.Connection.Clear();
            req.Headers.Connection.Add("keep-alive");

            // Content type
            req.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        }
    }
}
