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
using System.Diagnostics;
using System.Windows.Interop;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using Soylent.Model.HumanMacro;

namespace Soylent.View.HumanMacro
{
    /// <summary>
    /// Interaction logic for HumanMacroDialog.xaml
    /// </summary>
    public partial class HumanMacroDialog : UserControl
    {
        Word.Range text;

        public string subtitle
        {
            get
            {
                int sentenceEnd = -1;
                for (int i = 0; i < instructions.Length; i++)
                {
                    if (Char.IsPunctuation(instructions[i])) {
                        sentenceEnd = i;
                        break;
                    }
                }

                if (sentenceEnd != -1)
                {
                    _subtitle = instructions.Substring(0, sentenceEnd+1);
                }
                else
                {
                    _subtitle = instructions;
                }

                return _subtitle;
            }
        }
        private string _subtitle;

        public string title
        {
            set
            {
                _title = value;
                titleLabel.GetBindingExpression(Label.ContentProperty).UpdateTarget();
            }
            get
            {
                return _title;
            }
        }
        private string _title;

        public string totalPrice
        {
            get
            {
                double value = payment * numRepetitions * numSections;
                return String.Format("{0:C}", (object) value);
            }
        }

        public string testRunPrice
        {
            get
            {
                return String.Format("{0:C}", payment * numRepetitions);
            }
        }

        public int numSections
        {
            get
            {
                return text.Paragraphs.Count;
            }
        }

        public string firstUnit
        {
            get
            {
                return text.Paragraphs.First.Range.Text;
            }
        }

        public int numRepetitions
        {
            get
            {
                return _numRepetitions;
            }
            set
            {
                _numRepetitions = value;
                testSpent.GetBindingExpression(Label.ContentProperty).UpdateTarget();
                totalSpent.GetBindingExpression(Label.ContentProperty).UpdateTarget();
            }
        }
        private int _numRepetitions = 4;

        public double payment
        {
            get
            {
                return _payment;
            }
            set
            {
                _payment = value;
                testSpent.GetBindingExpression(Label.ContentProperty).UpdateTarget();
                totalSpent.GetBindingExpression(Label.ContentProperty).UpdateTarget();
            }
        }
        private double _payment = .02;

        public string fullInstructions
        {
            get
            {
                string full = _instructions;
                if (example.Trim() != "")
                {
                    full += "\r\n\r\nExample:\r\n";
                    full += example;
                }
                _fullInstructions = full;

                return _fullInstructions;
            }
            set
            {
                _fullInstructions = value;
            }
        }
        private string _fullInstructions = "";

        public string example
        {
            get
            {
                return _example;
            }
            set
            {
                _example = value;
                instructionTextBlock.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            }
        }
        private string _example = "";

        public string instructions
        {
            get
            {
                return _instructions;
            }
            set
            {
                _instructions = value;
                instructionTextBlock.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                subtitleLabel.GetBindingExpression(Label.ContentProperty).UpdateTarget();
            }
        }
        private string _instructions = "";

        private int jobNumber;

        /*
        public string separator
        {
            get
            {
                return _separator;
            }
            set
            {
                _separator = value;
                separatorBox.GetBindingExpression(ComboBox.).UpdateTarget();
            }
        }
        private string _separator = "Paragraph";
        */

        private ComboBoxItem item1;
        private ComboBoxItem item2;
        private ComboBoxItem returnAsComments;
        private ComboBoxItem returnAsInline;

        public HumanMacroDialog(Word.Range text, int jobNumber)
        {
            this.text = text;
            this.jobNumber = jobNumber;
            InitializeComponent();


            Binding binding = new Binding();
            binding.Source = text;
            binding.Path = new PropertyPath("Text");
            textToWorkWith.SetBinding(TextBox.TextProperty, binding);

            numItems.Content = numSections + " paragraph" + (numSections == 1 ? "" : "s") + " selected, each as a separate task";

            item1 = new ComboBoxItem();
            item1.Content = "Paragraph";
            item2 = new ComboBoxItem();
            item2.Content = "Sentence";

            separatorBox.Items.Add(item1);
            separatorBox.Items.Add(item2);
            separatorBox.SelectedValue = item1;

            returnAsComments = new ComboBoxItem();
            returnAsComments.Content = "Comments";
            returnAsInline = new ComboBoxItem();
            returnAsInline.Content = "In-Line Changes";
            returnTypeBox.Items.Add(returnAsComments);
            returnTypeBox.Items.Add(returnAsInline);
            returnTypeBox.SelectedValue = returnAsComments;
        }

