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
using Soylent.Model;
using Soylent.View.Crowdproof;
using Soylent.View.HumanMacro;
using Soylent.View.Shortn;

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
        internal Button ShortnButton;
        internal Button CrowdproofButton;
        internal Button AcceptRevisions;
        internal Button RejectRevisions;


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
        }

        public void ShortnDataReceived()
        {
            grid.Children.Remove(hitProgress);
            this.hitType.FontWeight = FontWeights.ExtraBold;
            
            ShortnView spv = view as ShortnView;

            ShortnButton = new Button();
            ShortnButton.Content = "Shortn";
            ShortnButton.Name = "Shortn";
            ShortnButton.Height = 23;
            //ShortnButton.Width = 80;
            //ShortnButton.IsEnabled = false;
            ShortnButton.Click += new RoutedEventHandler(spv.Shortn_Clicked);

            StackPanel buttonPanel = new StackPanel();
            buttonPanel.Margin = new Thickness(5.0, 88.0, 5.0, 10.0);

            buttonPanel.Children.Add(ShortnButton);

            grid.Children.Add(buttonPanel);

            sidebar.alertSidebar(this.data.job);
        }

        public void CrowdproofDataReceived(){
            grid.Children.Remove(hitProgress);
            this.hitType.FontWeight = FontWeights.ExtraBold;

            if (((string) hitType.Content) == "Crowdproof")
            {
                CrowdproofView cpv = view as CrowdproofView;

                CrowdproofButton = new Button();
                CrowdproofButton.Content = "View Revisions";
                CrowdproofButton.Name = "Crowdproof";
                CrowdproofButton.Height = 23;
                //CrowdproofButton.Width = 90;
                //CrowdproofButton.IsEnabled = false;
                CrowdproofButton.Click += new RoutedEventHandler(cpv.Crowdproof_Clicked);

                AcceptRevisions = new Button();
                AcceptRevisions.Content = "Accept All";
                AcceptRevisions.Name = "AcceptRevisions";
                AcceptRevisions.Height = 23;
                AcceptRevisions.Width = 95;
                //AcceptRevisions.IsEnabled = false;
                AcceptRevisions.Click += new RoutedEventHandler(cpv.AcceptRevisions_Clicked);

                RejectRevisions = new Button();
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
                buttonPanel.Margin = new Thickness(5.0, 88.0, 5.0, 10.0);

                buttonPanel.Children.Add(CrowdproofButton);
                buttonPanel.Children.Add(buttons);

                grid.Children.Add(buttonPanel);

                sidebar.alertSidebar(this.data.job);
            }
        }

        public void HumanMacroDataReceived()
        {
            grid.Children.Remove(hitProgress);
            this.hitType.FontWeight = FontWeights.ExtraBold;

                HumanMacroView hpv = view as HumanMacroView;

                CrowdproofButton = new Button();
                CrowdproofButton.Content = "View Revisions";
                CrowdproofButton.Name = "Crowdproof";
                CrowdproofButton.Height = 23;
                //CrowdproofButton.Width = 90;
                //CrowdproofButton.IsEnabled = false;
                CrowdproofButton.Click += new RoutedEventHandler(hpv.HumanMacro_Clicked);

                AcceptRevisions = new Button();
                AcceptRevisions.Content = "Accept All";
                AcceptRevisions.Name = "AcceptRevisions";
                AcceptRevisions.Height = 23;
                AcceptRevisions.Width = 95;
                //AcceptRevisions.IsEnabled = false;
                AcceptRevisions.Click += new RoutedEventHandler(hpv.AcceptRevisions_Clicked);

                RejectRevisions = new Button();
                RejectRevisions.Content = "Reject All";
                RejectRevisions.Name = "RejectRevisions";
                RejectRevisions.Height = 23;
                RejectRevisions.Width = 95;
                //RejectRevisions.IsEnabled = false;
                RejectRevisions.Click += new RoutedEventHandler(hpv.RejectRevisions_Clicked);

                StackPanel buttons = new StackPanel();
                buttons.Orientation = System.Windows.Controls.Orientation.Horizontal;
                buttons.Children.Add(AcceptRevisions);
                buttons.Children.Add(RejectRevisions);

                StackPanel buttonPanel = new StackPanel();
                buttonPanel.Margin = new Thickness(5.0, 88.0, 5.0, 10.0);

                buttonPanel.Children.Add(CrowdproofButton);
                buttonPanel.Children.Add(buttons);

                grid.Children.Add(buttonPanel);

                sidebar.alertSidebar(this.data.job);
        }


        public void registerSidebar(Sidebar sidebar)
        {
            this.sidebar = sidebar;
        }

        public delegate void updateViewDelegate(double percent, double cost);

        public void updateView(double percent, double cost){
            this.cost.Content = "$" + cost;

            Duration duration = new Duration(TimeSpan.FromSeconds(0.5));
            DoubleAnimation doubleanimation = new DoubleAnimation(100 * percent, duration);
            hitProgress.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            //hitProgress.Value = percent * 100;
        }
    }
}
