using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace RecApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private Recorder _rec;
        private bool Recording;
        private List<RecordingSourceBase> RecordingSources;
        private RecorderOptions RecOptions;
        private bool MicEnabled;
        private Bitmap bitmap;
        private List<System.Windows.Controls.Button> Monitors;
        private int selected;
        private RecApp.Popup SelectRegion;
        private CustomEventArgs.ScreenAreaEventArgs Sae;
        private string videoPath;

        private List<Screen> Screens;

        //private List<System.Windows.Controls.Image> MonitorButtons;

        public void CreateRecording(int screenNum, CustomEventArgs.ScreenAreaEventArgs screenAreaEventArgs)
        {
            
        }


        public MainWindow()
        {
            MicEnabled = false;

            List<RecordableDisplay> AllDisplayes = new List<RecordableDisplay>();
            OutputDimensions a = Recorder.GetOutputDimensionsForRecordingSources(AllDisplayes);
            Screens= new List<Screen>();
            
            videoPath=Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            foreach (var screen in Screen.AllScreens.OrderBy(i => i.Bounds.X))
            {
                Screens.Add(screen);
            }
                RecordingSources = new List<RecordingSourceBase>();
            for(int i = 0; i< Screens.Count; i++)
            {
                List<RecordableDisplay> b = new List<RecordableDisplay>();
                AllDisplayes.Add(new RecordableDisplay(i.ToString(), Screens[i].DeviceName));
               
                AllDisplayes[i].OutputSize = new ScreenSize(Screens[i].Bounds.Width, Screens[i].Bounds.Height);
                AllDisplayes[i].Position = new ScreenPoint(Screens[i].Bounds.Left, Screens[i].Bounds.Top);
                AllDisplayes[i].RecorderApi = RecorderApi.WindowsGraphicsCapture;
                RecordingSources.Add(AllDisplayes[i]);
                
                //Console.WriteLine($"Device: {AllDisplayes[i].DeviceName}  FriendName: {AllDisplayes[i].FriendlyName} {AllDisplayes[i].IsCursorCaptureEnabled} {AllDisplayes[i].IsVideoCaptureEnabled} {AllDisplayes[i].Position} {AllDisplayes[i].RecorderApi} {AllDisplayes[i].SourceRect} {AllDisplayes[i].OutputSize} ");
            }
            
           
            RecOptions = new RecorderOptions
            {

                SourceOptions = new SourceOptions
                {

                    RecordingSources = RecordingSources


                },
                OutputOptions = new OutputOptions
                {
                    
                    RecorderMode = RecorderMode.Video,

                    //This sets a custom size of the video output, in pixels.

                    OutputFrameSize = new ScreenSize(Screens[0].Bounds.Width, Screens[0].Bounds.Height),
                    //Stretch controls how the resizing is done, if the new aspect ratio differs.
                    Stretch = StretchMode.Uniform,
                    //SourceRect allows you to crop the output.
                    SourceRect = new ScreenRect(0, 0, Screens[0].Bounds.Width, Screens[0].Bounds.Height),

                },
                AudioOptions = new AudioOptions
                {
                    Bitrate = AudioBitrate.bitrate_128kbps,
                    Channels = AudioChannels.Stereo,
                    IsAudioEnabled = true,


                },
                MouseOptions = new MouseOptions
                {
                    //Displays a colored dot under the mouse cursor when the left mouse button is pressed.	
                    IsMouseClicksDetected = true,
                    MouseLeftClickDetectionColor = "#FFFF00",
                    MouseRightClickDetectionColor = "#FFFF00",
                    MouseClickDetectionRadius = 30,
                    MouseClickDetectionDuration = 100,
                    IsMousePointerEnabled = true,
                    /* Polling checks every millisecond if a mouse button is pressed.
                       Hook is more accurate, but may affect mouse performance as every mouse update must be processed.*/
                    MouseClickDetectionMode = MouseDetectionMode.Hook
                },
                LogOptions = new LogOptions
                {
                    //This enabled logging in release builds.
                    IsLogEnabled = true,
                    //If this path is configured, logs are redirected to this file.
                    LogFilePath = "recorder.log",
                    LogSeverityLevel = ScreenRecorderLib.LogLevel.Debug
                },
                VideoEncoderOptions = new VideoEncoderOptions
                {
                    Bitrate = 8000 * 1000,
                    Framerate = 60,
                    IsFixedFramerate = true,
                    //Currently supported are H264VideoEncoder and H265VideoEncoder
                    Encoder = new H264VideoEncoder
                    {
                        BitrateMode = H264BitrateControlMode.CBR,
                        EncoderProfile = H264Profile.Main,
                    },
                    //Fragmented Mp4 allows playback to start at arbitrary positions inside a video stream,
                    //instead of requiring to read the headers at the start of the stream.
                    IsFragmentedMp4Enabled = true,
                    //If throttling is disabled, out of memory exceptions may eventually crash the program,
                    //depending on encoder settings and system specifications.
                    IsThrottlingDisabled = false,
                    //Hardware encoding is enabled by default.
                    IsHardwareEncodingEnabled = true,
                    //Low latency mode provides faster encoding, but can reduce quality.
                    IsLowLatencyEnabled = false,
                    //Fast start writes the mp4 header at the beginning of the file, to facilitate streaming.
                    IsMp4FastStartEnabled = false
                }
            };
            //  < Image x: Name = "CaptureSectionImage" ></ Image >
            //</ Popup >
            //selected = 0;
            

           
            Recording = false;
            InitializeComponent();
            Monitors = new List<System.Windows.Controls.Button>();
            int screenCount = Screen.AllScreens.Length;
            if (screenCount > 0)
            {
                Monitors.Add(this.MonOnebtn);
                this.Mon1Lbl.Visibility = Visibility.Visible;
            }
            if (screenCount > 1)
            {
                Monitors.Add(this.MonTwobtn);
                this.Mon2Lbl.Visibility = Visibility.Visible;
            }
            if (screenCount > 2)
            {
                Monitors.Add(this.MonThreebtn);
                this.Mon3Lbl.Visibility = Visibility.Visible;
            }
            if (screenCount >= 3)
            {
                this.Mon4Lbl.Visibility = Visibility.Visible;
                Monitors.Add(this.MonFourbtn);
            }


        }
        private void Rec_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            //this.RecBtn.Background = new SolidColorBrush(Colors.White);
            string error = e.Error;
            
            sender.ToString();
            Console.WriteLine(e.Error);
            //Recorder.GetSystemVideoCaptureDevices()[0]..GetDeviceRemovedReason()
            //Console.WriteLine()
        }
        private async void RecBtn_Click(object sender, RoutedEventArgs e)
        {

            if (!Recording)
            {
                //Change the icons to reflect recording state
                this.RecImg.Source = new BitmapImage(
               new Uri("pack://application:,,,/Assets/video-camera-Active.png"));

                Recording = true;
                int AddX = 0;

                if(Screens[0].Bounds.X<0)
                {
                    if (selected >= 1)
                        for (int i = 1; i <=selected; i++)
                        {

                            AddX += Math.Abs(Screens[i-1].Bounds.Left);
                        }
                    else
                        AddX += Math.Abs(Screens[0].Bounds.Left);
                }
                
                List<AudioDevice> outputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices);
                AudioDevice selectedOutputDevice = outputDevices.FirstOrDefault();//select one of the devices.. Passing empty string or null uses system default playback device.
                
                   
                var AudioOptions = new AudioOptions
                {
                    IsAudioEnabled = true,
                    IsOutputDeviceEnabled = true,
                    IsInputDeviceEnabled = MicEnabled,
                    AudioOutputDevice = selectedOutputDevice.DeviceName,
                    //AudioInputDevice = selectedInputDevice
                };

                string selectedInputDevice = null;
                if (MicEnabled)
                {
                    List<AudioDevice> inputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices);
                    this.MicImg.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Assets/microphone_recording.png"));
                    selectedInputDevice = inputDevices.FirstOrDefault().DeviceName;//select one of the devices.. Passing empty string or null uses system default recording device.
                    AudioOptions.AudioInputDevice = selectedInputDevice;
                    AudioOptions.IsInputDeviceEnabled = true;
                }
                RecOptions.AudioOptions = AudioOptions;
                RecOptions.OutputOptions.SourceRect = new ScreenRect(Screens[Sae.ScreenNum].Bounds .Left+ Sae.Left+AddX, Screens[Sae.ScreenNum].Bounds.Top+Sae.Top, Sae.Width, Sae.Height);
                RecOptions.OutputOptions.OutputFrameSize = new ScreenSize(Sae.Width, Sae.Height);

                int AddY = 0;

                _rec = Recorder.CreateRecorder(RecOptions);
                _rec.OnRecordingFailed += Rec_OnRecordingFailed;

                
                _rec.Record(videoPath + "\\" + DateTime.Now.ToString("MMddyyyyhhmmsstt") + ".mp4");

            }
            else
            {
                this.RecImg.Source = new BitmapImage(
              new Uri("pack://application:,,,/Assets/video-camera.png"));
                if(MicEnabled)
                {

                    this.MicImg.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Assets/microphone-selected.png"));
                } else
                {
                    this.MicImg.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Assets/microphone.png"));
                }
                Recording = false;
                _rec.Stop();
            }
            
        }

        private void MonOnebtn_Click(object sender, RoutedEventArgs e)
        {
            setButtons(0);
            selected = 0;
            
            HandleScreen(0);
        }

        private void MonTwobtn_Click(object sender, RoutedEventArgs e)
        {
            setButtons(1);
            selected = 1;
            HandleScreen(1);
          
        }

        private void MonThreebtn_Click(object sender, RoutedEventArgs e)
        {

            setButtons(2);
            selected = 2;
            HandleScreen(selected);
            
        }

        private void MonFourbtn_Click(object sender, RoutedEventArgs e)
        {
                
                setButtons(3);
            selected = 3;
            HandleScreen(selected);
            
        }

        private void setButtons(int selected)
        {

            List<System.Windows.Controls.Image> MonitorButtons = new List<System.Windows.Controls.Image>();

            MonitorButtons.Add(this.Mon1Img);
            MonitorButtons.Add(this.Mon2Img);
            MonitorButtons.Add(this.Mon3Img);
            MonitorButtons.Add(this.Mon3Img);
            //Unselect previously selected monitor
            MonitorButtons[this.selected].Source = new BitmapImage(
                new Uri("pack://application:,,,/Assets/display.png"));
            //Select selected monitor; change image to a selected one
            MonitorButtons[selected].Source = new BitmapImage(
                new Uri("pack://application:,,,/Assets/monitor-selected.png"));

        }

        public void OnAreaSelected(object source, CustomEventArgs.ScreenAreaEventArgs e)
        {
            Sae = e;
            Console.WriteLine("TEST");
        }

        
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);


        private void HandleScreen(int ScreenNum)
        {
            //Calculates the location of the pop-up to allow the user to select a portion of the respective window
            if (SelectRegion != null)
            {

                SelectRegion.Close();
                SelectRegion = null;
            } 

                SelectRegion = new Popup(Screens[ScreenNum], ScreenNum);
                SelectRegion.AreaSelected += OnAreaSelected;

                // Calculate left/top offset regarding to primary screen (where the app runs)
                var virtualDisplay = Screens[ScreenNum].Bounds;
                var rectangle = Screens[ScreenNum].Bounds;

                // Capture screenie (rectangle is the area previously selected
                double left = rectangle.Left;
                double top = rectangle.Top;


                if (virtualDisplay.Left < rectangle.Left)
                {
                    left -= Math.Abs(virtualDisplay.Left - rectangle.Left);
                }
                if (virtualDisplay.Top < rectangle.Top)
                {
                    top -= Math.Abs(virtualDisplay.Top - rectangle.Top);
                }

                // Create a bitmap of the appropriate size to receive the full-screen screenshot.
                bitmap = new Bitmap(rectangle.Width, rectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // Draw the screenshot into our bitmap.
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen((int)left, (int)top, 0, 0, bitmap.Size);
                }

                IntPtr handle = bitmap.GetHbitmap();
                try
                {
                    SelectRegion.CaptureSectionImage.Height = rectangle.Height;
                    SelectRegion.CaptureSectionImage.Width = rectangle.Width;
                    SelectRegion.CaptureSectionImage.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    SelectRegion.CaptureSectionImage.Visibility = Visibility.Visible;
                    SelectRegion.CaptureSectionImage.Opacity = .50;

                }
                catch (Exception)
                {

                }
                finally
                {
                    DeleteObject(handle);

                }

                SelectRegion.Visibility = Visibility.Visible;
           
        }

        private void RecMicBtn_Click(object sender, RoutedEventArgs e)
        {
            if(!Recording)
            {
                if (MicEnabled)
                {
                    MicEnabled = false;
                    this.MicImg.Source = new BitmapImage(
               new Uri("pack://application:,,,/Assets/microphone.png"));
                } else
                {
                    MicEnabled = true;
                    this.MicImg.Source = new BitmapImage(
                   new Uri("pack://application:,,,/Assets/microphone-selected.png"));
                }
                    
            }
        }

        private void VidFolderSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                this.videoPath = openFileDlg.SelectedPath;
            }
            //root = txtPath.Text;

        }
    }
}
