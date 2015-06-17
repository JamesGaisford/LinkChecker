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

namespace LinkChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime logStamp = DateTime.Now;
            var log = CreateLog(logStamp);
            log.WriteLine("Old Url, Target Url, Response Code, Redirect Same as Target");

            var TransformsFromFile = GetUrlTransformsFromFile();
            var results = GetUrlResults(TransformsFromFile, false);

            //var notRedirected = results.Where(x => (int)x.Response.Result.StatusCode != 301);
            foreach (var result in results)
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

            //var onlyRedirects = results.Where(x => (int)x.Response.Result.StatusCode == 301);
            //var redirectResults = GetUrlResults(onlyRedirects.Select(x => new UrlTransform{ OldUrl = x.SourceUrls.OldUrl, NewUrl = x.SourceUrls.NewUrl}).ToList(), true);
            //foreach (var result in redirectResults)
            //{
            //    if (result.Response.IsCanceled || result.Response.IsFaulted)
            //    {
            //        log.WriteLine(String.Format("<<<<<<< ERROR {0} >>>>>>>", result.SourceUrls.OldUrl));
            //    }
            //    else
            //    {
            //        log.WriteLine(String.Format("{0},{1},{2},{3}", result.SourceUrls.OldUrl, result.SourceUrls.NewUrl, (int)result.Response.Result.StatusCode, (result.Response.Result.RequestMessage.RequestUri.LocalPath.ToLower() == result.SourceUrls.NewUrl.ToLower()).ToString()));
            //    }

            //}

            log.WriteLine("Total Redirects = " + results.Count().ToString());
            //log.WriteLine("Successful redirect count = " + onlyRedirects.Count().ToString());
            log.Close();
        }

        private static StreamWriter CreateLog(DateTime logStamp)
        {
            StreamWriter log;
            log = new StreamWriter(String.Format("logfile{0}.txt", logStamp.Ticks));
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
