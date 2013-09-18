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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mikity.visualize
{
    /// <summary>
    /// Plot.xaml の相互作用ロジック
    /// </summary>
    public partial class Plot2 : UserControl
    {
        List<double> vals = new List<double>();
        List<double> lastVals;
        Func<double, double> converterX;
        Func<double, double> converterY;
        public void add(double val)
        {
            vals.Add(val);
            update();
        }
        public void Clear()
        {
            lastVals = vals;
            vals=new List<double>();
            minX = 0;
            maxX = 0;
            minY = 0;
            maxY = 100;

        }
        int minX=0, maxX=0;
        double minY=0, maxY=100;
        public void update()
        {
            double aMinY = 0;
            /*if (vals.Count > 1000)
            {
                vals=vals.Skip(vals.Count-81).Take(81).ToList();
            }*/
            if (vals.Count < 1) return;
            if (vals.Count < 81)
            {
                minX = 0;
                maxX = vals.Count - 1;
                minY = minY * 0.9 + vals.Min() * 0.1;
                maxY = maxY * 0.9 + vals.Max() * 0.1;
                aMinY = vals.Min();
            }
            else
            {
                minX = vals.Count - 81;
                maxX = vals.Count - 1;
                var d=vals.Skip(minX).Take(81);
                minY = minY*0.9+d.Min()*0.1;
                maxY = maxY*0.9+d.Max()*0.1;
                aMinY = d.Min();
            }
            converterX = (v) => { return (v - minX) / 80d * 200d; };
            converterY = (v) => { double f = -(v - minY) / (maxY - minY) * 80 + 90; return f; };
            Canvas.SetTop(min, converterY(aMinY)-min.ActualHeight);
            Canvas.SetLeft(min, converterX(minX));
            minLine.Y1 = converterY(aMinY);
            minLine.Y2 = converterY(aMinY);
            min.Content = minY.ToString("G3");
            for (int i = 0; i < 80; i++)
            {
                lines[i].X1=converterX(i+minX);
                if(i+minX>maxX)
                {
                    lines[i].Y1 = converterY(vals[maxX]);
                }
                else
                {
                    lines[i].Y1=converterY(vals[i+minX]);
                }
                lines[i].X2 = converterX((i + 1) + minX);
                if(i+1+minX>maxX)
                {
                    lines[i].Y2 = converterY(vals[maxX]);
                }
                else
                {
                    lines[i].Y2=converterY(vals[i+1+minX]);
                }
            }
        }
        Line[] lines = new Line[80];
        public Plot2()
        {
            InitializeComponent();
            vals.Clear();
            for (int i = 0; i < 80; i++)
            {
                lines[i] = new Line();
                lines[i].StrokeThickness = 1;
                lines[i].Stroke = Brushes.Black;
                canvas.Children.Add(lines[i]);
            }
            this.update();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (vals != null&&vals.Count>0)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "result"; // Default file name
                dlg.DefaultExt = ".csv"; // Default file extension
                dlg.Filter = "csv document|*.csv"; // Filter files by extension 

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    // Save document 
                    string filename = dlg.FileName;
                    var sw = new System.IO.StreamWriter(filename, false);
                    for (int i = 0; i < vals.Count; i++)
                    {
                        sw.WriteLine((i + 1).ToString() + "," + vals[i].ToString());
                    }
                    sw.Close();
                }
            }
            else
            {
                MessageBox.Show("You don't have a valid data to export");
            }
        }
        private void MenuItem_Click2(object sender, RoutedEventArgs e)
        {
            if (lastVals != null&&lastVals.Count>0)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "result"; // Default file name
                dlg.DefaultExt = ".csv"; // Default file extension
                dlg.Filter = "csv document|*.csv"; // Filter files by extension 

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    // Save document 
                    string filename = dlg.FileName;
                    var sw = new System.IO.StreamWriter(filename, false);
                    for (int i = 0; i < vals.Count; i++)
                    {
                        sw.WriteLine((i + 1).ToString() + "," + lastVals[i].ToString());
                    } sw.Close();
                }
            }
            else
            {
                MessageBox.Show("You don't have a valid data to export");
            }
        }
    }
}
