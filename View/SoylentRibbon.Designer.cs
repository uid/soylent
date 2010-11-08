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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SoylentRibbon));
            this.Soylent = new Microsoft.Office.Tools.Ribbon.RibbonTab();
            this.viewGroup = new Microsoft.Office.Tools.Ribbon.RibbonGroup();
            //this.button6 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.button3 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.shortenBtn = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.humanMacroBtn = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.jobStatus = new Microsoft.Office.Tools.Ribbon.RibbonToggleButton();
            this.debug = new Microsoft.Office.Tools.Ribbon.RibbonGroup();
            this.button1 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.button2 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.button4 = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.directManipulate = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.humanMacroInline = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.humanMacroComment = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.Soylent.SuspendLayout();
            this.viewGroup.SuspendLayout();
            this.debug.SuspendLayout();
            this.SuspendLayout();
            // 
            // Soylent
            // 
            this.Soylent.Groups.Add(this.viewGroup);
            this.Soylent.Groups.Add(this.debug);
            this.Soylent.Label = "Soylent";
            this.Soylent.Name = "Soylent";
            // 
            // viewGroup
            // 
            this.viewGroup.Items.Add(this.button3);
            this.viewGroup.Items.Add(this.shortenBtn);
            this.viewGroup.Items.Add(this.humanMacroBtn);
            this.viewGroup.Items.Add(this.jobStatus);
            this.viewGroup.Name = "viewGroup";
            // 
            this.jobStatus.Checked = true;
            // button6
            // 
            //this.button6.Label = "button6";
            //this.button6.Name = "button6";
            //this.button6.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.button6_Click);
            // 
            // button3
            // 
            this.button3.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
            this.button3.Label = "Crowdproof";
            this.button3.Name = "button3";
            this.button3.ShowImage = true;
            this.button3.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.button3_Click);
            // 
            // shortenBtn
            // 
            this.shortenBtn.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.shortenBtn.Image = ((System.Drawing.Image)(resources.GetObject("shortenBtn.Image")));
            this.shortenBtn.Label = "Shortn";
            this.shortenBtn.Name = "shortenBtn";
            this.shortenBtn.ScreenTip = "Shortn";
            this.shortenBtn.ShowImage = true;
            this.shortenBtn.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.shortenBtn_Click);
            // 
            // humanMacroBtn
            // 
            this.humanMacroBtn.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.humanMacroBtn.Image = global::Soylent.Properties.Resources.humanmacro;
            this.humanMacroBtn.Label = "Human Macro";
            this.humanMacroBtn.Name = "humanMacroBtn";
            this.humanMacroBtn.ShowImage = true;
            this.humanMacroBtn.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.humanMacroBtn_Click);
            // 
            // jobStatus
            // 
            this.jobStatus.Label = "Job Status";
            this.jobStatus.Name = "jobStatus";
            this.jobStatus.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.jobStatus_Click);
            // 
            // debug
            // 
            this.debug.Items.Add(this.button1);
            this.debug.Items.Add(this.button2);
            this.debug.Items.Add(this.button4);
            this.debug.Items.Add(this.directManipulate);
            this.debug.Items.Add(this.humanMacroInline);
            this.debug.Items.Add(this.humanMacroComment);
            this.debug.Label = "Debug";
            this.debug.Name = "debug";
            this.debug.Visible = false;
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
            // directManipulate
            this.directManipulate.Label = "Shorten Window";
            this.directManipulate.Name = "directManipulate";
            this.directManipulate.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.directManipulate_Click);
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
            this.debug.ResumeLayout(false);
            this.debug.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab Soylent;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton directManipulate;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton humanMacroBtn;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton humanMacroInline;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton humanMacroComment;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup debug;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button3;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup viewGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton jobStatus;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button4;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton shortenBtn;
    }

    partial class ThisRibbonCollection : Microsoft.Office.Tools.Ribbon.RibbonReadOnlyCollection
    {
        internal SoylentRibbon Ribbon
        {
            get { return this.GetRibbon<SoylentRibbon>(); }
        }
    }
}
