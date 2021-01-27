using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pepe.Service {
    public class WebLoadedHtmlDocumentProvider : ILoadedHtmlDocumentProvider {
        const string url = "https://www.jidelnapepe.cz/";
        public HtmlDocument GetLoadedHtmlDocument() {
            var web = new HtmlAgilityPack.HtmlWeb();
            return web.Load(url);
        }
    }
}
