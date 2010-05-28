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

namespace Soylent
{
    /// <summary>
    /// Interaction logic for HITView.xaml
    /// </summary>
    public partial class HITView : UserControl
    {
        public Dictionary<HITData.ResultType, StageView> stageList;
        public HITData data;

        //public HITView(string workType, string originalText)
        public HITView(string workType, HITData data)
        {
            InitializeComponent();

            hitType.Content = workType;
            this.data = data;
            previewText.Text = data.originalText;
            stageList = new Dictionary<HITData.ResultType,StageView>();
        }

        public void addStage(int stageNum, HITData.ResultType type, string stageType, int totalTurkers, double totalCost) {
            StageData stagedata = data.stages[type];
            StageView newStage = new StageView(stageNum, type, stagedata, stageType, totalTurkers, totalCost);
            stages.Children.Insert(stages.Children.IndexOf(cancelBtn), newStage);
            stageList[type] = newStage;
            //stageList.Add(newStage);
        }

        public void updateView()
        {
            
            /*
            foreach (StageView stage in stageList)
            {
                stage.updateProgress(4,2);
            }
             */
        }

        //TODO: Move to subclass
        public void Shortn_Clicked(object sender, RoutedEventArgs e)
        {
            foreach(StageView stageview in stageList.Values)
            {
               // data.
                //stageview.hitProgress.Foreground = Brushes.Blue;
            }
        }
        
    }
}
