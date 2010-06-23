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

        public string separator
        {
            get
            {
                return separator;
            }
            set
            {
                separator = value;
            }
        }

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
        }

        public void RunMacro_Click(object sender, RoutedEventArgs e)
        {
            HumanMacroResult.Separator separator = HumanMacroResult.Separator.Sentence;
            string separatorText = "Paragraph";
            if (separatorText == "Sentence") { separator = HumanMacroResult.Separator.Sentence; }
            else if (separatorText == "Paragraph") { separator = HumanMacroResult.Separator.Paragraph; }

            double reward = 0.05;
            int redundancy = 2;
            string title = "\"Make my novel present tense\"";
            string subtitle = "\"I need to change some prose from past to present tense\"";
            string instructions = "'I am changing this section of my novel from the past tense to the present tense. Please read and fix to make everything present tense, e.g., \"Susan swerved and aimed the gun at her assailant. The man recoiled, realizing that his prey had now caught on to the scheme.\" becomes \"Susan swerves and aims the gun at her assailant. The man recoils, realizing that his prey had now caught on to the scheme.\"'";
            HumanMacroResult.ReturnType type = HumanMacroResult.ReturnType.Comment;

            HumanMacroResult data = new HumanMacroResult(text, jobNumber, separator, reward, redundancy, title, subtitle, instructions, type);

            HumanMacroJob job = new HumanMacroJob(data, jobNumber);
        }
    }
}
