using MentorAlgorithm.Algorithm;
using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            ListTraffics.ItemsSource = Traffics;

        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            if (NumberOfNodes <= 0 || Threshold <= 0 || Capacity < 0 || Radius < 0)
                return;

            Mentor mentor = new Mentor(NumberOfNodes, Capacity, Threshold, Radius);
            mentor.GenerateNodes();
            mentor.GenerateCosts(0.4);

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

            Plotter.DataContext = mentor;

            //logger
            Logger.Inlines.Add("Lưu lượng giữa các nút: \n");
            foreach(var item in mentor.Traffics)
                Logger.Inlines.Add("T(" + item.Key.Item1.Name + ", " + item.Key.Item2.Name + ") = " + item.Value + "\t");

            Logger.Inlines.Add("\nTrọng số của các nút: \n");
            for (int i = 0; i < mentor.NumberOfNode; i++)
                Logger.Inlines.Add("W(" + i + ") = " + mentor.Nodes[i].Traffic + "\t");

            //Logger.Inlines.Add("\nLưu lượng thực tế đi qua nút backbones: \n");
            //Dictionary<Tuple<Node, Node>, double> realTrafficBackbones = mentor.RealTrafficBackbones();
            //foreach(var item in realTrafficBackbones)
            //    Logger.Inlines.Add("T(" + item.Key.Item1.Name + ", " + item.Key.Item2.Name + ") = " + item.Value + "\t");

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
    }

    public class Traffic
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Value { get; set; }
    }
}
