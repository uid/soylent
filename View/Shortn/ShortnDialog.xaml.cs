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
using Soylent.Model;
using Soylent.Model.Shortn;

namespace Soylent.View.Shortn
{
    /// <summary>
    /// Interaction logic for ShortenDialog.xaml
    /// </summary>
    public partial class ShortnDialog : UserControl
    {
        private ShortnData data;

        /// <summary>
        /// The dialog window that opens when a user wants to interact with returned Shortn data.
        /// </summary>
        /// <param name="data">The Model for this Shortn job</param>
        public ShortnDialog(ShortnData data)
        {
            InitializeComponent();
            this.data = data;
            updateParagraphs(1);
            initSliderTicks();
        }

        private void clickHandler(object sender, RoutedEventArgs e)
        {
            Run run = sender as Run;
            run.Foreground = Brushes.Green;
        }

        private List<Run> getOriginalRuns(List<PatchSelection> selections)
        {
            List<Run> runs = new List<Run>();

            foreach (PatchSelection selection in selections)
            {
                Run r = new Run(selection.patch.original);

                r.MouseUp += new MouseButtonEventHandler(clickHandler);
                r.Cursor = Cursors.Hand;

                if (!selection.isOriginal)
                {
                    r.Foreground = Brushes.Red;
                }
                else if (!(selection.patch is DummyPatch))
                {
                    r.Foreground = Brushes.Purple;
                }

                runs.Add(r);
            }
            return runs;
        }

        private List<Run> getShortenedRuns(List<PatchSelection> selections)
        {
            List<Run> runs = new List<Run>();

            foreach (PatchSelection selection in selections)
            {
                Run r = new Run(selection.selection);

                if (!selection.isOriginal)
                {
                    r.Foreground = Brushes.Red;
                }

                runs.Add(r);
            }
            return runs;
        }

        private void lengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (data == null)
                return;
            double max = lengthSlider.Maximum;
            double percent = e.NewValue / max;

            updateParagraphs(percent);
            //data.makeChangesInDocument((int)Math.Round(data.longestLength * percent));
            //Globals.Soylent.soylent.Invoke(new makeChangesInDocumentDelegate(this.makeChangesInDocument), new object[] { (int)Math.Round(data.longestLength * percent) });
            makeChangesInDocument((int)Math.Round(data.longestLength * percent));
        }

        private delegate void makeChangesInDocumentDelegate(int var);
        private void makeChangesInDocument(int var)
        {
            data.makeChangesInDocument(var);
        }

        private void updateParagraphs(double percent)
        {
            //int lengthVaration = (data.longestLength - data.shortestLength);
            //int newLength = (int)Math.Round(lengthVaration * percent) + data.shortestLength;
            int newLength = (int)Math.Round(data.longestLength * percent);

            List<PatchSelection> selections = data.getPatchSelections(newLength);

            List<Run> beforeRuns = getOriginalRuns(selections);
            before.Inlines.Clear();
            before.Inlines.AddRange(beforeRuns);

            List<Run> afterRuns = getShortenedRuns(selections);
            after.Inlines.Clear();
            after.Inlines.AddRange(afterRuns);
        }

        private void initSliderTicks()
        {
            List<int> lengths = data.possibleLengths();
            DoubleCollection tickMarks = new DoubleCollection();

            foreach (int length in lengths) {
                double percent = ((double)length) / data.longestLength;
                tickMarks.Add(percent * 100);
            }
            lengthSlider.Ticks = tickMarks;

            lengthSlider.SelectionStart = tickMarks[0];
        }
    }
}
