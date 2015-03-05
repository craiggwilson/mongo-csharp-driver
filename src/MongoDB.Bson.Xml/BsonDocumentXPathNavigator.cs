using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace MongoDB.Bson.Xml
{
    public class BsonDocumentXPathNavigator : XPathNavigator
    {
        private readonly BsonDocument _document;

        public BsonDocumentXPathNavigator(BsonDocument document)
        {
            _document = document;
        }

        public override string BaseURI
        {
            get { return "MongoDB"; }
        }

        public override XPathNavigator Clone()
        {
            throw new NotImplementedException();
        }

        public override bool IsEmptyElement
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            throw new NotImplementedException();
        }

        public override string LocalName
        {
            get { throw new NotImplementedException(); }
        }

        public override bool MoveTo(XPathNavigator other)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToFirstAttribute()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToFirstChild()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToId(string id)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToNext()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToNextAttribute()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToParent()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToPrevious()
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Xml.XmlNameTable NameTable
        {
            get { throw new NotImplementedException(); }
        }

        public override string NamespaceURI
        {
            get { throw new NotImplementedException(); }
        }

        public override XPathNodeType NodeType
        {
            get { throw new NotImplementedException(); }
        }

        public override string Prefix
        {
            get { throw new NotImplementedException(); }
        }

        public override string Value
        {
            get { throw new NotImplementedException(); }
        }
    }
}
