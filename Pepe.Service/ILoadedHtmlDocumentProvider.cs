using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pepe.Service {
    public interface ILoadedHtmlDocumentProvider {
        HtmlDocument GetLoadedHtmlDocument();
    }
}
