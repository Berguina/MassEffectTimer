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
using System.IO;
using System.Data;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Globalization;
using System.Resources;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.ComponentModel;




namespace MassEffectTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        public int seconds;
        public int minutes;
        public int hours;
        public bool print_hours = false;
        public DispatcherTimer timer1;

        public DateTime endtime;
        // public Thread waker;
        public Waker waker;
        public int total_time;
        public int current_rest_time;
        public TextBox textBox1;
        public TextBox textBox2;
        public TextBox textBox3;
        public Button confirmButton;
        public Window dynamicPanelReady=new Window();
        public SoundPlayer player;
        public SoundPlayer player1;
        public Window AboutWindow=new Window();
        public Window SettingsWindow=new Window();
        public bool aboutWindowCreated = false;
        public bool SettingsWindowCreated = false;
        public bool dynamicPanelReadyCreated = false;



        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
           

            

            hours = Properties.Settings.Default.SavedHours;
            minutes = Properties.Settings.Default.SavedMinutes;
            seconds = Properties.Settings.Default.SavedSeconds;

            if (hours > 0)
            {
                print_hours = true;
            }

            timer1 = new System.Windows.Threading.DispatcherTimer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = new TimeSpan(0, 0, 1);

            DisplayText(hours, minutes, seconds);
            // waker = new Thread(new ThreadStart(SetWaitForWakeUpTime));
            waker = new Waker();
            Assembly assembly = Assembly.GetExecutingAssembly();
            player = new SoundPlayer(assembly.GetManifestResourceStream("MassEffectTimer.upload_completed_wav.wav"));
            player.LoadAsync();
            player1 = new SoundPlayer(assembly.GetManifestResourceStream("MassEffectTimer.biosnd_end001.wav"));
            player1.LoadAsync();
            dynamicPanelReady.Closed += new EventHandler(Window_Closed);

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                //Window wnd = Window.GetWindow(sender.);
                this.DragMove();
                Point pos = PointToScreen(e.MouseDevice.GetPosition(null));

                Properties.Settings.Default.Left = System.Convert.ToInt32(pos.X);
                Properties.Settings.Default.Top = System.Convert.ToInt32(pos.Y);
                Properties.Settings.Default.Save();

            }

        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            int top = Properties.Settings.Default.Top;
            int left = Properties.Settings.Default.Left;

            if (top > 0 && left > 0) {
                this.Left = left;
                this.Top = top;
                
            }
        
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void btnPlay_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {


            if (!timer1.IsEnabled)
            {


                btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_pause.png"));
                btnEdit.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_edit2_disabled.png"));
                this.Settings.IsEnabled = false;
                try
                {
                    total_time = seconds + minutes * 60 + hours * 3600;
                    current_rest_time = total_time;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum =  total_time;

                    // MessageBox.Show(total_time + " - " + progressBar1.Value);
                    endtime = DateTime.Now.AddSeconds(current_rest_time);
                    // MessageBox.Show(endtime.ToLongTimeString());
                    timer1.Start();
                    waker.SetEndtime(endtime);
                    waker.Start();


                    DisplayText(hours, minutes, seconds);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {

                timer1.Stop();
                waker.Pause();
                btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9.png"));
                btnEdit.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_edit2.png"));
                this.Settings.IsEnabled = true;
                btnPlay.IsEnabled = true;

            }
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan rest = endtime - DateTime.Now;
            double rest_time = rest.TotalSeconds;
            int rest_sec = Convert.ToInt32(rest_time);
            // if ((minutes == 0) && (seconds == 0) && (hours == 0))
            if (rest_sec <= 0)
            {
                // If the time is over, clear all settings and fields.
                // Also, show the message, notifying that the time is over.
                timer1.Stop();

                hours = 0;
                minutes = 0;
                seconds = 0;
                btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9_disabled.png"));
                btnPlay.IsEnabled = false;
                btnEdit.IsEnabled = false;

                DisplayText(hours, minutes, seconds);
                progressBar1.Value = total_time;

                //dynamicPanelReady = new Window();
                if (!dynamicPanelReadyCreated)
                {
                    dynamicPanelReady.Background = new SolidColorBrush(Colors.Transparent);
                    dynamicPanelReady.Width = 540;

                    dynamicPanelReady.Height = 125;
                    dynamicPanelReady.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                    dynamicPanelReady.AllowsTransparency = true;
                    dynamicPanelReady.WindowStyle = WindowStyle.None;
                    dynamicPanelReadyCreated = true;

                    Button readyButton = new Button();
                    readyButton.Width = 529;
                    readyButton.Height = 105;
                    readyButton.Style = (Style)FindResource("MEButtonStyle_2");
                    readyButton.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/completed1.png", UriKind.RelativeOrAbsolute));
                    readyButton.Click += new RoutedEventHandler(readyButton_Click);
                    dynamicPanelReady.Content = readyButton;

                }



               
                dynamicPanelReady.Show();





                if (player.IsLoadCompleted && player1.IsLoadCompleted){
                    player.PlaySync();
                    player1.PlayLooping();
                }
 




            }
            else
            {

                int diff = current_rest_time - rest_sec;


                hours = Convert.ToInt32(Math.Floor(rest_sec / 3600.00));
                minutes = Convert.ToInt32(Math.Floor((rest_sec - hours * 3600) / 60.00));
                seconds = Convert.ToInt32(rest_sec - minutes * 60 - hours * 3600);




                progressBar1.Value += diff;
                DisplayText(hours, minutes, seconds);
                current_rest_time = Convert.ToInt32(rest_time);

            }
        }



        private void image1_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }


        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!timer1.IsEnabled)
            {

                btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9_disabled.png"));
                btnPlay.IsEnabled = false;
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/img/bg_edit.png", UriKind.RelativeOrAbsolute)); ;
                dynamicPanel.Background = brush;

                textBox1 = new TextBox();
                textBox1.Text = Properties.Settings.Default.SavedHours.ToString();
                textBox1.Width = 70;
                textBox1.Height = 35;
                textBox1.Background = new SolidColorBrush(Colors.White);
                textBox1.FontFamily = new System.Windows.Media.FontFamily(Properties.Settings.Default.FontFamilyName);
                textBox1.FontSize = 24;
                textBox1.FontStyle = System.Windows.FontStyles.Normal;

                Canvas.SetLeft(textBox1, 53.00);
                Canvas.SetTop(textBox1, 95.00);

                textBox2 = new TextBox();
                textBox2.Text = Properties.Settings.Default.SavedMinutes.ToString();
                textBox2.Width = 70;
                textBox2.Height = 35;
                textBox2.Background = new SolidColorBrush(Colors.White);
                textBox2.FontFamily = new System.Windows.Media.FontFamily(Properties.Settings.Default.FontFamilyName);
                textBox2.FontSize = 24;
                textBox2.FontStyle = System.Windows.FontStyles.Normal;
                textBox3 = new TextBox();
                textBox3.Text = Properties.Settings.Default.SavedSeconds.ToString();
                textBox3.Width = 70;
                textBox3.Height = 35;
                textBox3.Background = new SolidColorBrush(Colors.White);
                textBox3.FontFamily = new System.Windows.Media.FontFamily(Properties.Settings.Default.FontFamilyName);
                textBox3.FontSize = 24;
                textBox3.FontStyle = System.Windows.FontStyles.Normal;

                Canvas.SetLeft(textBox2, 126.00);
                Canvas.SetTop(textBox2, 95.00);
                Canvas.SetLeft(textBox3, 200.00);
                Canvas.SetTop(textBox3, 95.00);


                confirmButton = new Button();
                confirmButton.Width = 159;
                confirmButton.Height = 27;
                confirmButton.Style = (Style)FindResource("MEButtonStyle_1");

                confirmButton.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/confirm1.png", UriKind.RelativeOrAbsolute)); ;

                confirmButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                confirmButton.BorderThickness = new System.Windows.Thickness(0);
                this.confirmButton.MouseEnter += new MouseEventHandler(confirmButton_MouseEnter);
                this.confirmButton.MouseLeave += new MouseEventHandler(confirmButton_MouseLeave);
                this.confirmButton.Click += new RoutedEventHandler(confirmButton_Click);




                Canvas.SetLeft(confirmButton, 50.0);
                Canvas.SetTop(confirmButton, 178.0);


                this.dynamicPanel.Children.Add(textBox1);
                this.dynamicPanel.Children.Add(textBox2);
                this.dynamicPanel.Children.Add(textBox3);
                this.dynamicPanel.Children.Add(confirmButton);


            }

        }

        public void DisplayText(int hours, int minutes, int seconds)
        {
            String textToDisplay = "";
            if (print_hours)
            {
                if (hours < 10)
                {
                    textToDisplay += "0";
                }
                textToDisplay += hours.ToString() + ":";
                if (minutes < 10)
                {
                    textToDisplay += "0";
                }
            }
            textToDisplay += minutes.ToString() + ":";
            if (seconds < 10)
            {

                textToDisplay += "0";
            }
            textToDisplay += seconds.ToString();
            // Create a formatted text string.
            FormattedText formattedText = new FormattedText(
                textToDisplay,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(Properties.Settings.Default.FontFamilyName),
                36,
                System.Windows.Media.Brushes.Black);

            // Set the font weight to Bold for the formatted text.
            formattedText.SetFontWeight(FontWeights.Bold);

            // Build a geometry out of the formatted text.
            Geometry geometry = null;
            if (print_hours)
            {
                geometry = formattedText.BuildGeometry(new System.Windows.Point(80, 10));
            }
            else
            {
                geometry = formattedText.BuildGeometry(new System.Windows.Point(180, 10));
            }

            // Create a set of polygons by flattening the Geometry object.
            PathGeometry pathGeometry = geometry.GetFlattenedPathGeometry();

            // Supply the empty Path element in XAML with the PathGeometry in order to render the polygons.


            path.Data = pathGeometry;

        }

        private void confirmButton_MouseLeave(object sender, EventArgs e)
        {

            confirmButton.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/confirm1.png", UriKind.RelativeOrAbsolute));

        }


        private void confirmButton_MouseEnter(object sender, EventArgs e)
        {

            confirmButton.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/confirm1b.png", UriKind.RelativeOrAbsolute));

        }

        private void confirmButton_Click(object sender, EventArgs e)
        {

            Properties.Settings.Default.SavedHours = System.Convert.ToInt32(textBox1.Text);
            Properties.Settings.Default.SavedMinutes = System.Convert.ToInt32(textBox2.Text);
            Properties.Settings.Default.SavedSeconds = System.Convert.ToInt32(textBox3.Text);
            Properties.Settings.Default.Save();
            hours = System.Convert.ToInt32(textBox1.Text);
            minutes = System.Convert.ToInt32(textBox2.Text);
            seconds = System.Convert.ToInt32(textBox3.Text);
            this.dynamicPanel.Children.Remove(textBox1);
            this.dynamicPanel.Children.Remove(textBox2);
            this.dynamicPanel.Children.Remove(textBox3);
            this.dynamicPanel.Children.Remove(confirmButton);
            dynamicPanel.Background = new SolidColorBrush(Colors.Transparent);
            textBox1 = null;
            textBox2 = null;
            textBox3 = null;


            if (hours > 0)
            {
                print_hours = true;
            }
            else
            {
                print_hours = false;
            }
            DisplayText(hours, minutes, seconds);
            progressBar1.Value = 0;
            btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9.png"));
            btnPlay.IsEnabled = true;

        }

        private void readyButton_Click(object sender, EventArgs e)
        {
            dynamicPanelReady.Hide();
            hours = Properties.Settings.Default.SavedHours;
            minutes = Properties.Settings.Default.SavedMinutes;
            seconds = Properties.Settings.Default.SavedSeconds;
            if (hours > 0)
            {
                print_hours = true;
            }
            DisplayText(hours, minutes, seconds);
            total_time = seconds + minutes * 60 + hours * 3600;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = total_time;
            progressBar1.Value = 0;
            timer1.Stop();

            btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9.png"));
            btnPlay.IsEnabled = true;
            btnEdit.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_edit2.png"));
            btnEdit.IsEnabled = true;

            player.Stop();
            player1.Stop();



        }

        private void About_Click(object sender, EventArgs e)
        {

            if (SettingsWindow.IsVisible)
            {
                SettingsWindow.Hide();
                btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9.png"));
                btnPlay.IsEnabled = true;

            }
           // AboutWindow = new Window();
            if (!aboutWindowCreated)
            {
                AboutWindow.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/img/messageBox_IB2.png")));
                AboutWindow.Width = 512;

                AboutWindow.Height = 512;
                AboutWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                AboutWindow.AllowsTransparency = true;
                AboutWindow.WindowStyle = WindowStyle.None;


                Canvas panel = new Canvas();
                panel.Width = 400;
                panel.Height = 500;
                panel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                Canvas.SetTop(panel, 0.00);


                Label aboutLabel = new Label();
                aboutLabel.Content = " About ";

                aboutLabel.Width = 400;
                aboutLabel.Height = 100;
                aboutLabel.Background = new SolidColorBrush(Colors.Transparent);
                aboutLabel.Foreground = new SolidColorBrush(Colors.White);
                aboutLabel.FontFamily = new System.Windows.Media.FontFamily("Arial");
                aboutLabel.FontSize = 24;
                aboutLabel.FontStyle = System.Windows.FontStyles.Normal;
                Canvas.SetTop(aboutLabel, 85.00);

                panel.Children.Add(aboutLabel);

                TextBlock aboutBox = new TextBlock();
                aboutBox.Width = 400;
                aboutBox.Height = 400;
                aboutBox.TextWrapping = TextWrapping.WrapWithOverflow;
                aboutBox.Background = new SolidColorBrush(Colors.Transparent);
                aboutBox.Foreground = new SolidColorBrush(Colors.White);
                aboutBox.FontFamily = new System.Windows.Media.FontFamily("Arial");
                aboutBox.FontSize = 16;
                aboutBox.FontStyle = System.Windows.FontStyles.Normal;
                //aboutBox.Text = " Mass Effect Themed Tea-Timer \n developed by Anna Putrino. \n\nThe code is open source, you can download it at  https://github.com/Berguina/MassEffectTimer \n\nThe copyright of images and sounds belongs to Bioware/EA. \n\n\nI've developed this timer for fun inspiring myself to my prefered game. \nEnjoy! ";
                aboutBox.Inlines.Add(" Mass Effect Themed Tea-Timer \n developed by Anna Putrino. \n\nThe code is open source, you can download it at  ");


                Hyperlink hyperLink = new Hyperlink()
                {
                    NavigateUri = new Uri("https://github.com/Berguina/MassEffectTimer")
                };
                hyperLink.Inlines.Add("https://github.com/Berguina/MassEffectTimer");
                hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
                aboutBox.Inlines.Add(hyperLink);

                aboutBox.Inlines.Add(" \n\nThe copyright of images and sounds belongs to Bioware/EA. \n\n\nI've developed this timer for fun inspiring myself to my prefered game. \nEnjoy! ");

                Canvas.SetTop(aboutBox, 125.0);
                panel.Children.Add(aboutBox);

                Button closeButton = new Button();
                closeButton.Width = 182;
                closeButton.Height = 33;
                closeButton.Style = (Style)FindResource("MEButtonStyle_2");

                closeButton.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/close.png", UriKind.RelativeOrAbsolute)); ;

                closeButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                closeButton.BorderThickness = new System.Windows.Thickness(0);
                closeButton.MouseEnter += new MouseEventHandler(closeButton_MouseEnter);
                closeButton.MouseLeave += new MouseEventHandler(closeButton_MouseLeave);
                closeButton.Click += new RoutedEventHandler(closeButton_Click);




                Canvas.SetLeft(closeButton, 220.0);
                Canvas.SetTop(closeButton, 400.0);
                panel.Children.Add(closeButton);

                AboutWindow.Content = panel;
                aboutWindowCreated = true;
            }

            AboutWindow.Show();



        }
        private void closeButton_MouseLeave(object sender, EventArgs e)
        {
            // MessageBox.Show(sender.ToString());
            var ele = sender as ContentControl;
            ele.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/close.png", UriKind.RelativeOrAbsolute));

        }


        private void closeButton_MouseEnter(object sender, EventArgs e)
        {
            var ele = sender as ContentControl;
            ele.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/close_o.png", UriKind.RelativeOrAbsolute));

        }

        private void closeButton_Click(object sender, EventArgs e)
        {

            var ele = sender as ContentControl;
            var panel = ele.Parent as ContentControl;
            this.AboutWindow.Hide();


        }


        private void Settings_Click(object sender, EventArgs e)
        {
            if (!timer1.IsEnabled)
            {
                
                if (AboutWindow.IsVisible) {
                    AboutWindow.Hide();
                
                }
                btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9_disabled.png"));
                btnPlay.IsEnabled = false;
               // SettingsWindow = new Window();
                if (!SettingsWindowCreated)
                {
                    SettingsWindow.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/img/messageBox_IB2.png")));
                    SettingsWindow.Width = 512;

                    SettingsWindow.Height = 512;
                    SettingsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                    SettingsWindow.AllowsTransparency = true;
                    SettingsWindow.WindowStyle = WindowStyle.None;


                    Canvas panel = new Canvas();
                    panel.Width = 400;
                    panel.Height = 500;
                    panel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    Canvas.SetTop(panel, 0.00);

                    Label settingsBox = new Label();
                    settingsBox.Content = " General Settings ";

                    settingsBox.Width = 400;
                    settingsBox.Height = 100;
                    settingsBox.Background = new SolidColorBrush(Colors.Transparent);
                    settingsBox.Foreground = new SolidColorBrush(Colors.White);
                    settingsBox.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    settingsBox.FontSize = 24;
                    settingsBox.FontStyle = System.Windows.FontStyles.Normal;
                    Canvas.SetTop(settingsBox, 85.00);

                    panel.Children.Add(settingsBox);

                    TextBlock settingsExp = new TextBlock();
                    settingsExp.Width = 400;
                    settingsExp.Height = 100;
                    settingsExp.TextWrapping = TextWrapping.WrapWithOverflow;
                    settingsExp.Background = new SolidColorBrush(Colors.Transparent);
                    settingsExp.Foreground = new SolidColorBrush(Colors.White);
                    settingsExp.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    settingsExp.FontSize = 12;
                    settingsExp.FontStyle = System.Windows.FontStyles.Normal;
                    // settingsExp.Text = "  Set the font for your timer.\n  You can download Mass Effect font from the Web for a better result \n (http://sta.sh/013u2wkdr1j9).";
                    settingsExp.Inlines.Add("  Set fonts for your timer.\n  You can download Mass Effect font from the Web for a better result \n  ");
                    Hyperlink hyperLink = new Hyperlink()
                    {
                        NavigateUri = new Uri("http://sta.sh/013u2wkdr1j9")
                    };
                    hyperLink.Inlines.Add("(http://sta.sh/013u2wkdr1j9) ");
                    hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
                    settingsExp.Inlines.Add(hyperLink);
                    Canvas.SetTop(settingsExp, 135.00);

                    panel.Children.Add(settingsExp);
                    Label fontLabel = new Label();
                    fontLabel.Content = " Font ";

                    fontLabel.Width = 400;
                    fontLabel.Height = 100;
                    fontLabel.Background = new SolidColorBrush(Colors.Transparent);
                    fontLabel.Foreground = new SolidColorBrush(Colors.White);
                    fontLabel.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    fontLabel.FontSize = 16;
                    fontLabel.FontStyle = System.Windows.FontStyles.Normal;
                    Canvas.SetTop(fontLabel, 185.00);

                    panel.Children.Add(fontLabel);

                    ComboBox fontList = new ComboBox();
                    fontList.Name = "fontList";
                    fontList.Width = 200;
                    // fontList.ItemsSource = Fonts.SystemFontFamilies;
                    fontList.IsDropDownOpen = false;

                    //bool found = false;
                    IOrderedEnumerable<FontFamily> sortedFonts = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());
                    foreach (FontFamily font in sortedFonts)
                    {

                        ComboBoxItem comboBoxItem = new ComboBoxItem();
                        comboBoxItem.Content = font.ToString();
                        comboBoxItem.Tag = font;
                        fontList.Items.Add(comboBoxItem);
                        if (font.ToString() == Properties.Settings.Default.FontFamilyName)
                        {
                            fontList.SelectedItem = comboBoxItem;
                            //found = true;
                        }
                    }

                //    if (!found) {
                //        Properties.Settings.Default.FontFamilyName = "Arial";
                //        Properties.Settings.Default.Save();
                //        foreach (ComboBoxItem item in fontList.Items) {
                //          if(item.Tag.Equals(Properties.Settings.Default.FontFamilyName))
                //          {
                //               fontList.SelectedItem = item;
                //              found=true;
                //          }
                //        }
                //}

                    //fontList.SelectedIndex =fontList.Items.IndexOf(Properties.Settings.Default.FontFamilyName);
                    //fontList.SelectedIndex = fontList.Items.IndexOf(Properties.Settings.Default.FontFamilyName);
                    // MessageBox.Show(fontList.SelectedIndex.ToString());
                    Canvas.SetLeft(fontList, 100.0);
                    Canvas.SetTop(fontList, 185.00);

                    panel.Children.Add(fontList);


                    TextBlock timerExp = new TextBlock();
                    timerExp.Width = 400;
                    timerExp.Height = 100;
                    timerExp.TextWrapping = TextWrapping.WrapWithOverflow;
                    timerExp.Background = new SolidColorBrush(Colors.Transparent);
                    timerExp.Foreground = new SolidColorBrush(Colors.White);
                    timerExp.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    timerExp.FontSize = 12;
                    timerExp.FontStyle = System.Windows.FontStyles.Normal;
                    timerExp.Text = "  Set the duration of your timer.\n  You can set it also pressing the button E next to play in the interface";
                    Canvas.SetTop(timerExp, 235.00);

                    panel.Children.Add(timerExp);

                    Label timerLabel = new Label();
                    timerLabel.Content = " Timer ";

                    timerLabel.Width = 400;
                    timerLabel.Height = 100;
                    timerLabel.Background = new SolidColorBrush(Colors.Transparent);
                    timerLabel.Foreground = new SolidColorBrush(Colors.White);
                    timerLabel.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    timerLabel.FontSize = 16;
                    timerLabel.FontStyle = System.Windows.FontStyles.Normal;
                    Canvas.SetTop(timerLabel, 275.00);

                    panel.Children.Add(timerLabel);

                    TextBox textBox1 = new TextBox();
                    textBox1.Name = "sh";
                    textBox1.Text = Properties.Settings.Default.SavedHours.ToString();
                    textBox1.Width = 70;
                    textBox1.Height = 35;
                    textBox1.Background = new SolidColorBrush(Colors.LightGray);
                    textBox1.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    textBox1.FontSize = 24;
                    textBox1.FontStyle = System.Windows.FontStyles.Normal;

                    Canvas.SetLeft(textBox1, 100.00);
                    Canvas.SetTop(textBox1, 275.00);

                    TextBox textBox2 = new TextBox();
                    textBox2.Name = "sm";
                    textBox2.Text = Properties.Settings.Default.SavedMinutes.ToString();
                    textBox2.Width = 70;
                    textBox2.Height = 35;
                    textBox2.Background = new SolidColorBrush(Colors.LightGray);
                    textBox2.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    textBox2.FontSize = 24;
                    textBox2.FontStyle = System.Windows.FontStyles.Normal;
                    TextBox textBox3 = new TextBox();
                    textBox3.Name = "ss";
                    textBox3.Text = Properties.Settings.Default.SavedSeconds.ToString();
                    textBox3.Width = 70;
                    textBox3.Height = 35;
                    textBox3.Background = new SolidColorBrush(Colors.LightGray);
                    textBox3.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    textBox3.FontSize = 24;
                    textBox3.FontStyle = System.Windows.FontStyles.Normal;

                    Canvas.SetLeft(textBox2, 173.00);
                    Canvas.SetTop(textBox2, 275.00);
                    Canvas.SetLeft(textBox3, 247.00);
                    Canvas.SetTop(textBox3, 275.00);

                    panel.Children.Add(textBox1);
                    panel.Children.Add(textBox2);
                    panel.Children.Add(textBox3);

                    Button okButton = new Button();
                    okButton.Width = 186;
                    okButton.Height = 33;
                    okButton.Style = (Style)FindResource("MEButtonStyle_2");

                    okButton.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/ok1.png", UriKind.RelativeOrAbsolute)); ;

                    okButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    okButton.BorderThickness = new System.Windows.Thickness(0);
                    okButton.MouseEnter += new MouseEventHandler(okButton_MouseEnter);
                    okButton.MouseLeave += new MouseEventHandler(okButton_MouseLeave);
                    okButton.Click += new RoutedEventHandler(okButton_Click);




                    Canvas.SetLeft(okButton, 220.0);
                    Canvas.SetTop(okButton, 400.0);
                    panel.Children.Add(okButton);

                    SettingsWindow.Content = panel;
                    SettingsWindowCreated = true;

                }
                SettingsWindow.Show();
            }


        }

        private void okButton_Click(object sender, EventArgs e)
        {

            var ele = sender as ContentControl;
            var panel = ele.Parent as Panel;

            for (var i = 0; i < panel.Children.Count; i++) {
                var c=panel.Children[i];
                if (c is System.Windows.Controls.ComboBox) {

                    var fontList = c as ComboBox;
                    if (fontList.SelectedItem != null)
                    {
                        var selectedFont = fontList.SelectedItem.ToString().Substring(38);


                        if (selectedFont != null && selectedFont.ToString() != "")
                        {
                            Properties.Settings.Default.FontFamilyName = selectedFont;
                            Properties.Settings.Default.Save();
                        }
                    }

                }
                if (c is System.Windows.Controls.TextBox) {
                    var val = c as TextBox;
                    if (val.Name == "sh") {
                        Properties.Settings.Default.SavedHours = System.Convert.ToInt32(val.Text);

                    }
                    else if (val.Name == "sm") {
                        Properties.Settings.Default.SavedMinutes = System.Convert.ToInt32(val.Text);
                    }else if(val.Name=="ss"){

                        Properties.Settings.Default.SavedSeconds = System.Convert.ToInt32(val.Text);
                    }
                }
                //MessageBox.Show(panel.Children[i].GetType().ToString());
            }

            Properties.Settings.Default.Save();
            

            hours = Properties.Settings.Default.SavedHours;
            minutes = Properties.Settings.Default.SavedMinutes;
            seconds = Properties.Settings.Default.SavedSeconds;

            

            if (hours > 0)
            {
                print_hours = true;
            }
            else
            {
                print_hours = false;
            }
            DisplayText(hours, minutes, seconds);
            total_time = seconds + minutes * 60 + hours * 3600;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = total_time;
            progressBar1.Value = 0; 
            btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9.png"));
            btnPlay.IsEnabled = true;
            SettingsWindow.Hide();
            

        }
        private void okButton_MouseLeave(object sender, EventArgs e)
        {
            // MessageBox.Show(sender.ToString());
            var ele = sender as ContentControl;
            ele.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/ok1.png", UriKind.RelativeOrAbsolute));

        }


        private void okButton_MouseEnter(object sender, EventArgs e)
        {
            var ele = sender as ContentControl;
            ele.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/ok1_o.png", UriKind.RelativeOrAbsolute));

        }

        private void CloseApp(object sender, EventArgs e)
        {

            
            Application.Current.Shutdown();
            
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }


        // private void dynamicPanelReady_Closing(object sender, CancelEventArgs e)
        //{
        //    Application.Current.Shutdown();
        //}
    }
    }
