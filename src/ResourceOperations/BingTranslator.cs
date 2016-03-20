using System.Collections.Generic;
using System.Data;
using System.Linq;
using ResxTranslator.Properties;
using ResxTranslator.Service_References.TranslatorSvc;

namespace ResxTranslator.ResourceOperations
{
    public class BingTranslator
    {
        public static void AutoTranslate(ResourceHolder resourceHolder, string languageCode)
        {
            var appId = Settings.Default.BingAppId;

            var toTranslate = new List<string>();

            foreach (DataRow row in resourceHolder.StringsTable.Rows)
            {
                if (!string.IsNullOrEmpty(row["NoLanguageValue"].ToString())
                    && (row[languageCode.ToLower()].ToString() == row["NoLanguageValue"].ToString()
                        || string.IsNullOrEmpty(row[languageCode.ToLower()].ToString())))
                {
                    toTranslate.Add(row["NoLanguageValue"].ToString());
                }
            }


            if (string.IsNullOrEmpty(appId))
            {
                return;
            }

            var svc = new LanguageServiceClient();

            var translatedTexts
                = svc.TranslateArray(appId
                    , toTranslate.ToArray()
                    , Settings.Default.NeutralLanguageCode
                    , languageCode.ToLower().Substring(0, 2)
                    , new TranslateOptions());

            var i = 0;
            foreach (DataRow row in resourceHolder.StringsTable.Rows)
            {
                if (!string.IsNullOrEmpty(row["NoLanguageValue"].ToString())
                    && (row[languageCode.ToLower()].ToString() == row["NoLanguageValue"].ToString()
                        || string.IsNullOrEmpty(row[languageCode.ToLower()].ToString())))
                {
                    if (string.IsNullOrEmpty(translatedTexts[i].Error))
                    {
                        row[languageCode.ToLower()] = translatedTexts[i].TranslatedText;
                    }
                    ++i;
                }
            }
        }

        public static string GetDefaultLanguage(ResourceHolder resourceHolder)
        {
            var appId = Settings.Default.BingAppId;

            if (string.IsNullOrEmpty(appId))
            {
                return "";
            }
            var toTranslate = new List<string>();
            var cnt = 0;
            foreach (DataRow row in resourceHolder.StringsTable.Rows)
            {
                if (!string.IsNullOrEmpty(row["NoLanguageValue"].ToString())
                    && (row["NoLanguageValue"].ToString().Length > 10 || resourceHolder.StringsTable.Rows.Count < 10))
                {
                    toTranslate.Add(row["NoLanguageValue"].ToString());
                    cnt++;
                }
                if (cnt > 10)
                {
                    break;
                }
            }

            if (cnt == 0)
            {
                return "";
            }

            var svc = new LanguageServiceClient();
            var translatedTexts = svc.TranslateArray(appId, toTranslate.ToArray(), Settings.Default.NeutralLanguageCode,
                "en", new TranslateOptions());

            // find most frequent language
            var maxArr = translatedTexts
                .GroupBy(t => t.From)
                .Select(grp => new {Language = grp.Key, Count = grp.Count()})
                .OrderByDescending(y => y.Count);

            return maxArr.First().Language;
        }

        public static string TranslateString(string src, string to)
        {
            var appId = Settings.Default.BingAppId;
            var svc = new LanguageServiceClient();
            var tolanguage = string.IsNullOrEmpty(to.Trim()) ? "" : (to.Trim() + "  ").Substring(0, 2);
            var translateOptions = new TranslateOptions();
            translateOptions.ContentType = "text/html";
            translateOptions.Category = "general";

            var translatedTexts = svc.TranslateArray(appId, new[] {src}, Settings.Default.NeutralLanguageCode,
                tolanguage, translateOptions);

            return translatedTexts[0].TranslatedText;
        }
    }
}