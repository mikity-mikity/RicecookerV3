using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace mikity.visualize
{
    /// <summary>
    /// fullScreen.xaml の相互作用ロジック
    /// </summary>
    public partial class fullScreen : Window
    {
        public void activate()
        {
            this.Activate();
            this.view.Opacity = 1.0;
        }
        public void deactivate()
        {
            this.view.Opacity = 0;
        }
        public double getDt()
        {
            return dt.getDt();
        }
        public fullScreen()
        {
            InitializeComponent();
            init(false);
        }
        public fullScreen(bool dev)
        {
            InitializeComponent();
            init(dev);
        }
        public void init(bool dev)
        {
            drift0();
            setDbgText("Test");
            if (!dev)
            {
                stack1.Visibility = System.Windows.Visibility.Collapsed;
                stack2.Visibility = System.Windows.Visibility.Collapsed;
                stack3.Visibility = System.Windows.Visibility.Collapsed;
                dbg.Visibility = System.Windows.Visibility.Collapsed;
                Dock0.Children.Remove(TT);
                Dock1.Children.Add(TT);
                dt.SetValue(DockPanel.DockProperty, Dock.Right);
                TT.SetValue(DockPanel.DockProperty, Dock.Left);
                window.SizeChanged += window_SizeChanged;
            }
        }

        void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender == window)
            {
                view.Width=e.NewSize.Width / 2;
                dockPanel1.Width = e.NewSize.Width / 2;
            }
        }
        public void addNorm(double v,double v2)
        {
            this.graph2.add(v);
            this.graph3.add(v2);
        }
        public void clearNorm()
        {
            this.graph2.Clear();
            this.graph3.Clear();
        }
        public void renewPlot(Func<double, double> d)
        {
            graph.renew(d);
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }
        public void onRF()
        {
            _rf2.Content = "ON";
            _rf2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void offRF()
        {
            _rf2.Content = "OFF";
            _rf2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E2");
        }
        public void onNorm()
        {
            _norm2.Content = "ON";
            _norm2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void offNorm()
        {
            _norm2.Content = "OFF";
            _norm2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E2");
        }
        public void onIF()
        {
            _if2.Content = "ON";
            _if2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void offIF()
        {
            _if2.Content = "OFF";
            _if2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E2");
        }
        public void onGeo()
        {
            _geo2.Content = "ON";
            _geo2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void offGeo()
        {
            _geo2.Content = "OFF";
            _geo2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E2");
        }
        public void onGo()
        {
            TT.Child = (UIElement)this.FindResource("D2");
        }
        public void resetGo()
        {
            TT.Child = (UIElement)this.FindResource("D1");
        }
        public void pauseGo()
        {

            TT.Child = (UIElement)this.FindResource("D3");

        }
        public void drift0()
        {
            _drift2.Content = "Viscous";
            _drift2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E2");
        }
        public void drift1()
        {
            _drift2.Content = "Drift!";
            _drift2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void drift2()
        {
            _drift2.Content = "Kinetic";
            _drift2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void drift3()
        {
            _drift2.Content = "None";
            _drift2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void move(double val)
        {
            graph.move(val);
        }
        public void setDbgText(string text)
        {
            this.dbg.Text = text;
        }
        public Action<string> _selectMaterial;
        private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            if(_selectMaterial!=null)
            _selectMaterial((string)((Label)((ComboBoxItem)sender).Content).Content);
        }
    }
}
