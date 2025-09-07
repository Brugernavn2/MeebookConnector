using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MeebookConnector
{
    public class Helpers
    {
        private const string UserAgentString =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
        "AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/139.0.0.0 Safari/537.36";

        private const string AcceptString =
            "text/html,application/xhtml+xml,application/xml;q=0.9," +
            "image/avif,image/webp,image/apng,*/*;q=0.8," +
            "application/signed-exchange;v=b3;q=0.7";

        private const string AcceptLanguageString = "da,en;q=0.9";

        private const string SecChUaString =
            "\"Not;A=Brand\";v=\"99\", " +
            "\"Google Chrome\";v=\"139\", " +
            "\"Chromium\";v=\"139\"";

        // Overload til de fleste brugsscenarier
        public static void AddBrowserHeaders(HttpRequestMessage req) =>
            AddBrowserHeaders(
                req,
                includeOrigin: true,
                originNull: false,
                origin: "https://www.aula.dk/",
                fetchSite: "same-origin"
            );

        // Fleksibel version med alle parametre
        public static void AddBrowserHeaders(
            HttpRequestMessage req,
            bool includeOrigin = false,
            bool originNull = false,
            string? origin = null,
            string? fetchSite = null)
        {
            // Standard browser headers
            req.Headers.UserAgent.ParseAdd(UserAgentString);
            req.Headers.Accept.ParseAdd(AcceptString);
            req.Headers.AcceptLanguage.ParseAdd(AcceptLanguageString);

            // Client hints
            req.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            req.Headers.TryAddWithoutValidation("sec-ch-ua", SecChUaString);
            req.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            req.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");

            // Origin
            if (includeOrigin)
            {
                var originValue = (originNull || origin == null) ? "null" : origin;
                req.Headers.TryAddWithoutValidation("Origin", originValue);
            }

            // Fetch metadata
            if (!string.IsNullOrEmpty(fetchSite))
                req.Headers.TryAddWithoutValidation("Sec-Fetch-Site", fetchSite);

            req.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
            req.Headers.TryAddWithoutValidation("Sec-Fetch-User", "?1");
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
        }

        public static int GetWeekNumber(DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static DateTime FirstDateOfWeek(int year, int weekNumber)
        {
            // Find første dag i året
            DateTime jan1 = new DateTime(year, 1, 1);

            // Find hvilken uge den dag tilhører (ifølge ISO-8601)
            //var cal = CultureInfo.InvariantCulture.Calendar;
            var cal = CultureInfo.GetCultureInfo("da-DK").Calendar; // dansk kultur
            int jan1Week = cal.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            // Hvis jan1 allerede er i uge 1, starter vi der, ellers hopper vi til næste uge
            int weekOffset = (jan1Week == 1 ? weekNumber - 1 : weekNumber);

            // Mandag i den ønskede uge
            DateTime monday = jan1.AddDays(weekOffset * 7);

            // Justér tilbage til mandag
            int diff = (7 + (monday.DayOfWeek - DayOfWeek.Monday)) % 7;
            return monday.AddDays(-diff);
        }

        public static string? ConvertSecureStringToString(SecureString secureString)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static string? CapitalizeFirstLetter(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}
