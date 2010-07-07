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
    /// Interaction logic for HITView.xaml
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

        /*
        public void child_updated(object sender, EventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            try
            {
                sp.Height = sp.DesiredSize.Height;
            }
            catch { }
        }
        */

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
                    Console.WriteLine("DoubleAnimation TargetElement is set!");
                    StackPanel sp = anim.TargetElement as StackPanel;
                    //expand(sp);
                    shrink(sp);
                    //sp.Height = Double.NaN;
                    //sp.ClearValue(HeightProperty);
                    //sp.MaxHeight = sp.MaxHeight + 100;
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
                    StackPanel sp = anim.TargetElement as StackPanel;
                    expand(sp);
                    //sp.Height = Double.NaN;
                    //sp.ClearValue(HeightProperty);
                    //sp.MaxHeight = sp.MaxHeight + 100;
                }
            }
        } 

        private void option1_click(object sender, RoutedEventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            StackPanel usedToBe = currentlyExpanded;

            if (currentlyExpanded != null)
            {
                /*
                if (currentlyExpanded == option1)
                {
                    shrink();
                }
                else
                {
                    oldShrink();
                }*/
                double initialHeight = sp.DesiredSize.Height;

                StackPanel sub = new StackPanel();

                shrink();
                this.UpdateLayout();
                double finalHeight = views[sp].stub.DesiredSize.Height;
                sub.Height = initialHeight - finalHeight;
                sp.Children.Add(sub);
                this.UpdateLayout();

                //sp.Height = initialHeight;
                //sp.MinHeight = initialHeight; sp.MaxHeight = initialHeight;     
                //double percentDone = 0.50;

                //doubleanimation.Completed += new EventHandler(animationDone);

                Duration duration = new Duration(TimeSpan.FromSeconds(0.50));
                //DoubleAnimation doubleanimation = new DoubleAnimation(finalHeight, duration);
                DoubleAnimationPlus doubleanimation = new DoubleAnimationPlus();
                doubleanimation.Duration = duration;
                doubleanimation.To = 0;

                doubleanimation.TargetElement = sp;


                doubleanimation.Completed += new EventHandler(ShrinkAnimationCompleted);

                //sp.BeginAnimation(MaxHeightProperty, doubleanimation);
                sub.BeginAnimation(HeightProperty, doubleanimation);
                //sp.Height = finalHeight;
                
                //sp.BeginAnimation(MinHeightProperty, doubleanimation);
                //sp.BeginAnimation(MaxHeightProperty, doubleanimation);
                //sp.Height = Double.NaN;
            }
            if (usedToBe != sp)
            {
                double initialHeight = sp.DesiredSize.Height;

                expand(sp);
                this.UpdateLayout();
                double finalHeight = views[sp].DesiredSize.Height;
                shrink();
                //views[sp].Height = initialHeight;
                this.UpdateLayout();

                StackPanel sub = new StackPanel();
                sp.Children.Add(sub);

                sub.Height = 0;
               
                
                Duration duration = new Duration(TimeSpan.FromSeconds(0.50));
                DoubleAnimationPlus doubleanimation = new DoubleAnimationPlus();
                doubleanimation.Duration = duration;
                doubleanimation.To = finalHeight - initialHeight;

                doubleanimation.TargetElement = sp;
                
                doubleanimation.Completed += new EventHandler(AnimationCompleted);
                
                sub.BeginAnimation(HeightProperty, doubleanimation);
                //views[sp].BeginAnimation(HeightProperty, doubleanimation);
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

        private void oldExpand(StackPanel sp)
        {
            foreach (UIElement elm in sp.Children)
                {
                    Label lb = elm as Label;
                    if (lb != null)
                    {
                        lb.FontWeight = FontWeights.Bold;
                    }
                }
                sp.Background = Brushes.WhiteSmoke;

                ProgressBar pb = new ProgressBar(); ProgressBar pb2 = new ProgressBar(); ProgressBar pb3 = new ProgressBar();
                pb.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch; pb.Height = 20;
                pb2.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch; pb2.Height = 20;
                pb3.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch; pb3.Height = 20;

                Label label1 = new Label(); Label label2 = new Label(); Label label3 = new Label();
                label1.Name = "find"; label1.Content = "Find:";
                label2.Name = "fixx"; label2.Content = "Fix:";
                label3.Name = "verify"; label3.Content = "Verify:";
                //double percentDone = 0.50;
                //Duration duration = new Duration(TimeSpan.FromSeconds(0.25));
                //DoubleAnimation doubleanimation = new DoubleAnimation((double)(60), duration);
                //pb2.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
                pb2.SetValue(ProgressBar.ValueProperty, 50.0);

                Button button = new Button();
                button.Content = "Button";
                button.Click += Button_Click;
                button.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
                button.Height = 30;

                sp.Children.Add(label1);
                sp.Children.Add(pb);
                sp.Children.Add(label2);
                sp.Children.Add(pb2);
                sp.Children.Add(label3);
                sp.Children.Add(pb3);
                sp.Children.Add(button);

                Border border = new Border();
                border.BorderBrush = Brushes.WhiteSmoke;
                border.Height = 5;

                sp.Children.Add(border);

                //sp.BeginAnimation(HeightProperty,doubleanimation); 

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
            /*
            Label hitType = new Label();
            hitType.Content = views[option1].hitType.Content;
            currentlyExpanded.Children.Add(hitType);
            Label preview = new Label();
            preview.Content = views[option1].previewText.Text;
            preview.Height = 38;
            currentlyExpanded.Children.Add(preview);
             * */
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

        private void oldShrink()
        {
            currentlyExpanded.Background = Brushes.LightGray;
                List<UIElement> list = new List<UIElement>();
                foreach (UIElement element in currentlyExpanded.Children)
                {
                    if (element is Label){
                        if ((element as Label).Name.Substring(0, 4) == "base")
                        {
                            (element as Label).FontWeight = FontWeights.Regular;
                        }
                        else
                        {
                            list.Add(element);
                        }
                    }
                    //if ((element != this.base2) && (element != this.base1) && (element != this.base3))
                    else
                    {
                        list.Add(element);
                    }
                    /*
                    else
                    {
                        (element as Label).FontWeight = FontWeights.Regular;
                    }
                     * */
                }
                foreach (UIElement element in list)
                {
                    currentlyExpanded.Children.Remove(element);
                }
                currentlyExpanded = null;
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
