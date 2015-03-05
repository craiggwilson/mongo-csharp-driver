using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace MongoDB.Bson.Xml
{
    public class XPathBsonDocumentAdapter : IXPathNavigable
    {
        private readonly BsonDocument _document;

        public XPathBsonDocumentAdapter(BsonDocument document)
        {
            _document = document;
        }

        public XPathNavigator CreateNavigator()
        {
            return new BsonDocumentXPathNavigator(_document);
        }
    }
}
