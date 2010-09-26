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
using System.Windows.Threading;
using System.Diagnostics;
using Soylent.Model;
using Soylent.Model.Shortn;

namespace Soylent.View.Shortn
{
    /// <summary>
    /// Interaction logic for ShortenDialog.xaml
    /// </summary>
    public partial class ShortnDialog : UserControl
    {
        private Dictionary<Run, PatchSelection> runMap;
        private ShortnData data;
        private double currentPercent;
        private string rootDirectory = null;
        private string UNLOCK_TEXT = "(vary)"; //This is the text for the context menu option that unlocks the selection

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

        // Makes left clicks open the context menu
        private void clickHandler(object sender, RoutedEventArgs e)
        {
            Run run = sender as Run;
            run.ContextMenu.PlacementTarget = this;
            run.ContextMenu.IsOpen = true;
        }

        // Makes left clicks open the context menu on the lock icons
        private void lockClickHandler(object sender, RoutedEventArgs e)
        {
            InlineUIContainer iuc = sender as InlineUIContainer;
            iuc.ContextMenu.PlacementTarget = this;
            iuc.ContextMenu.IsOpen = true;
        }

        private List<Run> getOriginalRuns(List<PatchSelection> selections)
        {
            List<Run> runs = new List<Run>();

            foreach (PatchSelection selection in selections)
            {
                string runText = selection.patch.original;
                if (runText.EndsWith("\r"))
                {
                    runText += "\r"; // line break between paragraphs
                }
                Run r = new Run(runText);

                if (!selection.isOriginal)
                {
                    r.Foreground = Brushes.Red;
                }
                else if (!(selection.patch is DummyPatch))
                {
                    r.Foreground = Brushes.Purple;
                }
                else
                {
                    r.Foreground = Brushes.Black;
                }
                if ((selection.patch as ShortnPatch).isLocked)
                {
                    //r.Foreground = Brushes.Green;
                }

                runs.Add(r);
            }

            Run lastRun = runs[runs.Count - 1];
            lastRun.Text = lastRun.Text.TrimEnd(new char[] {'\r'});

            return runs;
        }

        private List<Run> getShortenedRuns(List<PatchSelection> selections)
        {
            List<Run> runs = new List<Run>();

            foreach (PatchSelection selection in selections)
            {
                string runText = selection.selection;
                if (runText.EndsWith("\r"))
                {
                    runText += "\r"; // line break between paragraphs
                }
                Run r = new Run(runText);

                if (!selection.isOriginal)
                {
                    r.Foreground = Brushes.Red;
                    r.ForceCursor = true;
                    r.Cursor = Cursors.Hand;
                    r.MouseUp += new MouseButtonEventHandler(clickHandler);

                    r.TextDecorations.Add(new TextDecoration(TextDecorationLocation.Underline, null, 0, TextDecorationUnit.Pixel, TextDecorationUnit.Pixel));

                    // Add the proper context menu
                    ContextMenu cm = new ContextMenu();

                    // Make the option to unlock the selection
                    MenuItem i = new MenuItem();
                    i.Header = UNLOCK_TEXT;
                    i.Click += new RoutedEventHandler(contextMenuHandler);
                    i.Tag = r;
                    cm.Items.Add(i);
                    
                    foreach (string replacement in selection.patch.replacements)
                    {
                        MenuItem i1 = new MenuItem();
                        i1.Header = replacement;
                        i1.Click += new RoutedEventHandler(contextMenuHandler);

                        i1.Tag = r;
                        cm.Items.Add(i1);
                    }
                    cm.MaxWidth = 400;
                    //cm.Width = 400; // Wasn't sure what to set this to.
                    r.ContextMenu = cm;
                }
                else if (!(selection.patch is DummyPatch))
                {
                    r.Foreground = Brushes.Purple;
                    r.ForceCursor = true;
                    r.Cursor = Cursors.Hand;
                    r.MouseUp += new MouseButtonEventHandler(clickHandler);

                    r.TextDecorations.Add(new TextDecoration(TextDecorationLocation.Underline, null, 0, TextDecorationUnit.Pixel, TextDecorationUnit.Pixel));

                    ContextMenu cm = new ContextMenu();

                    MenuItem i = new MenuItem();
                    i.Header = UNLOCK_TEXT;
                    i.Click += new RoutedEventHandler(contextMenuHandler);
                    i.Tag = r;
                    cm.Items.Add(i);

                    foreach (string replacement in selection.patch.replacements)
                    {
                        MenuItem i1 = new MenuItem();
                        i1.Header = replacement;
                        i1.Click += new RoutedEventHandler(contextMenuHandler);

                        i1.Tag = r;
                        cm.Items.Add(i1);
                    }
                    //cm.Width = 400; // Not sure
                    r.ContextMenu = cm;

                }
                else
                {
                    r.Foreground = Brushes.Black;
                }
                if ((selection.patch as ShortnPatch).isLocked)
                {
                    r.Foreground = Brushes.Green;
                }

                runs.Add(r);
                runMap.Add(r, selection);
            }

            Run lastRun = runs[runs.Count - 1];
            lastRun.Text = lastRun.Text.TrimEnd(new char[] { '\r' });
            return runs;
        }

