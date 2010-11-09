using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Soylent.Model
{
    public class AmazonKeys
    {
        public string amazonID;
        public string secretKey;

        /// <summary>
        /// Reads in the AMT secret and key from the amazon.xml file so that HITs can be submitted.
        /// </summary>
        public static AmazonKeys GetAmazonKeys(string rootDirectory)
        {
            //System.Xml.XmlTextReader amazonReader = new System.Xml.XmlTextReader("./amazon.template.xml");
            XDocument doc = XDocument.Load(rootDirectory + @"\amazon.xml");
            XElement secret = doc.Root.Element("amazonSECRET");
            XElement key = doc.Root.Element("amazonKEY");

            return new AmazonKeys
            {
                amazonID = key.Value,
                secretKey = secret.Value
            };
        }
    }
}
