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

namespace Maze_PathFinder_Lee
{
    /// <summary>
    /// Interaction logic for ResizeMazeWindow.xaml
    /// </summary>
    public partial class ResizeMazeWindow : Window
    {
        int[] values;
        int nval;
        int mval;
        bool nok = true;
        bool mok = true;
        public ResizeMazeWindow(int[] values)
        {
            InitializeComponent();
            this.values = values;
            Loaded += ResizeMazeWindow_Loaded;
        }

        private void ResizeMazeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NBox.Text = values[0].ToString();
            MBox.Text = values[1].ToString();
        }

        private void NBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(int.TryParse(NBox.Text, out int value))
            {
                if(value >= 6 && value <= 100)
                {
                    if ((value & 1) == 0)
                    {
                        value += 1;
                    }
                    nval = value;
                    nok = true;
                }
            }
            else
            {
                nok = false;
            }
            isValid();
        }

        private void MBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(MBox.Text, out int value))
            {
                if (value >= 6 && value <= 100)
                {
                    if((value & 1) == 0)
                    {
                        value += 1;
                    }
                    mval = value;
                    mok = true;
                }
            }
            else
            {
                mok = false;
            }
            isValid();
        }
        void isValid()
        {
            if (nok && mok)
            {
                SaveButton.IsEnabled = true;
            }
            else SaveButton.IsEnabled = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            values[0] = nval;
            values[1] = mval;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
