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
using Word = Microsoft.Office.Interop.Word;
using System.Diagnostics;

namespace Soylent.View
{
    /// <summary>
    /// Interaction logic for HITView.xaml
    /// </summary>
    public partial class HITView : UserControl
    {
        public Dictionary<HITData.ResultType, StageView> stageList;
        public HITData data;
        public HITViewStub stub;
        public int job;

        /// <summary>
        /// The view for a task in the sidebar
        /// </summary>
        /// <param name="workType">Job type</param>
        /// <param name="data">Data Model for this View</param>
        public HITView(string workType, HITData data, int job)
        {
            InitializeComponent();

            hitType.Content = workType;

            this.data = data;
            data.register(this);
            previewText.Text = data.originalText;
            stageList = new Dictionary<HITData.ResultType,StageView>();

            stub = new HITViewStub(workType, data, this);

            this.job = job;
            cancelBtn.IsEnabled = true;
        }

        /// <summary>
        /// Add a stage to this container
        /// </summary>
        /// <param name="stageNum"></param>
        /// <param name="type"></param>
        /// <param name="stageType"></param>
        /// <param name="totalTurkers"></param>
        /// <param name="totalCost"></param>
        public void addStage(int stageNum, HITData.ResultType type, StageData sdata, string stageType, int totalTurkers, double totalCost, int job) {
            StageData stagedata = sdata;
            StageView newStage = new StageView(stageNum, type, stagedata, stageType, totalTurkers, totalCost, job);
            stages.Children.Insert(stages.Children.IndexOf(cancelBtn), newStage);
            stageList[type] = newStage;
            //stageList.Add(newStage);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            /*
            object missing = System.Reflection.Missing.Value;
            object what = Word.WdGoToItem.wdGoToLine;
            object which = Word.WdGoToDirection.wdGoToFirst;
            data.range.GoTo(ref what, ref which, ref missing, ref missing);
             */
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            AmazonKeys.AskForAmazonKeys((AmazonKeys keys) =>
            {
                data.tk.cancelTask(keys);
                RemoveHITVIew();
            }, null);
        }

        /// <summary>
        /// Removes the HITView from the sidebar
        /// </summary>
        public void RemoveHITVIew()
        {
            Word.Document doc = Globals.Soylent.Application.ActiveDocument;

            UIElementCollection children = Globals.Soylent.soylentMap[doc].sidebar.jobs.Children;

            // We might be in stub mode or in full mode
            children.Remove((UIElement)this.Parent);
            children.Remove(((UIElement)stub.Parent));
            //Globals.Soylent.soylentMap[doc].sidebar.jobs.Children.Clear();
        }

        public void clearErrors()
        {
            IEnumerable<UIElement> errors = stages.Children.Cast<UIElement>().Where(child => child.GetType() == typeof(HITError)).ToList();
            foreach (UIElement error in errors)
            {
                stages.Children.Remove(error);
            }

            IEnumerable<UIElement> stubErrors = stub.details.Children.Cast<UIElement>().Where(child => child.GetType() == typeof(HITError)).ToList();
            foreach (UIElement error in stubErrors)
            {
                stub.details.Children.Remove(error);
            }
        }

        public void showError(string exceptionCode)
        {
            // First we remove old errors, so they don't get added multiple times
            clearErrors();

            if (exceptionCode == "AWS.MechanicalTurk.InsufficientFunds")
            {
                string errorText = @"You have run out of Mechanical Turk funds. Soylent cannot continue to post tasks.";
                string resolutionText = @"Purchase more Mechanical Turk credit.";
                string url = @"https://requester.mturk.com/mturk/prepurchase";
                HITError error = new HITError(errorText, resolutionText, url);
                stages.Children.Insert(0, error);
                
                HITError stubError = new HITError(errorText, resolutionText, url);
                stub.details.Children.Insert(0, stubError);
            }
            else
            {
                HITError error = new HITError(exceptionCode);
                HITError stubError = new HITError(exceptionCode);
                stages.Children.Insert(0, error);
                stub.details.Children.Insert(0, stubError);
            }
        }
    }
}
