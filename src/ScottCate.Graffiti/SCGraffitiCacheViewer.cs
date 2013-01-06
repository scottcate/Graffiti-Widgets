using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Caching;
using Graffiti.Core;

namespace ScottCate.Graffiti
{
    [Serializable]
    [WidgetInfo("{774DD468-BE0F-4577-88CF-8B87EF1AB6AB}", "ScottCate Graffiti Cache Viewer", "Display the Results from the Graffiti Cache")]
    public class SCGraffitiCacheViewer : Widget
    {
        public override string Name
        {
            get { return "ScottCate Graffiti Cache Viewer"; }
        }

        public override bool IsUserValid()
        {
            return GraffitiUsers.IsAdmin(GraffitiUsers.Current);
        }

        public override string RenderData()
        {
            HttpContext current = HttpContext.Current;
            Cache cache = current != null ? current.Cache : HttpRuntime.Cache;

            IDictionaryEnumerator enumerator = cache.GetEnumerator();
            var list = new List<string>();

            var sb = new StringBuilder("<h3>super simple cache viewer / by key");
            sb.Append("<ul>");
            while (enumerator.MoveNext())
            {
                sb.AppendFormat("<li>{0}</li>", enumerator.Key);
            }
            return sb.Append("</ul>").ToString();            
        }
    }
}