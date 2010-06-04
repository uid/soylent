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
using System.Windows.Forms.Integration;

namespace Soylent
{
    public class ShortnView : HITView
    {
        Button ShortnButton;
        public ShortnView(string workType, ShortenData data) : base(workType, data)
        {
            //Globals.Soylent.soylent.Controls.Add(new System.Windows.Forms.Button());
            ShortnButton = new Button();
            ShortnButton.Content = "Shortn";
            ShortnButton.Name = "Shortn";
            ShortnButton.Height = 23;
            ShortnButton.Width = 80;
            ShortnButton.IsEnabled = false;
            ShortnButton.Click += new RoutedEventHandler(Shortn_Clicked);

            data.register(this);
            //ShortnButton.
            //ShortnButton.Click = Shortn_Clicked;
            stages.Children.Add(ShortnButton);
        }
        public void Shortn_Clicked(object sender, RoutedEventArgs e)
        {
            openShortnDialog(data as ShortenData);
            foreach (StageView stageview in stageList.Values)
            {
                stageview.hitProgress.Foreground = Brushes.LightSkyBlue; //Yay light blue
            }
        }

        /// <summary>
        /// Open the Shortn dialog window.  Used internally after the Shortn button is pressed, and by Ribbon for the debugging
        /// </summary>
        /// <param name="data"></param>
        public static void openShortnDialog(ShortenData data)
        {
            System.Windows.Forms.Form newForm = new System.Windows.Forms.Form();
            newForm.Width = 1200;
            newForm.Height = int.MaxValue;
            newForm.BackColor = System.Drawing.Color.White;

            // Create the ElementHost control for hosting the
            // WPF UserControl.
            ElementHost host = new ElementHost();
            host.Dock = System.Windows.Forms.DockStyle.Fill;

            // Create the WPF UserControl.
            ShortenDialog sd = new ShortenDialog(data);

            // Assign the WPF UserControl to the ElementHost control's
            // Child property.
            host.Child = sd;

            newForm.Visible = false;
            // Add the ElementHost control to the form's
            // collection of child controls.
            newForm.Controls.Add(host);
            newForm.Show();

            // set the form's height based on what the textbox wants to be
            newForm.Height = (int)(sd.DesiredSize.Height + newForm.Padding.Vertical + System.Windows.Forms.SystemInformation.CaptionHeight);
            sd.grid.Height = sd.grid.DesiredSize.Height - System.Windows.SystemParameters.ScrollWidth;
            newForm.Width = 1200;
            host.MaximumSize = new System.Drawing.Size(1200, System.Windows.Forms.SystemInformation.VirtualScreen.Height);
            newForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;

            newForm.Visible = true;
        }

        /// <summary>
        /// If the ShortenData model has received the final data, it should call this function. Turns progress bars blue and enables the Shortn button.
        /// </summary>
        public void shortenDataReceived()
        {
            foreach (StageView stageview in stageList.Values)
            {
                stageview.hitProgress.Foreground = Brushes.Blue; //Turn the bars blue
            }
            this.ShortnButton.IsEnabled = true; //Enable the Shortn button
        }
    }
}
