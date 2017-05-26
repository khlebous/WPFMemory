using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace WPFMemory
{
    class ExpanderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 0 : 300;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class SliderConverterToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{System.Convert.ToInt32(value)} ms";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<int> list = new List<int>();
        public ObservableCollection<ImagesListViewItem> oc { get; set; } = new ObservableCollection<ImagesListViewItem>();
        int leftButtons = 8;
        List<Button> listButtons = new List<Button>();
        DispatcherTimer timer = new DispatcherTimer();
        bool isPaused = true;
        int howManyLeft = 3;
        Button firstClicked;
        Button secondClicked;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            InitButtons();
            AddElToList();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            TimerTextBlock.Text = $"time: {howManyLeft}s";
            ImagesListView.ItemsSource = oc;
        }

        public void AddElToList()
        {
            for (int i = 1; i < 9; i++)
            {
                oc.Add(new ImagesListViewItem()
                {
                    Source = $"{Environment.CurrentDirectory}/Images/{i}.jpg",
                    FileName = $"{i}.jpg",
                    CreationDate = System.IO.Directory.GetCreationTime($"Images/{i}.jpg"),
                    Header = $"name{i}"
                });

            }
        }
        public void InitButtons()
        {
            for (int i = 0; i < 8; i++)
            {
                list.Add(i);
                list.Add(i);
            }
            Random r = new Random();
            list = list.OrderBy(x => r.Next()).Select(x => x).ToList();

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    Button MyControl = new Button();
                    Style style = this.FindResource("MemoryButtonStyle") as Style;
                    MyControl.Style = style;
                    MyControl.Content = list[i * 4 + j].ToString();
                    MyControl.Name = "Button" + (i * 4 + j).ToString();

                    Grid.SetColumn(MyControl, j);
                    Grid.SetRow(MyControl, i);
                    listButtons.Add(MyControl);
                    MemoryButtonsGrid.Children.Add(MyControl);
                }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            InitButtons();
        }

        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            howManyLeft -= 1;
            TimerTextBlock.Text = $"time: {howManyLeft}s";
            if (howManyLeft < 1)
            {
                timer.Stop();
                showMessageBox("Lost!");
            }
        }

        public void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused)
            {
                isPaused = false;
                timer.IsEnabled = true;

                timer.Start();
                PlayPauseButton.Content = "Pause";
            }
            else
            {
                isPaused = true;
                timer.Stop();
                PlayPauseButton.Content = "Play";
            }
        }

        public void GameButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused)
                return;

            if (firstClicked == null)
            {
                firstClicked = sender as Button;
                firstClicked.Background = Brushes.Red;
            }
            else
            {
                secondClicked = sender as Button;
                if (firstClicked.Content.ToString() == secondClicked.Content.ToString())
                {
                    firstClicked.Visibility = Visibility.Hidden;
                    secondClicked.Visibility = Visibility.Hidden;
                    leftButtons--;
                    if (leftButtons == 0)
                        showMessageBox("Win!");
                }
                else
                {
                    firstClicked.Background = Brushes.AliceBlue;
                }
                firstClicked = null;
            }
        }

        void showMessageBox(string s)
        {
            MessageBoxResult result = MessageBox.Show($"{s} again?", "game over", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                this.Close();
            }
            else
                InitButtons();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
                foreach (var item in oc)
                {
                    item.AmIExpanded = true;
                }
        }
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
                foreach (var item in oc)
                {
                    item.AmIExpanded = false;
                }
        }
        
    }
}
