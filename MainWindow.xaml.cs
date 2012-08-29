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
        public int total_time;
        public TextBox textBox1;
        public TextBox textBox2;
        public TextBox textBox3;
        public Button confirmButton;
        public Window dynamicPanelReady;
        public SoundPlayer player;
        public SoundPlayer player1 ;

        public MainWindow()
        {
            InitializeComponent();
            
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

            DisplayText(hours,minutes,seconds);


        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();

        }
        private void btnPlay_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {

            if (!timer1.IsEnabled)
            {


                btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_pause.png"));
                btnEdit.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_edit2_disabled.png"));
                try
                {
                    total_time = seconds + minutes * 60 + hours * 3600;
                    progressBar1.Minimum = 1;
                    progressBar1.Maximum = total_time;

                     timer1.Start();

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
                btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9.png"));
 
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((minutes == 0) && (seconds == 0) && (hours == 0))
            {
                // If the time is over, clear all settings and fields.
                // Also, show the message, notifying that the time is over.
                timer1.Stop();

                dynamicPanelReady = new Window();
                dynamicPanelReady.Background = new SolidColorBrush(Colors.Transparent);
                dynamicPanelReady.Width = 540;

                dynamicPanelReady.Height = 125;
                dynamicPanelReady.WindowStartupLocation = WindowStartupLocation.CenterScreen;
               
                dynamicPanelReady.AllowsTransparency = true;
                dynamicPanelReady.WindowStyle = WindowStyle.None;


                Button readyButton = new Button();
                readyButton.Width = 529;
                readyButton.Height = 105;
                readyButton.Style = (Style)FindResource("MEButtonStyle_2");
                readyButton.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/completed1.png", UriKind.RelativeOrAbsolute));
                readyButton.Click += new RoutedEventHandler(readyButton_Click);
                dynamicPanelReady.Content = readyButton;
                dynamicPanelReady.Show();


               
                Assembly assembly = Assembly.GetExecutingAssembly();
                player = new SoundPlayer(assembly.GetManifestResourceStream("MassEffectTimer.upload_completed_wav.wav"));
                player.PlaySync();
                player = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("MassEffectTimer.biosnd_end001.wav"));
                player.PlaySync();
                player = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("MassEffectTimer.biosnd_end001.wav"));
                player.PlayLooping();
   


                
            }
            else
            {
                // Else continue counting.
                if (seconds < 1)
                {
                    seconds = 59;
                    if (minutes == 0)
                    {
                        minutes = 59;
                        if (hours != 0)
                        {
                            hours -= 1;
                        }

                    }
                    else
                    {
                        minutes -= 1;
                    }
                }
                else
                {
                    seconds -= 1;
                }


 
                progressBar1.Value += 1;
                DisplayText(hours, minutes, seconds);
 
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

                Canvas.SetLeft(textBox1,53.00);
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

        public void DisplayText(int hours,int minutes, int seconds)
        {
            String textToDisplay = "";
            if (print_hours)
            {
                if (hours < 10)
                {
                    textToDisplay += "0";
                }
                textToDisplay += hours.ToString()+":";
                if (minutes < 10)
                {
                    textToDisplay += "0";
                }
            }
            textToDisplay += minutes.ToString()+":";
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
                geometry = formattedText.BuildGeometry(new System.Windows.Point(180,10));
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
            btnPlay.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_play9.png"));

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
            btnEdit.Tag = new BitmapImage(new Uri(@"pack://application:,,,/img/button_edit2.png"));
            player.Stop();
            

        }      
    }
}
