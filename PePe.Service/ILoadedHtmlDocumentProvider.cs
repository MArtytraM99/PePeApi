using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Service {
    public interface ILoadedHtmlDocumentProvider {
        HtmlDocument GetLoadedHtmlDocument();
    }
}
