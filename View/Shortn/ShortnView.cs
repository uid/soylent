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
using System.Windows.Media.Animation;
using Soylent.Model.Shortn;


namespace Soylent.View.Shortn
{
    public class ShortnView : HITView
    {
        Button ShortnButton;
        /// <summary>
        /// HITView subclass specific to Shortn tasks.  This adds the Shortn button and additional necessary functionality.
        /// </summary>
        /// <param name="workType"></param>
        /// <param name="data"></param>
        public ShortnView(string workType, ShortnData data) : base(workType, data)
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
        /// <summary>
        /// CallBack for when the Shortn button is clicked.  Opens the dialog window and changes the color of the status bars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Shortn_Clicked(object sender, RoutedEventArgs e)
        {
            openShortnDialog(data as ShortnData);
            foreach (StageView stageview in stageList.Values)
            {
                stageview.hitProgress.Foreground = Brushes.LightSkyBlue; //Yay light blue
            }
            stub.hitType.FontWeight = FontWeights.ExtraBold;
        }

        /// <summary>
        /// Open the Shortn dialog window.  Used internally after the Shortn button is pressed, and by Ribbon for the debugging
        /// </summary>
        /// <param name="data"></param>
        public static void openShortnDialog(ShortnData data)
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
            ShortnDialog sd = new ShortnDialog(data);

            // Assign the WPF UserControl to the ElementHost control's
            // Child property.
            host.Child = sd;

            newForm.Visible = false;
            // Add the ElementHost control to the form's
            // collection of child controls.
            newForm.Controls.Add(host);
            newForm.Show();

            // set the form's height based on what the textbox wants to be
            int dialogHeight = (int)sd.grid.DesiredSize.Height;
            newForm.Height = (int)(sd.DesiredSize.Height + newForm.Padding.Vertical + System.Windows.Forms.SystemInformation.CaptionHeight + System.Windows.SystemParameters.ScrollWidth);
            sd.grid.Height = sd.grid.DesiredSize.Height;
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
            stub.ShortnDataReceived();
        }

        public void updateView()
        {
            double find = stageList[Model.HITData.ResultType.Find].percentDone;
            double fix = stageList[Model.HITData.ResultType.Fix].percentDone;
            double verify = stageList[Model.HITData.ResultType.Verify].percentDone;

            double total = (find / 3.0) + (fix / 3.0) + (verify / 3.0);

            HITViewStub.updateViewDelegate del = new HITViewStub.updateViewDelegate(stub.updateView);
            Globals.Soylent.soylent.Invoke(del, total);
        }
    }
}
