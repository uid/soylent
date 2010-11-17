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

namespace Soylent.View
{
    /// <summary>
    /// Interaction logic for Error.xaml
    /// </summary>
    public partial class HITError : UserControl
    {
        public HITError(string description, string resolutionLinkText = "", string resolutionUrl = "")
        {
            InitializeComponent();

            errorText.Text = description;
            if (resolutionLinkText != null && resolutionLinkText != "" && resolutionUrl != null && resolutionUrl != "")
            {
                Run run = new Run(resolutionLinkText);
                hyperlink.Inlines.Clear();
                hyperlink.Inlines.Add(run);
                hyperlink.NavigateUri = new Uri(resolutionUrl);
            }
            else
            {
                hyperlinkBox.Visibility = Visibility.Collapsed;
            }
        }

        private void hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Amazon.NavigateToUrl(((Hyperlink)sender).NavigateUri.ToString());
            e.Handled = true;
        }
    }
}
