using System;
using System.Collections.Generic;
using System.Web;
using Graffiti.Core;

namespace ScottCate.Graffiti
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Original code found on http://support.graffiticms.com/p/885/4663.aspx#4663
    /// </remarks>
    [Chalk("SCProps")]
    public class SCProps
    {
        public void SetResponseStatusCode(int status)
        {
            HttpContext ctx = HttpContext.Current;
            ctx.Response.StatusCode = status;
        }

        public string Concat(string one, string two)
        {
            return string.Concat(one, two);
        }
    }

    [Chalk("SC404")]
    public class SC404
    {
        public void Register404()
        {
            var ctx = HttpContext.Current;
            ctx.Response.StatusCode = 404;
            var requestedURL = ctx.Request.ServerVariables["QUERY_STRING"];
            requestedURL.Replace("404;", string.Empty);
        }

        public string GetHTTP_REFERER()
        {
            var referer = HttpContext.Current.Request["HTTP_REFERER"];
            if (string.IsNullOrEmpty(referer))
                referer = "None";
            return referer.ToLower();
        }

        public string GetQUERY_STRING()
        {
            return HttpContext.Current.Request.ServerVariables["QUERY_STRING"];
        }

        public string GetRequested404URL()
        {
            var requestedURL = HttpContext.Current.Request.ServerVariables["QUERY_STRING"];
            if(requestedURL.StartsWith("404;"))
                requestedURL = requestedURL.Replace("404;", string.Empty);
            else if(requestedURL.StartsWith("aspxerrorpath="))
                requestedURL = requestedURL.Replace("aspxerrorpath=", string.Empty);
            else requestedURL = "x404";
            
            return requestedURL;
        }

        public void Log404URL()
        {
            
            string url404 = GetRequested404URL();
            if(url404 != "x404")
            {
                var storage = ObjectManager.Get<SC404Storage>(url404);
                HttpContext current = HttpContext.Current;

                if (storage == null)
                    storage = new SC404Storage();

                if(!string.IsNullOrEmpty(storage.redirectTo))
                {
                    var httpResponse = current.Response;
                    httpResponse.StatusCode = 301;
                    httpResponse.Redirect(storage.redirectTo);
                }
                else
                {
                    //Log and Save the 404
                    storage.hitCount++;
                    storage.url = url404;

                    string referer = GetHTTP_REFERER();
                    if(referer != "none" && !storage.referals.Contains(referer))
                    {
                        storage.referals.Add(referer);
                    }
                    ObjectManager.Save(storage, url404);
                }
            }
        }

    }

    [Serializable]
    public class SC404Storage
    {
        public SC404Storage(){}

        public string url { get; set; }
        public int hitCount { get; set; }
        public List<string> referals { get; set; }
        public string redirectTo { get; set; }
    }
}