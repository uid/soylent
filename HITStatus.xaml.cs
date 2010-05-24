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
    /// Interaction logic for HITStatus.xaml
    /// </summary>
    public partial class HITStatus : UserControl
    {
        public List<StageStatus> stageList { get; set; }

        public HITStatus(string workType, string originalText)
        {
            InitializeComponent();

            hitType.Content = workType;
            previewText.Text = originalText;
            stageList = new List<StageStatus>();
        }

        public void addStage(int stageNum, string stageType, int totalTurkers, double totalCost) {
            StageStatus newStage = new StageStatus(stageNum, stageType, totalTurkers, totalCost);
            stages.Children.Insert(stages.Children.IndexOf(cancelBtn), newStage);
            stageList.Add(newStage);
        }

        
    }
}
