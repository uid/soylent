namespace Soylent
{
    partial class SoylentRibbon
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Soylent = new Microsoft.Office.Tools.Ribbon.RibbonTab();
            this.viewGroup = new Microsoft.Office.Tools.Ribbon.RibbonGroup();
            this.jobStatus = new Microsoft.Office.Tools.Ribbon.RibbonToggleButton();
            this.button5 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.group3 = new Microsoft.Office.Tools.Ribbon.RibbonGroup();
            this.button3 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.group1 = new Microsoft.Office.Tools.Ribbon.RibbonGroup();
            this.shortenBtn = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.directManipulate = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.group2 = new Microsoft.Office.Tools.Ribbon.RibbonGroup();
            this.humanMacroBtn = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.humanMacroInline = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.humanMacroComment = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.group4 = new Microsoft.Office.Tools.Ribbon.RibbonGroup();
            this.button1 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.button2 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.button4 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.button6 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.Soylent.SuspendLayout();
            this.viewGroup.SuspendLayout();
            this.group3.SuspendLayout();
            this.group1.SuspendLayout();
            this.group2.SuspendLayout();
            this.group4.SuspendLayout();
            this.SuspendLayout();
            // 
            // Soylent
            // 
            this.Soylent.Groups.Add(this.viewGroup);
            this.Soylent.Groups.Add(this.group3);
            this.Soylent.Groups.Add(this.group1);
            this.Soylent.Groups.Add(this.group2);
            this.Soylent.Groups.Add(this.group4);
            this.Soylent.Label = "Soylent";
            this.Soylent.Name = "Soylent";
            // 
            // viewGroup
            // 
            this.viewGroup.Items.Add(this.jobStatus);
            this.viewGroup.Items.Add(this.button5);
            this.viewGroup.Items.Add(this.button6);
            this.viewGroup.Label = "View";
            this.viewGroup.Name = "viewGroup";
            // 
            // jobStatus
            // 
            this.jobStatus.Label = "Job Status";
            this.jobStatus.Name = "jobStatus";
            this.jobStatus.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.jobStatus_Click);
            // 
            // button5
            // 
            this.button5.Label = "button5";
            this.button5.Name = "button5";
            this.button5.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.button5_Click);
            // 
            // group3
            // 
            this.group3.Items.Add(this.button3);
            this.group3.Label = "Proofreading";
            this.group3.Name = "group3";
            // 
            // button3
            // 
            this.button3.Label = "Proofread";
            this.button3.Name = "button3";
            this.button3.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.button3_Click);
            // 
            // group1
            // 
            this.group1.Items.Add(this.shortenBtn);
            this.group1.Items.Add(this.directManipulate);
            this.group1.Label = "Shortn";
            this.group1.Name = "group1";
            // 
            // shortenBtn
            // 
            this.shortenBtn.Label = "Shorten Text";
            this.shortenBtn.Name = "shortenBtn";
            this.shortenBtn.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.shortenBtn_Click);
            // 
            // directManipulate
            // 
            this.directManipulate.Label = "Shorten Window";
            this.directManipulate.Name = "directManipulate";
            this.directManipulate.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.directManipulate_Click);
            // 
            // group2
            // 
            this.group2.Items.Add(this.humanMacroBtn);
            this.group2.Items.Add(this.humanMacroInline);
            this.group2.Items.Add(this.humanMacroComment);
            this.group2.Label = "The Human Macro";
            this.group2.Name = "group2";
            // 
            // humanMacroBtn
            // 
            this.humanMacroBtn.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.humanMacroBtn.Label = "Issue Request";
            this.humanMacroBtn.Name = "humanMacroBtn";
            this.humanMacroBtn.ShowImage = true;
            this.humanMacroBtn.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.humanMacroBtn_Click);
            // 
            // humanMacroInline
            // 
            this.humanMacroInline.Label = "Inline";
            this.humanMacroInline.Name = "humanMacroInline";
            this.humanMacroInline.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.humanMacroInline_Click);
            // 
            // humanMacroComment
            // 
            this.humanMacroComment.Label = "Comment";
            this.humanMacroComment.Name = "humanMacroComment";
            this.humanMacroComment.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.humanMacroComment_Click);
            // 
            // group4
            // 
            this.group4.Items.Add(this.button1);
            this.group4.Items.Add(this.button2);
            this.group4.Items.Add(this.button4);
            this.group4.Label = "debug";
            this.group4.Name = "group4";
            // 
            // button1
            // 
            this.button1.Label = "Start Socket Server";
            this.button1.Name = "button1";
            this.button1.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Label = "Start TurKit";
            this.button2.Name = "button2";
            this.button2.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.button2_Click);
            // 
            // button4
            // 
            this.button4.Label = "button4";
            this.button4.Name = "button4";
            this.button4.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.button4_Click);
            // 
            // button6
            // 
            this.button6.Label = "button6";
            this.button6.Name = "button6";
            this.button6.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.button6_Click);
            // 
            // SoylentRibbon
            // 
            this.Name = "SoylentRibbon";
            this.RibbonType = "Microsoft.Word.Document";
            this.Tabs.Add(this.Soylent);
            this.Load += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonUIEventArgs>(this.Ribbon_Load);
            this.Soylent.ResumeLayout(false);
            this.Soylent.PerformLayout();
            this.viewGroup.ResumeLayout(false);
            this.viewGroup.PerformLayout();
            this.group3.ResumeLayout(false);
            this.group3.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.group2.ResumeLayout(false);
            this.group2.PerformLayout();
            this.group4.ResumeLayout(false);
            this.group4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab Soylent;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton shortenBtn;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton directManipulate;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton humanMacroBtn;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton humanMacroInline;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton humanMacroComment;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group3;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group4;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button3;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup viewGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton jobStatus;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button4;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button5;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button6;
    }

    partial class ThisRibbonCollection : Microsoft.Office.Tools.Ribbon.RibbonReadOnlyCollection
    {
        internal SoylentRibbon Ribbon
        {
            get { return this.GetRibbon<SoylentRibbon>(); }
        }
    }
}
