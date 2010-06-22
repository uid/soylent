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
using Soylent.Model;
using Soylent.Model.Crowdproof;

namespace Soylent.View.Crowdproof

{
    class CrowdproofView : HITView
    {
        new CrowdproofData data;
        static string turkerName = "Turker";
        Button CrowdproofButton;
        Button AcceptRevisions;
        /// <summary>
        /// HITView subclass specific to Shortn tasks.  This adds the Shortn button and additional necessary functionality.
        /// </summary>
        /// <param name="workType"></param>
        /// <param name="data"></param>
        public CrowdproofView(string workType, CrowdproofData data) : base(workType, data)
        {
            //Globals.Soylent.soylent.Controls.Add(new System.Windows.Forms.Button());
            CrowdproofButton = new Button();
            CrowdproofButton.Content = "Show Revisions";
            CrowdproofButton.Name = "Crowdproof";
            CrowdproofButton.Height = 23;
            //CrowdproofButton.Width = 90;
            CrowdproofButton.IsEnabled = false;
            CrowdproofButton.Click += new RoutedEventHandler(Crowdproof_Clicked);

            AcceptRevisions = new Button();
            AcceptRevisions.Content = "Accept Revisions";
            AcceptRevisions.Name = "AcceptRevisions";
            AcceptRevisions.Height = 23;
            //AcceptRevisions.Width = 90;
            AcceptRevisions.IsEnabled = false;
            AcceptRevisions.Click += new RoutedEventHandler(AcceptRevisions_Clicked);

            StackPanel buttons = new StackPanel();
            buttons.Orientation = System.Windows.Controls.Orientation.Horizontal;
            buttons.Children.Add(CrowdproofButton);
            buttons.Children.Add(AcceptRevisions);

            this.data = data;
            data.register(this);
            //ShortnButton.
            //ShortnButton.Click = Shortn_Clicked;
            stages.Children.Add(buttons);
        }
        public void AcceptRevisions_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Globals.Soylent.Application.ActiveDocument.DeleteAllComments();
                Globals.Soylent.Application.ActiveDocument.AcceptAllRevisions();
                Globals.Soylent.Application.ActiveDocument.TrackRevisions = false;
                Globals.Soylent.Application.ActiveDocument.ShowRevisions = false;
            }
            catch
            {

            }
        }
        /// <summary>
        /// CallBack for when the Shortn button is clicked.  Opens the dialog window and changes the color of the status bars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Crowdproof_Clicked(object sender, RoutedEventArgs e)
        {
            //openShortnDialog(data as ShortnData);
            foreach (StageView stageview in stageList.Values)
            {
                stageview.hitProgress.Foreground = Brushes.LightSkyBlue; //Yay light blue
            }
            insertTrackChanges(this.data);
        }

        /// <summary>
        /// Open the Shortn dialog window.  Used internally after the Shortn button is pressed, and by Ribbon for the debugging
        /// </summary>
        /// <param name="data"></param>

        public void insertTrackChanges(CrowdproofData data)
        {
            Globals.Soylent.Application.ActiveDocument.TrackRevisions = true;
            Globals.Soylent.Application.ActiveDocument.ShowRevisions = true;

            Microsoft.Office.Core.DocumentProperties properties;

            properties = (Microsoft.Office.Core.DocumentProperties) Globals.Soylent.Application.ActiveDocument.BuiltInDocumentProperties;
            string defaultAuthor = properties["Author"].Value as string;
            if (defaultAuthor == turkerName)
            {
                turkerName = turkerName + "B";
            }

            foreach (CrowdproofPatch patch in data.patches)
            {
                string comment = "";
                foreach (string reason in patch.reasons)
                {
                    if (patch.reasons.IndexOf(reason) == 0)
                    {
                        comment += reason;
                        
                    }
                    else if (patch.reasons.IndexOf(reason) == 1) 
                    {
                        comment += "\n\nOther Explanations:";
                        comment += "\n - ";
                        comment += reason;
                    }
                    else
                    {
                        comment += "\n - ";
                        comment += reason;
                    }
                }
                foreach (string suggestion in patch.replacements)
                {
                    if (patch.replacements.IndexOf(suggestion) == 0)
                    {
                        if (patch.replacements.Count > 1)
                        {
                            comment += "\n\nOther Suggestions:";
                        }
                    }
                    else
                    {
                        comment += "\n - ";
                        comment += suggestion;
                    }
                }
                object commentText = comment;
                Globals.Soylent.Application.ActiveDocument.Comments.Add(patch.range, commentText);


                Globals.Soylent.Application.UserName = turkerName;
                patch.range.Text = patch.replacements[0];
                Globals.Soylent.Application.UserName = defaultAuthor;
            }

            foreach (Microsoft.Office.Interop.Word.Comment c in Globals.Soylent.Application.ActiveDocument.Comments)
            {
                c.Author = turkerName;
                c.Initial = turkerName;
            }

            this.AcceptRevisions.IsEnabled = true;
        }
        

        /// <summary>
        /// If the ShortenData model has received the final data, it should call this function. Turns progress bars blue and enables the Shortn button.
        /// </summary>
        public void crowdproofDataReceived()
        {
            foreach (StageView stageview in stageList.Values)
            {
                stageview.hitProgress.Foreground = Brushes.Blue; //Turn the bars blue
            }
            this.CrowdproofButton.IsEnabled = true; //Enable the Shortn button
        }
    }
}
