using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace LinkChecker
{
    class Program
    {
        static List<string> LogFilePaths = null;

        static void Main(string[] args)
        {
            DateTime logStamp = DateTime.Now;
            LogFilePaths = new List<string>();

            var TransformsFromFile = GetUrlTransformsFromFile();
            var results = GetUrlResults(TransformsFromFile, false);

            var groupedResults = results.GroupBy(x => (int)x.Response.Result.StatusCode).ToList();
            var notRedirected = groupedResults.Where(x => x.Key != 301);
            var onlyRedirects = groupedResults.Where(x => x.Key == 301).FirstOrDefault();
            LogErrorUrls(logStamp, notRedirected, results.Count);
            LogRedirectedUrls(logStamp, onlyRedirects, results.Count);
            Console.WriteLine("Opening log files......");
            foreach (string LogFilePath in LogFilePaths)
            {
                Process.Start(LogFilePath);
            }
            Console.Read();
           
        }

        private static void LogErrorUrls(DateTime logStamp, IEnumerable<System.Linq.IGrouping<int, UrlTransformResult>> notRedirected, int TotalRequests)
        {
            var log = CreateLog(logStamp, "Errors");
            log.WriteLine("Old Url, Target Url, Response Code");
            foreach (var group in notRedirected)
            {
                foreach (var result in group)
                {
                    if (result.Response.IsCanceled || result.Response.IsFaulted)
                    {
                        log.WriteLine(String.Format("<<<<<<< ERROR {0} >>>>>>>", result.SourceUrls.OldUrl));
                    }
                    else
                    {
                        log.WriteLine(String.Format("{0},{1},{2}", result.SourceUrls.OldUrl, result.SourceUrls.NewUrl, (int)result.Response.Result.StatusCode));
                    }

                }
                
                log.WriteLine("Total results with response code " + group.Key + " = " + group.Count().ToString());
            }
            if (notRedirected.Any())
            {
                Console.WriteLine("Total Requests = " + TotalRequests.ToString());
            }
            else
            {
                Console.WriteLine("No requests that were not redirected!");
            }
            LogFilePaths.Add(((FileStream)(log.BaseStream)).Name);
            log.Close();
        }

        private static void LogRedirectedUrls(DateTime logStamp, System.Linq.IGrouping<int, UrlTransformResult> onlyRedirects, int TotalRequests)
        {
            var log = CreateLog(logStamp, "Redirects");
            log.WriteLine("Old Url, Target Url, Response Code, Redirect Same as Target");
            int SuccessCounter = 0;
            List<UrlTransform> ThreeOOnes = onlyRedirects.Select(x => new UrlTransform { OldUrl = x.SourceUrls.OldUrl, NewUrl = x.SourceUrls.NewUrl }).ToList();
            var redirectResults = GetUrlResults(ThreeOOnes, true);
            foreach (var result in redirectResults)
            {
                if (result.Response.IsCanceled || result.Response.IsFaulted)
                {
                    log.WriteLine(String.Format("<<<<<<< ERROR {0} >>>>>>>", result.SourceUrls.OldUrl));
                }
                else
                {
                    bool NewUrlAndActualRedirectUrlMatch = (result.Response.Result.RequestMessage.RequestUri.LocalPath.ToLower() == result.SourceUrls.NewUrl.ToLower());
                    if (NewUrlAndActualRedirectUrlMatch)
                    {
                        SuccessCounter++;
                    }
                    log.WriteLine(String.Format("{0},{1},{2},{3}", result.SourceUrls.OldUrl, result.SourceUrls.NewUrl, (int)result.Response.Result.StatusCode, NewUrlAndActualRedirectUrlMatch.ToString()));
                }

            }
            Console.WriteLine("Total Requests = " + TotalRequests.ToString());
            Console.WriteLine("Successful redirect count = " + SuccessCounter.ToString());
            LogFilePaths.Add(((FileStream)(log.BaseStream)).Name);
            log.Close();
        }

        private static StreamWriter CreateLog(DateTime logStamp, string LogFileSuffix)
        {
            StreamWriter log;
            log = new StreamWriter(String.Format("{0}_{1}.csv", logStamp.Ticks, LogFileSuffix));
            return (log);
        }

        private static List<UrlTransformResult> GetUrlResults(List<UrlTransform> UrlTransforms, bool AllowAutoRedirect)
        {
            var allResponses = new List<UrlTransformResult>();
            var allTasks = UrlTransforms.Select(async item =>
            {
                var response = MakeSiteRequest(item, AllowAutoRedirect);
                allResponses.Add(new UrlTransformResult { SourceUrls = item, Response = response});
            });
            Task.WhenAll(allTasks);
            return (allResponses);
        }


        private static async Task<HttpResponseMessage> MakeSiteRequest(UrlTransform t, bool AllowAutoRedirect)
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = AllowAutoRedirect
            };
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("http://mubaloo.com/");
                //client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                return (await client.GetAsync(t.OldUrl));
                
            }
            
        }

        private static List<UrlTransform> GetUrlTransformsFromFile()
        {
            FileHelperEngine engine = new FileHelperEngine(typeof(UrlTransform)); 
            var assemblylocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

            var dir = System.IO.Path.GetDirectoryName(assemblylocation);

	        // To Read Use: 
            UrlTransform[] res = engine.ReadFile("data/three-o-one.csv") as UrlTransform[];
            return res.ToList();
        }

        
    }

}
