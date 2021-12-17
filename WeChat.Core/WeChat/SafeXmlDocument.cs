using System;
using System.Xml;
namespace WeChat.Core.WeChat
{
    public class SafeXmlDocument:XmlDocument
    {
        public SafeXmlDocument()
        {
            this.XmlResolver = null;
        }
    }
}
