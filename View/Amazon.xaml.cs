using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;

namespace Soylent.View
{
    /// <summary>
    /// Interaction logic for Amazon.xaml
    /// </summary>
    public partial class Amazon : UserControl
    {
        public Amazon()
        {
            InitializeComponent();
            Model.AmazonKeys keys = Model.AmazonKeys.GetAmazonKeys(TurKit.getRootDirectory());
            accessKey.Text = keys.amazonID;
            secretKey.Text = keys.secretKey;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            string rootDirectory = TurKit.getRootDirectory();

            StreamReader reader = new StreamReader(rootDirectory + "amazon.template.xml");
            string content = reader.ReadToEnd();
            reader.Close();

            content = Regex.Replace(content, "AmazonKeyHere", accessKey.Text);
            content = Regex.Replace(content, "AmazonSecretHere", secretKey.Text);

            StreamWriter writer = new StreamWriter(rootDirectory + "amazon.xml", false);
            writer.Write(content);
            writer.Close();
        }
    }
}
