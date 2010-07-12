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
using Soylent.View.Crowdproof;

namespace Soylent.View
{
    /// <summary>
    /// Interaction logic for HITView.xaml
    /// </summary>
    public partial class HITViewStub : UserControl
    {
        public Dictionary<HITData.ResultType, StageView> stageList;
        public HITData data;
        public HITView view;
        public Sidebar sidebar;

        /// <summary>
        /// The view for a task in the sidebar
        /// </summary>
        /// <param name="workType">Job type</param>
        /// <param name="data">Data Model for this View</param>
        public HITViewStub(string workType, HITData data, HITView view)
        {
            InitializeComponent();

            hitType.Content = workType;
            this.view = view;
            this.data = data;
            //data.register(this);
            previewText.Text = data.originalText;
            stageList = new Dictionary<HITData.ResultType,StageView>();

            hitProgress.MouseUp += dataReceived;
        }
        
        public void dataReceived(){
            grid.Children.Remove(hitProgress);
            this.hitType.FontWeight = FontWeights.ExtraBold;

            if (((string) hitType.Content) == "Crowdproof")
            {
                CrowdproofView cpv = view as CrowdproofView;

                Button CrowdproofButton = new Button();
                CrowdproofButton.Content = "View Revisions";
                CrowdproofButton.Name = "Crowdproof";
                CrowdproofButton.Height = 23;
                //CrowdproofButton.Width = 90;
                //CrowdproofButton.IsEnabled = false;
                CrowdproofButton.Click += new RoutedEventHandler(cpv.Crowdproof_Clicked);

                Button AcceptRevisions = new Button();
                AcceptRevisions.Content = "Accept All";
                AcceptRevisions.Name = "AcceptRevisions";
                AcceptRevisions.Height = 23;
                AcceptRevisions.Width = 95;
                //AcceptRevisions.IsEnabled = false;
                AcceptRevisions.Click += new RoutedEventHandler(cpv.AcceptRevisions_Clicked);

                Button RejectRevisions = new Button();
                RejectRevisions.Content = "Reject All";
                RejectRevisions.Name = "RejectRevisions";
                RejectRevisions.Height = 23;
                RejectRevisions.Width = 95;
                //RejectRevisions.IsEnabled = false;
                RejectRevisions.Click += new RoutedEventHandler(cpv.RejectRevisions_Clicked);

                StackPanel buttons = new StackPanel();
                buttons.Orientation = System.Windows.Controls.Orientation.Horizontal;
                buttons.Children.Add(AcceptRevisions);
                buttons.Children.Add(RejectRevisions);

                StackPanel buttonPanel = new StackPanel();
                buttonPanel.Margin = new Thickness(5.0, 60.0, 5.0, 10.0);

                buttonPanel.Children.Add(CrowdproofButton);
                buttonPanel.Children.Add(buttons);

                grid.Children.Add(buttonPanel);

                sidebar.alertSidebar(this.data.job);
            }
        }

        public void dataReceived(object sender, RoutedEventArgs e)
        {
            this.dataReceived();
            e.Handled = true;
            
        }

        public void registerSidebar(Sidebar sidebar)
        {
            this.sidebar = sidebar;
        }
    }
}
