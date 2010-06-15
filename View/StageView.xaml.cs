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
using System.Windows.Media.Animation;

using Soylent.Model;

namespace Soylent.View
{
    /// <summary>
    /// Interaction logic for StageView.xaml
    /// </summary>
    public partial class StageView : UserControl
    {
        private int totalTurkers;
        private double totalCost;
        private HITData.ResultType type;
        private StageData stagedata;

        public StageView(int stageNum, HITData.ResultType type, StageData stagedata, string stageType, int totalTurkers, double totalCost)
        {
            InitializeComponent();

            this.stagedata = stagedata;
            this.type = type;
            this.totalTurkers = totalTurkers;
            this.totalCost = totalCost;
            stagedata.registerListener(this);

            stageName.Content = String.Format("Stage {0}: {1:c}", stageNum, stageType);

            updateProgress(0, 0);
        }

        /// <summary>
        /// Update the View with new status information
        /// </summary>
        /// <param name="curTurkers">The number of Turkers who have completed this stage</param>
        /// <param name="curCost">The current cost of those Turkers</param>
        public void updateProgress(int curTurkers, double curCost)
        {
            numTurkers.Content = curTurkers + " of " + totalTurkers + " workers";
            cost.Content = String.Format("{0:c}", curCost);

            double percentDone = ((double)curTurkers) / totalTurkers;
            Duration duration = new Duration(TimeSpan.FromSeconds(1));
            DoubleAnimation doubleanimation = new DoubleAnimation(100 * percentDone, duration);
            hitProgress.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
        }
        /// <summary>
        /// Update the View with new status information. Reflects all changes in model data.
        /// </summary>
        public void updateProgress()
        {
            totalTurkers = stagedata.numRequested;
            totalCost = stagedata.moneySpent;
            int curTurkers = stagedata.numCompleted;
            numTurkers.Content = curTurkers + " of " + totalTurkers + " workers";
            cost.Content = String.Format("{0:c}", totalCost);

            double percentDone = ((double)curTurkers) / totalTurkers;
            Duration duration = new Duration(TimeSpan.FromSeconds(1));
            DoubleAnimation doubleanimation = new DoubleAnimation(100 * percentDone, duration);
            hitProgress.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
        }
        public delegate void updateProgressDelegate();
        /// <summary>
        /// Notifies this View of changes in the Model
        /// </summary>
        public void notify()
        {
            Globals.Soylent.soylent.Invoke(new updateProgressDelegate(this.updateProgress), new object[] { });
            //updateProgress(stagedata.numCompleted);
        }
    }
}
