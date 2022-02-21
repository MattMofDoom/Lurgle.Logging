using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Lurgle.Logging.Classes
{
    /// <summary>
    ///     HTTP Client handler for Seq or Splunk
    /// </summary>
    public class SeqClient : HttpClientHandler
    {
        /// <summary>
        ///     Return a HTTP Client Handler for Seq
        /// </summary>
        /// <returns></returns>
        public SeqClient()
        {
            if (Logging.Config.LogSeqUseProxy)
            {
                var proxy = new WebProxy
                {
                    Address = new Uri(Logging.Config.LogSeqProxyServer),
                    BypassProxyOnLocal = Logging.Config.LogSeqBypassProxyOnLocal,
                    BypassList = Logging.Config.LogSeqProxyBypass
                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim()).ToArray(),
                    UseDefaultCredentials = false
                };

                if (!string.IsNullOrEmpty(Logging.Config.LogSeqProxyUser) &&
                    !string.IsNullOrEmpty(Logging.Config.LogSeqProxyPassword))
                    proxy.Credentials = new NetworkCredential(Logging.Config.LogSeqProxyUser,
                        Logging.Config.LogSeqProxyPassword);
                else
                    proxy.UseDefaultCredentials = true;

                UseProxy = true;
                Proxy = proxy;
                UseDefaultCredentials = false;
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            else
            {
                UseProxy = false;
                UseDefaultCredentials = true;
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
        }
    }
}