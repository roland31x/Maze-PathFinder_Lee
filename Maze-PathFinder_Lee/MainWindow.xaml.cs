using System;
using System.Collections.Generic;
using System.IO;
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
        static readonly Brush BackgroundBrush = Brushes.GreenYellow;
        static readonly Brush PlayerBrush = Brushes.Turquoise;
        static readonly Brush PathBrush = Brushes.DarkViolet;
        static readonly Brush TargetBrush = Brushes.Red;
        static readonly Brush WallBrush = Brushes.Black;


        MarkedLabel[,] labels;
        int gamesizeH = 16;
        int gamesizeW = 16;
        bool isSelectingPlayer = false;
        bool isSelectingTarget = false;
        bool isSelectingWalls = false;
        bool isPlaying = false;
        string MapFile = @"..\..\..\Maze1.txt";


        MarkedLabel? Target;
        MarkedLabel? player;
        List<int[]> neighbors = new List<int[]>() { new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 1, 0 }, new int[] { 0, -1 } };

        
        public MainWindow()
        {
            InitializeComponent();
            Init(16,10);
            InfoBox.Content = "Idle";
            LoadMap(MapFile);
           
        }
        void LoadMap(string FileName)
        {
            StreamReader sr = new StreamReader(FileName);
            string buffer;
            List<string> lines = new List<string>();
            
            while (!sr.EndOfStream)
            {
                lines.Add(sr.ReadLine()!);                
            }
            sr.Close();
            Init(lines.Count, lines[0].Length);

            for(int i = 0; i < lines.Count; i++)
            {
                buffer = lines[i];
                for (int j = 0; j < buffer.Length; j++)
                {
                    if (buffer[j] - '0' == 1)
                    {
                        labels[i, j].label.Background = WallBrush;
                        labels[i, j].Value = -1;
                    }
                }
            }
        }
        void Init(int matsizeH, int matsizeW)
        {
            this.gamesizeH = matsizeH;
            this.gamesizeW = matsizeW;
            labels = new MarkedLabel[gamesizeH, gamesizeW];
            MainGrid.Children.Clear();
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();
            MainGrid.Background = Brushes.Black;
            for(int i = 0; i < gamesizeW; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(MainGrid.Width / gamesizeW) });
            }
            for (int i = 0; i < gamesizeH; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(MainGrid.Height / gamesizeH) });              
                for (int j = 0; j < gamesizeW; j++)
                {
                    Label L = new Label()
                    {
                        Background = BackgroundBrush,
                        //Margin = new Thickness(1, 1, 1, 1),
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
            player.label.Background = PlayerBrush;
        }
        private void LabelClick(object sender, MouseEventArgs e)
        {
            MarkedLabel clicked = ((sender as Label)!.Tag as MarkedLabel)!;
            if (isSelectingTarget)
            {
                if(clicked.Value != 0)
                {
                    return;
                }
                if(Target != null)
                {
                    Target.label.Background = BackgroundBrush;
                }                  
                Target = clicked;
                Target.label.Background = TargetBrush;
                
            }
            else if(isSelectingWalls)
            {               
                if(clicked.Value == 0)
                {
                    clicked.Value = -1;
                    clicked.label.Background = WallBrush;
                }
                else if(clicked.Value == -1)
                {
                    clicked.Value = 0;
                    clicked.label.Background = BackgroundBrush;
                }
                else
                {
                    return;
                }
            }
            else if(isSelectingPlayer)
            {
                if (clicked.Value != 0)
                {
                    return;
                }
                if (player != null)
                {
                    player.label.Background = BackgroundBrush;
                }
                player = clicked;
                player.label.Background = PlayerBrush;
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
                foreach(MarkedLabel n in Neighbors(next))
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

            //foreach (MarkedLabel label in labels)
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
            player!.label.Background = PathBrush;
            player = label;
            player.label.Background = PlayerBrush;
            await Task.Delay(50);
        }
        List<MarkedLabel> Neighbors(MarkedLabel target)
        {
            List<MarkedLabel> tor = new List<MarkedLabel>();
            foreach (int[] neighbors in neighbors)
            {
                int i = target.Row + neighbors[0];
                int j = target.Column + neighbors[1];

                if (i < 0 || j < 0 || i >= gamesizeH || j >= gamesizeW)
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
                l.label.Background = BackgroundBrush;
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
