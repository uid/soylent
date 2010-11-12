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
using Soylent.Model;
using System.Diagnostics;

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

            AmazonKeys keys = AmazonKeys.LoadAmazonKeys();
            if (keys != null)
            {
                fillTextField(keys);
            }
        }

        private void fillTextField(AmazonKeys keys)
        {
            accessKey.Text = keys.amazonID;
            secretKey.Text = keys.secretKey;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Model.AmazonKeys.SetAmazonKeys(accessKey.Text, secretKey.Text);
        }

        // Example from http://laurenlavoie.com/avalon/159
        void HandleRequestNavigate(object sender, RoutedEventArgs e)
        {
            string navigateUri = ((Hyperlink) sender).NavigateUri.ToString();
            // if the URI somehow came from an untrusted source, make sure to
            // validate it before calling Process.Start(), e.g. check to see
            // the scheme is HTTP, etc.
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }
    }
}
