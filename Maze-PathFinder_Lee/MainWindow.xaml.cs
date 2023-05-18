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
using System.Xml.XPath;
using static System.Net.Mime.MediaTypeNames;

namespace Maze_PathFinder_Lee
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MarkedLabel[,] labels;
        int gamesize = 25;
        bool isSelectingPlayer = false;
        bool isSelectingTarget = false;
        bool isSelectingWalls = false;
        bool isPlaying = false;
       
        MarkedLabel? Target;
        MarkedLabel? player;
        List<int[]> neighbors = new List<int[]>() { new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 1, 0 }, new int[] { 0, -1 } };
        public MainWindow()
        {
            InitializeComponent();
            Init();
            InfoBox.Content = "Idle";
        }
        void Init()
        {
            labels = new MarkedLabel[gamesize, gamesize];
            MainGrid.Background = Brushes.Black;
            for (int i = 0; i < gamesize; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(MainGrid.Height / gamesize) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(MainGrid.Width / gamesize) });
                for (int j = 0; j < gamesize; j++)
                {
                    Label L = new Label()
                    {
                        Background = Brushes.ForestGreen,
                        Margin = new Thickness(1, 1, 1, 1),
                    };
                    L.MouseDown += LabelClick;

                    MarkedLabel tag = new MarkedLabel(L, i, j);
                    L.Tag = tag;                   
                    MainGrid.Children.Add(L);
                    Grid.SetColumn(L, j);
                    Grid.SetRow(L, i);
                    labels[i, j] = tag;
                }
            }
            player = labels[0, 0];
            player.label.Background = Brushes.Turquoise;
        }
        private void LabelClick(object sender, MouseEventArgs e)
        {
            MarkedLabel clicked = ((sender as Label)!.Tag as MarkedLabel)!;
            if (isSelectingTarget)
            {
                if(Target != null)
                {
                    Target.label.Background = Brushes.ForestGreen;
                }                  
                Target = clicked;
                Target.label.Background = Brushes.Red;
                
            }
            else if(isSelectingWalls)
            {
                
                if(clicked.Value == 0)
                {
                    clicked.Value = -1;
                    clicked.label.Background = Brushes.Black;
                }
                else if(clicked.Value == -1)
                {
                    clicked.Value = 0;
                    clicked.label.Background = Brushes.ForestGreen;
                }
                else
                {
                    return;
                }
            }
            else
            {
                if(clicked.Value == 0)
                {
                    if(player != null)
                    {
                        player.label.Background = Brushes.ForestGreen;
                    }
                    player = clicked;
                    player.label.Background = Brushes.Turquoise;
                }
                else
                {
                    return;
                }
            }
        }

        private void PlayerSelect_click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                return;
            }
            InfoBox.Content = "Selecting player...";
            isSelectingPlayer = true;
            isSelectingTarget = false;
            isSelectingWalls = false;

        }
        private void Wall_click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                return;
            }
            InfoBox.Content = "Selecting walls...";
            isSelectingWalls = true;
            isSelectingTarget = false;
            isSelectingPlayer = false;
        }

        private void Target_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                return;
            }
            InfoBox.Content = "Selecting target...";
            isSelectingTarget = true;
            isSelectingWalls = false;
            isSelectingPlayer = false;
        }

        private async void Play_click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                return;
            }
            if (Target == null || player == null)
            {
                MessageBox.Show("No target and/or player selected!!!");
                return;
            }

            InfoBox.Content = "Playing...";
            isPlaying = true;
            isSelectingWalls = false;
            isSelectingTarget = false;
            isSelectingPlayer = false;
            
            Queue<MarkedLabel> lee = new Queue<MarkedLabel>();
            player.wasMarked = true;
            lee.Enqueue(player);
            bool found = false;
            while (lee.Count > 0 && !found)
            {
                MarkedLabel next = lee.Dequeue();
                if(next == Target)
                {
                    found = true;
                }
                List<MarkedLabel> neighbors = Neighbors(next);
                foreach(MarkedLabel n in neighbors)
                {
                    if (!n.wasMarked && n.Value >= 0)
                    {
                        n.wasMarked = true;
                        n.Value = next.Value + 1;
                        lee.Enqueue(n);
                    }
                }                  
                          
            }
            if (!found)
            {
                MessageBox.Show("There is no path between player and target!!");
                isPlaying = false;
                return;
            }

            //foreach(MarkedLabel label in labels)
            //{
            //    label.label.Content = label.Value.ToString();
            //}

            List<MarkedLabel> path = new List<MarkedLabel>();
            path.Add(Target);
            for (int i = 0; i < Target.Value; i++)
            {
                foreach (MarkedLabel label in Neighbors(path.Last()))
                {
                    if(label.Value == path.Last().Value - 1)
                    {
                        path.Add(label);
                        break;
                    }
                }
            }
            path.Reverse();
            for(int i = 0; i < path.Count; i++)
            {
                await MovePlayer(path[i]);
            }



            isPlaying = false;
            InfoBox.Content = "Idle...";
        }
        async Task MovePlayer(MarkedLabel label)
        {
            player!.label.Background = Brushes.ForestGreen;
            player = label;
            player.label.Background = Brushes.Turquoise;
            await Task.Delay(100);
        }
        List<MarkedLabel> Neighbors(MarkedLabel target)
        {
            List<MarkedLabel> tor = new List<MarkedLabel>();
            foreach (int[] neighbors in neighbors)
            {
                int i = target.Row + neighbors[0];
                int j = target.Column + neighbors[1];

                if (i < 0 || j < 0 || i >= gamesize || j >= gamesize)
                    continue;
                tor.Add(labels[i, j]);
            }
            return tor;
        }
        void Reset()
        {
            foreach(MarkedLabel l in labels)
            {
                l.Value = 0;
                l.label.Background = Brushes.ForestGreen;
                l.wasMarked = false;
            }
        }

        

        private void Reset_click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                return;
            }
            Reset();
        }
    }
    public class MarkedLabel
    {
        public int Row { get; private set; }
        public int Column { get; private set; }
        public bool wasMarked { get; set; }
        public int Value { get; set; }
        public Label label { get; private set; }
        public MarkedLabel(Label l, int row, int column)
        {
            Value = 0;
            label = l;
            this.Row = row;
            this.Column = column;
            this.wasMarked = false;
        }
    }
}