        /// <summary>
        /// The handler for the context menu on variable runs.  This locks / unlocks them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuHandler(System.Object sender, System.EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Run run = item.Tag as Run;

            ShortnPatch patch = runMap[run].patch as ShortnPatch;
            if ((item.Header as string) != UNLOCK_TEXT)
            {
                /*int index = 0;
                foreach (Inline inline in after.Inlines)
                {
                    if (!(inline is Run)) {
                        index++;
                        continue; 
                    }
                    Run tempRun = inline as Run;
                    if (tempRun == run)
                    {
                        break;
                    }
                    index++;
                }*/

                patch.lockSelection(item.Header as string);
                run.Foreground = Brushes.Green;
                updateParagraphs(currentPercent);   //refresh everything
                initSliderTicks(false);        
            }
            else
            {
                patch.unlockSelection();
                run.Foreground = Brushes.Red;
                updateParagraphs(currentPercent);
                initSliderTicks(false);
            }
        }

        private void lengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (data == null)
                return;
            double max = lengthSlider.Maximum;
            double percent = e.NewValue / max;

            updateParagraphs(percent);

            // set a timer to go off a few moments later and see if they're still at that tick mark. If so,
            // update the document. Otherwise, just update the UI.
            
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(changeDocumentTick);
            dispatcherTimer.Tag = percent;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            
            //makeChangesInDocument((int)Math.Round(data.longestLength * percent));
        }

        private delegate void changeDocumentTickDelegate(object sender, EventArgs e);
        private void changeDocumentTick(object sender, EventArgs e) {
            double curPercent = lengthSlider.Value / lengthSlider.Maximum;
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.Stop();
            double savedPercent = ((double) timer.Tag);

            if (curPercent == savedPercent) {
                makeChangesInDocument((int)Math.Round(data.longestLength * curPercent));
            }
        }

        private delegate void makeChangesInDocumentDelegate(int var);
        private void makeChangesInDocument(int var)
        {
            data.makeChangesInDocument(var);
        }

        /// <summary>
        /// Updates both text boxes.  Applies lock icons to locked runs in the after box
        /// </summary>
        /// <param name="percent"></param>
        private void updateParagraphs(double percent)
        {
            currentPercent = percent;
            int newLength = (int)Math.Round(data.longestLength * percent);

            List<PatchSelection> selections = data.getPatchSelections(newLength);

            runMap = new Dictionary<Run, PatchSelection>();

            List<Run> beforeRuns = getOriginalRuns(selections);
            before.Inlines.Clear();
            before.Inlines.AddRange(beforeRuns);

            List<Run> afterRuns = getShortenedRuns(selections);
            after.Inlines.Clear();
            after.Inlines.AddRange(afterRuns);

            // Need to get the directory for the image
            if (rootDirectory == null)
            {
                rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (rootDirectory.Length > 10)
                {
                    if (rootDirectory.Substring(rootDirectory.Length - 11, 10) == @"\bin\Debug")
                    {
                        rootDirectory = rootDirectory.Substring(0, rootDirectory.Length - 10);
                    }
                }
            }

            //The list of locked runs.  Must be dealt with later to avoid altering the collection we're iterating through
            List<Run> lockedRuns = new List<Run>();

            foreach (Inline inline in after.Inlines)
            {
                if (!(inline is Run))
                {
                    continue;
                }
                Run tempRun = inline as Run;

                if ((runMap[tempRun].patch as ShortnPatch).isLocked)
                {
                    lockedRuns.Add(tempRun);
                }
            }
            // Add the lock icon to the locked runs
            foreach (Run run in lockedRuns)
            {
                Image img = new Image();
                BitmapImage bmi = new BitmapImage(new Uri(rootDirectory + @"lock.png"));
                img.Source = bmi;
                img.Height = 9;
                img.Width = 9;

                InlineUIContainer iuc = new InlineUIContainer(img, run.ContentEnd); // Can kill Word if that run.TextPointer is not currently displayed

                iuc.ContextMenu = run.ContextMenu;
                iuc.Cursor = Cursors.Hand;
                iuc.MouseUp +=new MouseButtonEventHandler(lockClickHandler);
            }
        }

        private void initSliderTicks(bool useCache)
        {
            List<int> lengths = data.possibleLengths(useCache);
            DoubleCollection tickMarks = new DoubleCollection();

            foreach (int length in lengths)
            {
                double percent = ((double)length) / data.longestLength;
                tickMarks.Add(percent * 100);
            }
            lengthSlider.Ticks = tickMarks;

            lengthSlider.SelectionStart = tickMarks[0];
        }

        private void initSliderTicks()
        {
            initSliderTicks(true);
        }
    }
}
