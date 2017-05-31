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
using System.Windows.Media.Animation;
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
    class DoubleToDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Duration duration = new Duration(TimeSpan.FromMilliseconds((double)value));
            return duration;
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
        static int GameTimeInSeconds = 5;
        static int MaxLeftButtons = 8;
        bool IsListShuffled = false;

        List<int> ListWithInt; // 1..9
        List<Button> ListWithButttons;
        public ObservableCollection<ImagesListViewItem> oc { get; set; } =
            new ObservableCollection<ImagesListViewItem>(); // iamges

        //to initial in new game
        int leftButtons = MaxLeftButtons;
        int HowManySecondsLeft = GameTimeInSeconds;
        bool IsGamePaused = true;

        DispatcherTimer timer = new DispatcherTimer();

        Button firstClicked;
        Button secondClicked;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            LoadImagesToObservableCollection();
            InitBoardForNewGame();
            ImagesListView.ItemsSource = oc;
            InitTimer();

            TimerTextBlock.Text = $"time: {HowManySecondsLeft}s";
        }

        public void LoadImagesToObservableCollection()
        {
            for (int i = 1; i < 9; i++)
                oc.Add(new ImagesListViewItem()
                {
                    Source = $"{Environment.CurrentDirectory}/Images/{i}.jpg",
                    FileName = $"{i}.jpg",
                    CreationDate = System.IO.Directory.GetCreationTime($"Images/{i}.jpg"),
                    Header = $"name{i}"
                });
        }
        public void InitBoardForNewGame()
        {
            if (ListWithButttons != null)
                foreach (Button b in ListWithButttons)
                {
                    MemoryButtonsGrid.Children.Remove(b);
                }

            timer.Stop();
            leftButtons = MaxLeftButtons;
            HowManySecondsLeft = GameTimeInSeconds;
            PlayPauseButton.IsEnabled = true;
            IsGamePaused = true;
            PlayPauseButton.Content = "Play";

            ListWithInt = new List<int>();
            ListWithButttons = new List<Button>();

            for (int i = 0; i < 8; i++)
            {
                ListWithInt.Add(i);
                ListWithInt.Add(i);
            }

            Random r = new Random();
            if (IsListShuffled)
                ListWithInt = ListWithInt.OrderBy(x => r.Next()).Select(x => x).ToList();

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    Button MyControl = new Button();
                    MyControl.Style = this.FindResource("MemoryButtonStyle") as Style;
                    MyControl.Content = oc[ListWithInt[i * 4 + j]].Source;
                    MyControl.Name = "Button" + (i * 4 + j).ToString();
                    Grid.SetColumn(MyControl, j);
                    Grid.SetRow(MyControl, i);

                    ListWithButttons.Add(MyControl);
                    MemoryButtonsGrid.Children.Add(MyControl);
                }
        }
        public void InitTimer()
        {
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitBoardForNewGame();
        }
        public void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsGamePaused)
            {
                PlayPauseButton.Content = "Pause";
                timer.Start();

                IsGamePaused = false;
                timer.IsEnabled = true;
            }
            else
            {
                PlayPauseButton.Content = "Play";
                timer.Stop();

                IsGamePaused = true;
            }
        }
        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            HowManySecondsLeft -= 1;
            TimerTextBlock.Text = $"time: {HowManySecondsLeft}s";
            if (HowManySecondsLeft < 1)
            {
                showMessageBox("Lost!");
            }
        }
        public void GameButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsGamePaused)
                return;
            Image clickedImage1 = new Image();
            Image clickedImage2;
            if (firstClicked == null)
            {
                firstClicked = sender as Button;
                clickedImage1 = firstClicked.Template.FindName("ImageInButton", firstClicked) as Image;
                Storyboard sb = (this.FindResource("PlayAnimationShowPicture") as Storyboard).Clone();
                Storyboard.SetTarget(sb, clickedImage1);
                sb.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds((double)FlipBackSlider.Value));
                sb.Begin();
            }
            else if (secondClicked == null)
            {
                secondClicked = sender as Button;
                if (firstClicked == secondClicked) // ten sam button
                    return;

                clickedImage2 = secondClicked.Template.FindName("ImageInButton", secondClicked) as Image;
                Storyboard sb = (this.FindResource("PlayAnimationShowPicture") as Storyboard).Clone();
                Storyboard.SetTarget(sb, clickedImage2);
                sb.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds((double)FlipBackSlider.Value));
                sb.Completed += (ss, ee) =>
                {
                    // chowanie
                    Storyboard sb1 = new Storyboard();
                    Storyboard sb2 = new Storyboard();
                    if (firstClicked.Content.ToString() != secondClicked.Content.ToString())
                    {
                        clickedImage1 = firstClicked.Template.FindName("ImageInButton", firstClicked) as Image;
                        sb1 = (this.FindResource("PlayAnimationHidePicture") as Storyboard).Clone();
                        Storyboard.SetTarget(sb1, clickedImage1);
                        sb2 = (this.FindResource("PlayAnimationHidePicture") as Storyboard).Clone();
                        Storyboard.SetTarget(sb2, clickedImage2);
                    }
                    else
                    {
                        sb1 = (this.FindResource("PlayAnimationHideButton") as Storyboard).Clone();
                        Storyboard.SetTarget(sb1, firstClicked);
                        sb2 = (this.FindResource("PlayAnimationHideButton") as Storyboard).Clone();
                        Storyboard.SetTarget(sb2, secondClicked);
                    }
                    sb2.Completed += (sss, eee) =>
                      {
                          // jesli to samo usuwamy
                          if (firstClicked.Content.ToString() == secondClicked.Content.ToString())
                          {
                              firstClicked.Opacity = 0;
                              secondClicked.Opacity = 0;
                              firstClicked.IsEnabled = false;
                              secondClicked.IsEnabled = false;
                              leftButtons--;
                              if (leftButtons == 0)
                              {
                                  showMessageBox("Win!");
                              }
                          }
                          firstClicked = null;
                          secondClicked = null;
                      };
                    sb1.Begin();
                    sb2.Begin();
                };
                sb.Begin();
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var ocItem in oc)
                ocItem.AmIExpanded = true;
        }
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var ocItem in oc)
                ocItem.AmIExpanded = false;
        }
        void showMessageBox(string s)
        {
            timer.Stop();
            IsGamePaused = true;
            PlayPauseButton.Content = "Play";

            MessageBoxResult result = MessageBox.Show($"{s} again?", "game over", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
                EndAnimation();
            else
            {
                InitBoardForNewGame();
                PlayPauseButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
        void EndAnimation()
        {
            // lose
            foreach (Button b in ListWithButttons)
                b.Opacity = 0;

            ResetButton.IsEnabled = false;
            PlayPauseButton.IsEnabled = false;

            EndAnimationRec(0, 1500);
        }
        void EndAnimationRec(int i, double duration)
        {
            if (i == 16)
            {
                ResetButton.IsEnabled = true;
                return;
            }
            else
            {
                Button b = ListWithButttons[i];
                Storyboard sb1 = (this.FindResource("EndAnimaion") as Storyboard).Clone();
                Storyboard.SetTarget(sb1, b);
                sb1.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds(duration));

                Image buttonImage = b.Template.FindName("ImageInButton", b) as Image;
                Storyboard sb2 = (this.FindResource("EndAnimaion") as Storyboard).Clone();
                Storyboard.SetTarget(sb2, buttonImage);
                sb2.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds(duration));

                sb1.Completed += (ss, ee) =>
                {
                    EndAnimationRec(i + 1, duration * 0.9);
                };
                sb1.Begin();
                sb2.Begin();
            }
        }

    }
}