        public void RunMacro_Click(object sender, RoutedEventArgs e)
        {
            HumanMacroData.Separator separator = HumanMacroData.Separator.Sentence;
            if (separatorBox.SelectedItem == item2) { separator = HumanMacroData.Separator.Sentence; }
            else if (separatorBox.SelectedItem == item1) { separator = HumanMacroData.Separator.Paragraph; }

            double reward; int redundancy; string localtitle; string localsubtitle; string localinstructions;

            /*
            if (Soylent.DEBUG == true)
            {
                reward = 0.05;
                redundancy = 2;
                localtitle = "\"Make my novel present tense\"";
                localsubtitle = "\"I need to change some prose from past to present tense\"";
                localinstructions = "'I am changing this section of my novel from the past tense to the present tense. Please read and fix to make everything present tense, e.g., \"Susan swerved and aimed the gun at her assailant. The man recoiled, realizing that his prey had now caught on to the scheme.\" becomes \"Susan swerves and aims the gun at her assailant. The man recoils, realizing that his prey had now caught on to the scheme.\"'";
            }
            else
            {
             */
                reward = payment;
                redundancy = numRepetitions;
                localtitle = title;
                localsubtitle = subtitle;
                localinstructions = instructions;
            //}
                
            HumanMacroData.ReturnType type = HumanMacroData.ReturnType.Comment;
            if (returnTypeBox.SelectedItem == returnAsComments) { type = HumanMacroData.ReturnType.Comment; }
            else if (returnTypeBox.SelectedItem == returnAsInline) { type = HumanMacroData.ReturnType.SmartTag; }


            //Debug.WriteLine("########################");
            //Debug.WriteLine("Reward: " + reward + " || Redundancy: "+redundancy+" || Title: "+localtitle+" || Subtitle: "+localsubtitle+" || Instructions: "+localinstructions);
            //Debug.WriteLine(separatorBox.SelectedValue.ToString() + " 1 " + (item2 == separatorBox.SelectedValue) + " 2 " + (item2 == separatorBox.SelectionBoxItem) + " 3 " + (item2 == separatorBox.SelectedItem) + " 4 " + (item2.Content == separatorBox.SelectedValuePath));

            //HumanMacroData data = new HumanMacroData(text, jobNumber, separator, reward, redundancy, localtitle, localsubtitle, localinstructions, type, HumanMacroData.TestOrReal.Real);

            //HumanMacroJob job = new HumanMacroJob(data, jobNumber);
            HumanMacroJob job = new HumanMacroJob(text, jobNumber, separator, reward, redundancy, localtitle, localsubtitle, localinstructions, type, HumanMacroData.TestOrReal.Real);

            HwndSource source = (HwndSource)PresentationSource.FromVisual(sender as Button);
            System.Windows.Forms.Control ctl = System.Windows.Forms.Control.FromChildHandle(source.Handle);
            ctl.FindForm().Close();
        }

        private void TestMacro_Click(object sender, RoutedEventArgs e)
        {
            HumanMacroData.Separator separator = HumanMacroData.Separator.Sentence;
            if (separatorBox.SelectedItem == item2) { separator = HumanMacroData.Separator.Sentence; }
            else if (separatorBox.SelectedItem == item1) { separator = HumanMacroData.Separator.Paragraph; }

            double reward; int redundancy; string localtitle; string localsubtitle; string localinstructions;

            /*
            if (Soylent.DEBUG == true)
            {
                reward = 0.05;
                redundancy = 2;
                localtitle = "\"Make my novel present tense\"";
                localsubtitle = "\"I need to change some prose from past to present tense\"";
                localinstructions = "'I am changing this section of my novel from the past tense to the present tense. Please read and fix to make everything present tense, e.g., \"Susan swerved and aimed the gun at her assailant. The man recoiled, realizing that his prey had now caught on to the scheme.\" becomes \"Susan swerves and aims the gun at her assailant. The man recoils, realizing that his prey had now caught on to the scheme.\"'";
            }
            else
            {
             */
            reward = payment;
            redundancy = numRepetitions;
            localtitle = title;
            localsubtitle = subtitle;
            localinstructions = instructions;
            //}

            HumanMacroData.ReturnType type = HumanMacroData.ReturnType.Comment;
            if (returnTypeBox.SelectedItem == returnAsComments) { type = HumanMacroData.ReturnType.Comment; }
            else if (returnTypeBox.SelectedItem == returnAsInline) { type = HumanMacroData.ReturnType.SmartTag; }


            //Debug.WriteLine("########################");
            //Debug.WriteLine("Reward: " + reward + " || Redundancy: "+redundancy+" || Title: "+localtitle+" || Subtitle: "+localsubtitle+" || Instructions: "+localinstructions);
            //Debug.WriteLine(separatorBox.SelectedValue.ToString() + " 1 " + (item2 == separatorBox.SelectedValue) + " 2 " + (item2 == separatorBox.SelectionBoxItem) + " 3 " + (item2 == separatorBox.SelectedItem) + " 4 " + (item2.Content == separatorBox.SelectedValuePath));

            //HumanMacroData data = new HumanMacroData(text, jobNumber, separator, reward, redundancy, localtitle, localsubtitle, localinstructions, type, HumanMacroData.TestOrReal.Test);

            HumanMacroJob job = new HumanMacroJob(text, jobNumber, separator, reward, redundancy, localtitle, localsubtitle, localinstructions, type, HumanMacroData.TestOrReal.Test);

            HwndSource source = (HwndSource)PresentationSource.FromVisual(sender as Button);
            System.Windows.Forms.Control ctl = System.Windows.Forms.Control.FromChildHandle(source.Handle);
            ctl.FindForm().Close();
        }
    }
}
