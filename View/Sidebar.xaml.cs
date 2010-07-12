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
using System.Diagnostics;

using System.Windows.Media.Animation;

namespace Soylent.View
{
    /// <summary>
    /// This sidebar is the task pane that fills with items representing HITs   
    /// </summary>
    public partial class Sidebar : UserControl
    {
        StackPanel currentlyExpanded;
        Dictionary<StackPanel,HITView> views;
        Dictionary<int,StackPanel> panels;

        public Sidebar()
        {
            InitializeComponent();
            views = new Dictionary<StackPanel,HITView>();
            panels = new Dictionary<int, StackPanel>();
        }

        private void AddCustomXmlPartToActiveDocument(Microsoft.Office.Interop.Word.Document document)
        {
            string xmlString =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                "<employees xmlns=\"http://schemas.microsoft.com/vsto/samples\">" +
                    "<employee>" +
                        "<name>Karina Leal</name>" +
                        "<hireDate>1999-04-01</hireDate>" +
                        "<title>Manager</title>" +
                    "</employee>" +
                "</employees>";

            Microsoft.Office.Core.CustomXMLPart employeeXMLPart = document.CustomXMLParts.Add(xmlString);
            
        }




        public void addHitView(int jobNumber,HITView view)
        {
            StackPanel sp = new StackPanel();
            sp.MouseUp += option1_click;
            sp.Background = Brushes.LightGray;

            panels[jobNumber] = sp;
            views.Add(sp, view);

            jobs.Children.Add(sp);

            Border border = new Border();
            border.BorderThickness = new Thickness(2.0); border.Height = 1;
            border.BorderBrush = Brushes.Gray;

            sp.Cursor = Cursors.Hand;

            jobs.Children.Add(border);

            sp.Children.Add(view.stub);

            view.stub.registerSidebar(this);

            //sp.LayoutUpdated += child_updated;

            Expander ex = new Expander();
            //ex.Header = view.stub;
            //ex.Content = view;

            //jobs.Children.Add(ex);
        }

        public void alertSidebar(int jobNumber)
        {
            StackPanel sp = panels[jobNumber];
            //sp.UpdateLayout();
            //this.UpdateLayout();

            
            //double i = 0;
            //Debug.WriteLine(sp.MaxHeight);
            //sp.MinHeight = views[sp].stub.DesiredSize.Height;
            //sp.MaxHeight = views[sp].stub.DesiredSize.Height;
            //i = views[sp].stub.DesiredSize.Height;
            //Debug.WriteLine(sp.MaxHeight);
            //this.UpdateLayout();

        }

        private void ShrinkAnimationCompleted(object sender, EventArgs e)
        {
            Clock c = sender as Clock;
            DoubleAnimationPlus anim = c.Timeline as DoubleAnimationPlus;

            if (anim != null)
            {
                if (anim.TargetElement != null)
                {
                    //Console.WriteLine("DoubleAnimation TargetElement is set!");
                    StackPanel sp = anim.TargetElement as StackPanel;
                    shrink(sp);
                }
            }
        }

        internal class DoubleAnimationPlus : DoubleAnimation
        {
            private UIElement _target;

            public UIElement TargetElement
            {
                get { return _target; }
                set { _target = value; }
            }

            protected override Freezable CreateInstanceCore()
            {
                DoubleAnimationPlus p = new DoubleAnimationPlus();
                p.TargetElement = this.TargetElement;
                return p;
            }
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            Clock c = sender as Clock;
            DoubleAnimationPlus anim = c.Timeline as DoubleAnimationPlus;

            if (anim != null)
            {
                if (anim.TargetElement != null)
                {
                    Console.WriteLine("DoubleAnimation TargetElement is set!");
                    //StackPanel sp = anim.TargetElement as StackPanel;
                    //expand(sp);
                }
            }
        } 

        private void option1_click(object sender, RoutedEventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            StackPanel usedToBe = currentlyExpanded;

            if (currentlyExpanded != null)
            {
                double initialHeight = usedToBe.DesiredSize.Height;

                StackPanel sub = new StackPanel();

                shrink();
                this.UpdateLayout();
                double finalHeight = views[usedToBe].stub.DesiredSize.Height;
                sub.Height = initialHeight - finalHeight;
                usedToBe.Children.Add(sub);
                //views[sp].stub.Height = initialHeight;
                this.UpdateLayout();

                Duration duration = new Duration(TimeSpan.FromSeconds(0.50));
                DoubleAnimationPlus doubleanimation = new DoubleAnimationPlus();
                doubleanimation.Duration = duration;
                doubleanimation.To = 0;

                doubleanimation.TargetElement = usedToBe;


                doubleanimation.Completed += new EventHandler(ShrinkAnimationCompleted);

                //views[sp].stub.BeginAnimation(HeightProperty, doubleanimation);
                sub.BeginAnimation(HeightProperty, doubleanimation);

            }
            if (usedToBe != sp)
            {
                double initialHeight = sp.DesiredSize.Height;

                expand(sp);
                this.UpdateLayout();
                double finalHeight = views[sp].DesiredSize.Height;
                //shrink();
                views[sp].Height = initialHeight;
                this.UpdateLayout();

                
       
                //StackPanel sub = new StackPanel();
                //sp.Children.Add(sub);

                //sub.Height = 0;
               
                
                Duration duration = new Duration(TimeSpan.FromSeconds(0.50));
                DoubleAnimationPlus doubleanimation = new DoubleAnimationPlus();
                doubleanimation.Duration = duration;
                //doubleanimation.To = finalHeight - initialHeight;
                doubleanimation.To = finalHeight;
                doubleanimation.From = initialHeight;

                doubleanimation.TargetElement = sp;
                
                doubleanimation.Completed += new EventHandler(AnimationCompleted);
                
                //sub.BeginAnimation(HeightProperty, doubleanimation);
                views[sp].BeginAnimation(HeightProperty, doubleanimation);
            }

        }

        private void expand(StackPanel sp)
        {
            List<UIElement> list = new List<UIElement>();
            foreach (UIElement child in sp.Children)
            {
                list.Add(child);
            }
            foreach (UIElement child in list)
            {
                sp.Children.Remove(child);
            }

            sp.Children.Add(views[sp]);
            sp.Background = Brushes.WhiteSmoke;
            currentlyExpanded = sp;
        }

        private void shrink()
        {
            currentlyExpanded.Background = Brushes.LightGray;
            List<UIElement> list = new List<UIElement>();
            foreach (UIElement child in currentlyExpanded.Children)
            {
                list.Add(child);
            }
            foreach (UIElement child in list)
            {
                currentlyExpanded.Children.Remove(child);
            }
            currentlyExpanded.Children.Add(views[currentlyExpanded].stub);
            currentlyExpanded = null;
        }

        private void shrink(StackPanel sp)
        {
            List<UIElement> list = new List<UIElement>();
            foreach (UIElement child in sp.Children)
            {
                list.Add(child);
            }
            foreach (UIElement child in list)
            {
                sp.Children.Remove(child);
            }
            sp.Children.Add(views[sp].stub);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            b.Content = "other";

            //StageView sv = new StageView(1,HITData.ResultType.Find,new StageData(HITData.ResultType.Macro),"Macro",10,0.40);
            //option2.Children.Add(sv);
            //HITView hv = views[1];
            //option2.Children.Add(hv);

            e.Handled = true;
        }


    }
}
