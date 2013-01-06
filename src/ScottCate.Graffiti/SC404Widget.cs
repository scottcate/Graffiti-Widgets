using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DataBuddy;
using Graffiti.Core;

namespace ScottCate.Graffiti
{
    [Serializable]
    [WidgetInfo("{774DD468-BE0F-4577-88CF-8B87EF1AB6AC}", "ScottCate 404Widget", "Display the Results from Collected 404's")]
    public class SC404Widget : Widget
    {
        const string SC404StorageFullName = "ScottCate.Graffiti.SC404Storage";

        public override string Name
        {
            get { return "ScottCate 404Widget"; }
        }

        #region fields

        public string Post404ID;
        private int post404Id
        {
            get
            {
                int postid;
                int.TryParse(Post404ID, out postid);
                return postid;
            }
        }

        public string MinHitCount;
        private int minHitCount
        {
            get
            {
                int minCount;
                int.TryParse(MinHitCount, out minCount);
                return minCount;
            }
        }

        public string ClearStatistics;
        private bool clearStatistics
        {
            get
            {
                bool clear = false;
                if(!string.IsNullOrEmpty(ClearStatistics))
                    clear = ClearStatistics.ToLower() == "on";

                return clear;
            }
        }

        #endregion

        #region form data

        protected override NameValueCollection DataAsNameValueCollection()
        {
            var nvc = base.DataAsNameValueCollection();
            nvc["Post404Id"] = Post404ID;
            nvc["MinHitCount"] = MinHitCount;

            return nvc;
        }

        public override StatusType SetValues(HttpContext context, NameValueCollection nvc)
        {
            base.SetValues(context, nvc);
            Post404ID = nvc["Post404Id"];
            MinHitCount = nvc["MinHitCount"];
            ClearStatistics = nvc["ClearStatistics"];

            //clear 404 Stats table.
            if (clearStatistics)
            {
                //this didn't works...
                ZCache.RemoveCache("object-" + SC404StorageFullName);

                ObjectStore.Destroy(ObjectStore.Columns.Type, SC404StorageFullName);
                ZCache.Clear();
            }

            return StatusType.Success;
        }

        protected override FormElementCollection AddFormElements()
        {
            var fe = new FormElementCollection
                         {
                             new TextFormElement("Title", "Widget Title", string.Empty),
                             new TextFormElement("MinHitCount", "Minimum number of hits before display",
                                                 "(Suggest: 30; Defaults to 30 if left blank; Use 0 for all links)"),
                             new TextFormElement("Post404Id", "Post ID of the 404 Page to Display",
                                                 "Edit your 404, and check the end of the URL for <b>posts/write/?id=100</b> (100 is the post id)"),
                             new CheckFormElement("ClearStatistics",
                                                  "Clear 404 Statistics on Widget Update",
                                                  "Checked: Clear all 404Stats on Widget Update<br /> Unchecked: Keep Current Stats",false)
                         };
            return fe;
        }

        #endregion

        public override bool IsUserValid()
        {
            return GraffitiUsers.IsAdmin(GraffitiUsers.Current);
        }

        public override string RenderData()
        {
            Query q = ObjectStore.CreateQuery();
            q.AndWhere(ObjectStore.Columns.Type, SC404StorageFullName);
            var osc = ObjectStoreCollection.FetchByQuery(q);

            var sc404S = new List<SC404Storage>(osc.Count);
            foreach (var store in osc)
            {
                var item = ObjectManager.ConvertToObject<SC404Storage>(store.Data);
                if (item != null && item.hitCount > minHitCount) sc404S.Add(item);
            }

            //sort by hitCount, Largest First
            sc404S.Sort((s1, s2) => s2.hitCount.CompareTo(s1.hitCount));

            var output = new StringBuilder();
            output.AppendFormat("<h3><a href=\"http://scottcate.com/tags/404Manager/Default.aspx\">Scott Cate 404 Manager: Beta</a></h3>");
            output.AppendFormat("<h3>Showing [{0}] of [{1}] (&gt;{2} Hits)</h3>", sc404S.Count, osc.Count, minHitCount);
            output.Append("<ul>");
            output.Append(BuildItems(sc404S));
            output.Append("</ul>");
            return output.ToString();
        }

        private static string BuildItems(List<SC404Storage> sc404s)
        {

            var output = new StringBuilder();
            sc404s.ForEach(s =>
                               {
                                   string show = s.url;
                                   string details = s.url;
                                   if (s.referals.Count > 0)
                                   {
                                       details += "\\n\\nReferrals\\n\\n";
                                       s.referals.ForEach(r => details += string.Format("\\n{0}", r));
                                   }
                                   if (show.Length > 35)
                                       show = show.Substring(show.Length - 35);
                                   output.AppendFormat("<li><a onclick=javascript:alert('{0}');> [{1},{3}] {2}</a></li>", details.Replace("'", "''"), s.hitCount, show, s.referals.Count);
                               });

            return output.ToString();
        }
    }
}