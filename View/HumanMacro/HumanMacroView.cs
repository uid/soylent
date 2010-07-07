﻿using System;
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
using Soylent.Model.HumanMacro;

namespace Soylent.View.HumanMacro

{
    class HumanMacroView : HITView
    {
        new HumanMacroResult data;
        static string turkerName = "Turker";
        Button HumanMacroButton;
        Button AcceptRevisions;
        Button RejectRevisions;
        StackPanel buttons;
        /// <summary>
        /// HITView subclass specific to Shortn tasks.  This adds the Shortn button and additional necessary functionality.
        /// </summary>
        /// <param name="workType"></param>
        /// <param name="data"></param>
        public HumanMacroView(string workType, HumanMacroResult data) : base(workType, data)
        {
            //Globals.Soylent.soylent.Controls.Add(new System.Windows.Forms.Button());
            HumanMacroButton = new Button();
            HumanMacroButton.Content = "View Revisions";
            HumanMacroButton.Name = "HumanMacro";
            HumanMacroButton.Height = 23;
            //CrowdproofButton.Width = 90;
            HumanMacroButton.IsEnabled = false;
            HumanMacroButton.Click += new RoutedEventHandler(HumanMacro_Clicked);
 
            AcceptRevisions = new Button();
            AcceptRevisions.Content = "Accept All";
            AcceptRevisions.Name = "AcceptRevisions";
            AcceptRevisions.Height = 23;
            AcceptRevisions.Width = 100;
            AcceptRevisions.IsEnabled = false;
            AcceptRevisions.Click += new RoutedEventHandler(AcceptRevisions_Clicked);

            RejectRevisions = new Button();
            RejectRevisions.Content = "Reject All";
            RejectRevisions.Name = "RejectRevisions";
            RejectRevisions.Height = 23;
            RejectRevisions.Width = 100;
            RejectRevisions.IsEnabled = false;
            RejectRevisions.Click += new RoutedEventHandler(RejectRevisions_Clicked);

            buttons = new StackPanel();
            buttons.Orientation = System.Windows.Controls.Orientation.Horizontal;
            buttons.Children.Add(AcceptRevisions);
            buttons.Children.Add(RejectRevisions);
                          
            stages.Children.Add(HumanMacroButton);
            stages.Children.Add(buttons);

            this.data = data;
            data.register(this);

            
        }
        public void AcceptRevisions_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Globals.Soylent.Application.ActiveDocument.DeleteAllComments();
            }catch
            {

            }
            try
            {
                Globals.Soylent.Application.ActiveDocument.AcceptAllRevisions();
            }
            catch
            {

            }
            Globals.Soylent.Application.ActiveDocument.TrackRevisions = false;
            Globals.Soylent.Application.ActiveDocument.ShowRevisions = false;
            
            
            AcceptRevisions.IsEnabled = false;
            RejectRevisions.IsEnabled = false;
            HumanMacroButton.IsEnabled = true;
            //this.stages.Children.Remove(buttons);
            //this.stages.Children.Add(CrowdproofButton);
        }

        public void RejectRevisions_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Globals.Soylent.Application.ActiveDocument.DeleteAllComments();
            }
            catch
            {

            }
            try{
                Globals.Soylent.Application.ActiveDocument.RejectAllRevisions();
            }
            catch
            {

            }
            Globals.Soylent.Application.ActiveDocument.TrackRevisions = false;
            Globals.Soylent.Application.ActiveDocument.ShowRevisions = false;
            
            
            AcceptRevisions.IsEnabled = false;
            RejectRevisions.IsEnabled = false;
            HumanMacroButton.IsEnabled = true;
            //this.stages.Children.Remove(buttons);
            //this.stages.Children.Add(CrowdproofButton);
        }
        /// <summary>
        /// CallBack for when the Shortn button is clicked.  Opens the dialog window and changes the color of the status bars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HumanMacro_Clicked(object sender, RoutedEventArgs e)
        {
            //openShortnDialog(data as ShortnData);
            foreach (StageView stageview in stageList.Values)
            {
                stageview.hitProgress.Foreground = Brushes.LightSkyBlue; //Yay light blue
            }
            insertTrackChanges();

        }

        /// <summary>
        /// Open the Shortn dialog window.  Used internally after the Shortn button is pressed, and by Ribbon for the debugging
        /// </summary>
        /// <param name="data"></param>

        public void insertTrackChanges()
        {
            data.prepareRanges();
            HumanMacroResult.ReturnType type = data.type;
            Globals.Soylent.Application.ActiveDocument.TrackRevisions = true;
            Globals.Soylent.Application.ActiveDocument.ShowRevisions = true;

            Microsoft.Office.Core.DocumentProperties properties;

            properties = (Microsoft.Office.Core.DocumentProperties) Globals.Soylent.Application.ActiveDocument.BuiltInDocumentProperties;
            string defaultAuthor = properties["Author"].Value as string;
            if (defaultAuthor == turkerName)
            {
                turkerName = turkerName + "B";
            }

            foreach (Patch patch in data.patches)
            {
                string comment = "";
                foreach (string suggestion in patch.replacements)
                {
                    if (patch.replacements.IndexOf(suggestion) == 0)
                    {
                        if (type == HumanMacroResult.ReturnType.Comment)
                        {
                            comment += suggestion;
                            if (patch.replacements.Count > 1)
                            {
                                comment += "\n\nOther Suggestions:";
                            }
                        }
                        else
                        {
                            if (patch.replacements.Count > 1)
                            {
                                comment += "Other Suggestions:";
                            }
                        }
                    }
                    else
                    {
                        comment += "\n - ";
                        comment += suggestion;
                    }
                }
                object commentText = comment;
                if (comment != "")
                {
                    Globals.Soylent.Application.ActiveDocument.Comments.Add(patch.range, commentText);
                }

                if (type == HumanMacroResult.ReturnType.SmartTag)
                {
                    Globals.Soylent.Application.UserName = turkerName;
                    patch.range.Text = patch.replacements[0];
                    Globals.Soylent.Application.UserName = defaultAuthor;
                }
            }

            foreach (Microsoft.Office.Interop.Word.Comment c in Globals.Soylent.Application.ActiveDocument.Comments)
            {
                c.Author = turkerName;
                c.Initial = turkerName;
            }

            //this.AcceptRevisions.IsEnabled = true;
            //this.stages.Children.Remove(CrowdproofButton);
            //this.stages.Children.Add(buttons);
            AcceptRevisions.IsEnabled = true;
            RejectRevisions.IsEnabled = true;
            HumanMacroButton.IsEnabled = false;
        }
        

        /// <summary>
        /// If the ShortenData model has received the final data, it should call this function. Turns progress bars blue and enables the Shortn button.
        /// </summary>
        public void humanMacroDataReceived()
        {
            foreach (StageView stageview in stageList.Values)
            {
                stageview.hitProgress.Foreground = Brushes.Blue; //Turn the bars blue
            }
            this.HumanMacroButton.IsEnabled = true; //Enable the Shortn button
        }
    }
}
