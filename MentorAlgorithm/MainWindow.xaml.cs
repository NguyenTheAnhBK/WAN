using MentorAlgorithm.Algorithm;
using MentorAlgorithm.Helpers;
using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MentorAlgorithm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Traffic> Traffics { get; set; } = new List<Traffic>();
        public int NumberOfNodes { get; set; } = 90;
        public int Threshold { get; set; } = 2;
        public int Capacity { get; set; } = 10;
        public double Radius { get; set; } = 0.3;
        public double Alpha { get; set; } = 0.5;
        public double Umin { get; set; } = 0.7;

        Mentor mentor;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            ListTraffics.ItemsSource = Traffics;
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
                btnContinue.IsEnabled = false;
                
                if (NumberOfNodes <= 0 || Threshold <= 0 || Capacity < 0 || Radius < 0)
                    return;

                mentor = new Mentor(NumberOfNodes, Capacity, Threshold, Radius, Alpha, Umin);
                mentor.GenerateNodes();
                mentor.GenerateCosts(0.4);

                Execute();
            //}
            //catch
            //{
            //    MessageBox.Show("Initialize not empty!!!", "Warning");
            //}
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            if (NumberOfNodes <= 0 || Threshold <= 0 || Capacity < 0 || Radius < 0)
                return;

            var nodes = mentor.Nodes.Select(node => new Node(node.X, node.Y, node.Name));
            //var costs = mentor.Costs;
            //var maxCost = mentor._maxCost;
            mentor = new Mentor(NumberOfNodes, Capacity, Threshold, Radius, Alpha, Umin);
            //mentor.GenerateNodes();
            mentor.Nodes = nodes.ToList();
            //mentor.Costs = costs;
            //mentor._maxCost = maxCost;
            mentor.GenerateCosts(0.4);
            Execute();
        }

        private void Execute()
        {
            try
            {
                var listTraffics = ListTraffics.ItemsSource as List<Traffic>;
                for (int i = 0; i < listTraffics.Count; i++)
                {
                    string from = listTraffics[i].From, to = listTraffics[i].To;
                    if (!from.Contains('i') && !to.Contains('i'))
                    {
                        mentor.SetTraffic(mentor.Nodes[int.Parse(from)], mentor.Nodes[int.Parse(to)], listTraffics[i].Value);
                        continue;
                    }
                    for (int j = 0; j < mentor.NumberOfNode; j++)
                    {
                        int f = Helpers.Helper.Evaluate(from.Replace("i", j.ToString()));
                        int t = Helpers.Helper.Evaluate(to.Replace("i", j.ToString()));
                        if (t < mentor.NumberOfNode)
                            mentor.SetTraffic(mentor.Nodes[f], mentor.Nodes[t], listTraffics[i].Value);
                    }
                }

                //for (int i = 0; i < mentor.NumberOfNode; i++)
                //{
                //    if (i + 2 < mentor.NumberOfNode)
                //        mentor.SetTraffic(mentor.Nodes[i], mentor.Nodes[i + 2], 1);
                //    if (i + 8 < mentor.NumberOfNode)
                //        mentor.SetTraffic(mentor.Nodes[i], mentor.Nodes[i + 8], 2);
                //    if (i + 12 < mentor.NumberOfNode)
                //        mentor.SetTraffic(mentor.Nodes[i], mentor.Nodes[i + 12], 3);
                //}
                //mentor.SetTraffic(mentor.Nodes[13], mentor.Nodes[47], 15);
                //mentor.SetTraffic(mentor.Nodes[34], mentor.Nodes[69], 13);
                //mentor.SetTraffic(mentor.Nodes[20], mentor.Nodes[38], 30);
                //mentor.SetTraffic(mentor.Nodes[45], mentor.Nodes[29], 10);

                mentor.FindBackbone(x => x.Add(new Node(double.NaN, double.NaN, "Null")));

                Plotter.Annotations.Clear();
                for (int i = 0; i < mentor.NumberOfNode; i++)
                {
                    var textAnnotation = new TextAnnotation
                    {
                        Text = i.ToString(),
                        TextPosition = new DataPoint(mentor.Nodes[i].X, mentor.Nodes[i].Y),
                        StrokeThickness = 0,
                        FontSize = 11,
                        TextColor = Colors.White,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center
                    };
                    Plotter.Annotations.Add(textAnnotation);
                }

                //Chuyển lưu lượng giữa các nút backbone và dựng cây prim-dijkstra giữa các nút backbone
                mentor.ConnectBackbone();
                
                //Liên kết trên cây sau khi áp dụng mentor 2
                var temp = mentor.LinksResult.ToDictionary(k => k.Key, v => v.Value.Item1);
                foreach (var item in temp)
                {
                    if (item.Value > 0)
                    {
                        mentor.TreeLinks.Add(item.Key.Item1);
                        mentor.TreeLinks.Add(item.Key.Item2);
                        mentor.TreeLinks.Add(new Node(double.NaN, double.NaN, "Null"));
                    }
                }

                Plotter.DataContext = mentor;

                //logger

                if (overwriteConsole.IsChecked == true)
                    Logger.Inlines.Clear();

                LogLine("1. ");
                LogLine("Lưu lượng giữa các nút:");
                foreach (var item in mentor.Traffics)
                    Log("T(" + item.Key.Item1.Name + ", " + item.Key.Item2.Name + ") = " + item.Value + "\t");

                LogLine("Trọng số của các nút:");
                for (int i = 0; i < mentor.NumberOfNode; i++)
                    Log("W(" + i + ") = " + mentor.Nodes[i].Traffic + "\t");

                LogLine("Lưu lượng thực tế đi qua các nút backbones:");
                //foreach (var item in mentor._trafficBackbones)
                //    Log("T(" + item.Key.Item1.Name + ", " + item.Key.Item2.Name + ") = " + item.Value + "\t");
                Log(mentor._trafficBackbones.ToStringTable("Node", mentor.Backbones, node => node.Name));

                LogLine("2. ");
                LogLine("Số đường sử dụng trên từng liên kết và độ sử dụng trên liên kết đó: link(node i, node j) = (n, u)");
                //foreach (var item in mentor.LinksResult)
                //    Log("Link(" + item.Key.Item1.Name + ", " + item.Key.Item2.Name + ") = (" + item.Value.Item1 + ", " + item.Value.Item2.ToString() + ")\t");
                LogLine(temp.ToStringTable("Node", mentor.Backbones, node => node.Name));
                LogLine(mentor.LinksResult.ToDictionary(k => k.Key, v => v.Value.Item2).ToStringTable("Node", mentor.Backbones, node => node.Name));
                


                LogLine("Giá thay đổi trên liên kết trực tiếp sau khi dùng Mentor 2: ");
                //foreach (var item in mentor.D)
                //    Log("( " + item.Key.Item1.Name + ", " + item.Key.Item2.Name + ") = " + item.Value + "\t");
                //var columnHeaders = new List<string> { "Node" };
                //columnHeaders.AddRange(mentor.Backbones.Select(x => x.Name));
                //Log(mentor.D.ToStringTable(columnHeaders.ToArray(), node => node.Value));
                //LogLine("Trước khi thêm liên kết trực tiếp: ");
                //LogLine(mentor.OldD);
                //LogLine("Sau khi thêm liên kết trực tiếp: ");
                //LogLine(mentor.D.ToStringTable("Node", mentor.Backbones, node => node.Name));

                //LogLine("   *Giá: ");
                LogLine("Trước khi thêm liên kết trực tiếp: ");
                LogLine(mentor.d.ToStringTable("Node", mentor.Backbones, node => node.Name));
                LogLine("Sau khi thêm liên kết trực tiếp: ");
                LogLine(mentor.newCost.ToStringTable("Node", mentor.Backbones, node => node.Name));

                btnContinue.IsEnabled = true;
            }
            catch
            {
                MessageBox.Show("Initialize not empty!!!", "Warning");
            }

        }

        private void GridPlotter_Checked(object sender, RoutedEventArgs e)
        {
            Plotter.Axes[0].MajorGridlineStyle = LineStyle.Solid; //major: chính
            Plotter.Axes[0].MinorGridlineStyle = LineStyle.Solid; //minor: phụ

            Plotter.Axes[1].MajorGridlineStyle = LineStyle.Solid;
            Plotter.Axes[1].MinorGridlineStyle = LineStyle.Solid;
        }

        private void GridPlotter_Unchecked(object sender, RoutedEventArgs e)
        {
            Plotter.Axes[0].MajorGridlineStyle = LineStyle.None; //major: chính
            Plotter.Axes[0].MinorGridlineStyle = LineStyle.None; //minor: phụ

            Plotter.Axes[1].MajorGridlineStyle = LineStyle.None;
            Plotter.Axes[1].MinorGridlineStyle = LineStyle.None;
        }

        private void AppendConsole_Checked(object sender, RoutedEventArgs e)
        {
            overwriteConsole.IsChecked = false;
        }

        private void OverwriteConsole_Checked(object sender, RoutedEventArgs e)
        {
            appendConsole.IsChecked = false;
        }

        public void Log(string text)
        {
            Logger.Inlines.Add(text);
        }
        public void LogLine(string text)
        {
            Logger.Inlines.Add("\n" + text + "\n");
        }
    }

    public class Traffic
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Value { get; set; }
    }
}
