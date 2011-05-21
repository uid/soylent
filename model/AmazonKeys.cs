using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace Soylent.Model
{
    public class AmazonKeys
    {
        public string amazonID;
        public string secretKey;

        public static AmazonKeys LoadAmazonKeys()
        {
            string rootDirectory = Soylent.GetDataDirectory();
            string keyFile = rootDirectory + @"\amazon.xml";


            if (!File.Exists(keyFile))
            {
                return null;
            }
            else
            {
                AmazonKeys keys = getKeysFromFile(keyFile);
                return keys;
            }
        }

        /// <summary>
        /// Reads in the AMT secret and key from the amazon.xml file so that HITs can be submitted.
        /// </summary>
        public static void AskForAmazonKeys(TurKit.startTaskDelegate success, TurKit.noKeysDelegate cancel)
        {
            AmazonKeys keys = LoadAmazonKeys();
            if (keys == null)
            {
                Globals.Ribbons.Ribbon.AskForKeys(success, cancel);
            }
            else
            {
                success(keys);
            }
        }

        public static AmazonKeys getKeysFromFile(string keyFile)
        {
            XDocument doc = XDocument.Load(keyFile);
            XElement secret = doc.Root.Element("amazonSECRET");
            XElement key = doc.Root.Element("amazonKEY");

            return new AmazonKeys
            {
                amazonID = key.Value,
                secretKey = secret.Value
            };
        }

        public static void SetAmazonKeys(string key, string secret)
        {
            string rootDirectory = Soylent.GetDataDirectory();

            StreamReader reader = new StreamReader(rootDirectory + "amazon.template.xml");
            string content = reader.ReadToEnd();
            reader.Close();

            content = Regex.Replace(content, "AmazonKeyHere", key);
            content = Regex.Replace(content, "AmazonSecretHere", secret);

            StreamWriter writer = new StreamWriter(rootDirectory + "amazon.xml", false);
            writer.Write(content);
            writer.Close();
        }
    }
}
