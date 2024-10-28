using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
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
using System.Windows.Media.Imaging;
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
        private CustomEventArgs.OverlayEventArgs Oae;
        private string videoPath;
        private List<RecordingOverlayBase> overlays;
        private int selectedBitRate;
        private List<Screen> Screens;

        //private List<System.Windows.Controls.Image> MonitorButtons;

        public void CreateRecording(int screenNum, CustomEventArgs.ScreenAreaEventArgs screenAreaEventArgs)
        {
            
        }

        public MainWindow()
        {
            this.selectedBitRate = 32000*1000;
            this.overlayFilterSelected = "PNG Images|*.png";
            MicEnabled = false;

            List<RecordableDisplay> AllDisplayes = new List<RecordableDisplay>();
            OutputDimensions a = Recorder.GetOutputDimensionsForRecordingSources(AllDisplayes);
            Screens= new List<Screen>();
            
            videoPath=Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            foreach (var screen in Screen.AllScreens.OrderBy(i => i.Bounds.X).ThenBy(i=> i.Bounds.Y))
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
                    SourceRect = new ScreenRect(Screens[0].Bounds.X, Screens[0].Bounds.Height, Screens[0].Bounds.Width, Screens[0].Bounds.Height),

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
                    IsMouseClicksDetected = ShowMouse,
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
                    Bitrate = this.selectedBitRate,
                    Framerate = 60,
                    IsFixedFramerate = false,
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
                    IsMp4FastStartEnabled = true
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
            _rec.Stop();

            //Recorder.GetSystemVideoCaptureDevices()[0]..GetDeviceRemovedReason()
            //Console.WriteLine()
        }
        private int AddX = 0;
        private int AddY = 0;
        private async void RecBtn_Click(object sender, RoutedEventArgs e)
        {
             AddX = 0;
            AddY = 0;
            if (!Recording)
            {
                //Change the icons to reflect recording state
                this.RecImg.Source = new BitmapImage(
               new Uri("pack://application:,,,/Assets/video-camera-Active.png"));

                Recording = true;
                AddX = 0;
                AddY = 0;


                bool preSelectNegative = false;
                for (int i = 1; i <= selected + 1; i++)
                {
                    if (Screens[i - 1].Bounds.X < 0)
                    {
                        preSelectNegative = true;

                    }
                    if (preSelectNegative)
                        AddX += Math.Abs(Screens[i - 1].Bounds.X);
                }

                preSelectNegative = false;
                for (int i = 1; i <= selected + 1; i++)
                {
                    if (Screens[i - 1].Bounds.Y < 0)
                    {
                        preSelectNegative = true;
                    }
                    if (preSelectNegative)
                        AddY += Math.Abs(Screens[i - 1].Bounds.Y);
                }

                List<AudioDevice> outputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices);
                AudioDevice selectedOutputDevice = outputDevices.First();//select one of the devices.. Passing empty string or null uses system default playback device.


                var AudioOptions = new AudioOptions
                {
                    IsAudioEnabled = true,
                    IsOutputDeviceEnabled = true,
                    IsInputDeviceEnabled = MicEnabled,
                    AudioOutputDevice = null
                };

                string selectedInputDevice = null;
                setOverlay();

                if (MicEnabled)
                {
                    List<AudioDevice> inputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices);
                    this.MicImg.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Assets/microphone_recording.png"));
                    
                    selectedInputDevice = null;//select one of the devices.. Passing empty string or null uses system default recording device.
                    AudioOptions.AudioInputDevice = selectedInputDevice;
                    AudioOptions.IsInputDeviceEnabled = true;
                    //AudioOptions.AudioOutputDevice = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices).IndexOf()
                    //var waveIn = new NAudio.Wave.WaveInEvent
                    //{
                    //    DeviceNumber = 0, // customize this to select your microphone device
                    //    WaveFormat = new NAudio.Wave.WaveFormat(rate: 1000, bits: 16, channels: 1),
                    //    BufferMilliseconds = 10
                    //};
                    //waveIn.DataAvailable += WaveIn_DataAvailable; ;

                    //waveIn.StartRecording();
                }


                RecOptions.AudioOptions = AudioOptions;
                RecOptions.OutputOptions.SourceRect = new ScreenRect(Screens[Sae.ScreenNum].Bounds.Left + Sae.Left + AddX, Screens[Sae.ScreenNum].Bounds.Top + Sae.Top + AddY, Sae.Width, Sae.Height);
                RecOptions.OutputOptions.OutputFrameSize = new ScreenSize(Sae.Width, Sae.Height);
                RecOptions.VideoEncoderOptions.Bitrate = this.selectedBitRate;
                RecOptions.OverlayOptions = new OverLayOptions();
                RecOptions.OverlayOptions.Overlays = overlays;
                //RecOptions.MouseOptions.IsMouseClicksDetected = ShowMouse;
                _rec = Recorder.CreateRecorder(RecOptions);
                _rec.OnRecordingFailed += Rec_OnRecordingFailed;


                _rec.Record($"{videoPath}\\{DateTime.Now.ToString("MMddyyyyhhmmsstt")}.mp4");
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
                _rec.Dispose();
            }
            
        }

        private void setOverlay()
        {
            //Here is a list of all supported overlay types and their properties.
            //Overlays have an offset property that is the overlay position relative to the anchor point.
            overlays = new List<RecordingOverlayBase>();
            //overlays.Add(new VideoCaptureOverlay
            //{
            //    AnchorPoint = Anchor.TopLeft,
            //    Offset = new ScreenSize(100, 100),
            //    Size = new ScreenSize(0, 250),
            //    DeviceName = @"\\?\my_camera_device_name" //or null for system default camera
            //});
            //overlays.Add(new VideoOverlay
            //{
            //    AnchorPoint = Anchor.TopRight,
            //    SourcePath = @"C:\\Users\\wyatt\\Videos\\The Witcher 3\\The Witcher 3 2022.08.29 - 21.19.48.05.DVR.mp4",
            //    Offset = new ScreenSize(50, 50),
            //    Size = new ScreenSize(0, 200)
            //});
            if (this.overlayPath != null && this.Oae !=null)
            {
                if (this.overlayPath.EndsWith("mp4"))
                {
                    overlays.Add(new VideoOverlay()
                    {
                        AnchorPoint = Anchor.TopLeft,
                        SourcePath = overlayPath,
                        Offset = new ScreenSize(Screens[Sae.ScreenNum].Bounds.Left + Oae.Left + AddX, Screens[Sae.ScreenNum].Bounds.Top + Oae.Top + AddY),
                        Size = new ScreenSize(Oae.Width, Oae.Height),
                        Stretch = StretchMode.Uniform
                    });
                }
                else if(this.overlayPath.EndsWith("gif") || this.overlayPath.EndsWith("png"))
                {
                    overlays.Add(new ImageOverlay()
                    {
                        AnchorPoint = Anchor.TopLeft,
                        SourcePath = overlayPath,
                        Offset = new ScreenSize(Screens[Sae.ScreenNum].Bounds.Left + Oae.Left + AddX, Screens[Sae.ScreenNum].Bounds.Top + Oae.Top + AddY),
                        Size = new ScreenSize(Oae.Width, Oae.Height),
                        Stretch = StretchMode.Uniform
                    });
                } else if(this.overlayPath == "Default Webcam")
                {
                    var sources = new List<RecordingSourceBase>();
                    //To get a list of recordable cameras and other video inputs on the system, you can use the static Recorder.GetSystemVideoCaptureDevices() function.
                    var allRecordableCameras = Recorder.GetSystemVideoCaptureDevices();
                    //sources.Add(allRecordableCameras.FirstOrDefault());
                    
                    overlays.Add(new VideoCaptureOverlay
                {
                    AnchorPoint = Anchor.TopLeft,
                    Offset = new ScreenSize(Screens[Sae.ScreenNum].Bounds.Left + Oae.Left + AddX, Screens[Sae.ScreenNum].Bounds.Top + Oae.Top + AddY),
                    Size = new ScreenSize(Oae.Width, Oae.Height),
                    DeviceName = allRecordableCameras.FirstOrDefault().DeviceName == null? null : allRecordableCameras.FirstOrDefault().DeviceName,
                    
                        //DeviceName =/allRecordableCameras.FirstOrDefault()/$"\\\\?\\{GetAllConnectedCameras().First().Replace(" ","_")}" //getVidDevice.Result!=null ? getVidDevice.Result:null//@"\\?\my_camera_device_name"  or null for system default camera
                    });;
                }

                
            }
            

            //overlays.Add(new DisplayOverlay()
            //{
            //    AnchorPoint = Anchor.TopLeft,
            //    Offset = new ScreenSize(400, 100),
            //    Size = new ScreenSize(300, 0)
            //});
            //overlays.Add(new WindowOverlay()
            //{
            //    AnchorPoint = Anchor.BottomRight,
            //    Offset = new ScreenSize(100, 100),
            //    Size = new ScreenSize(300, 0)
            //});
        }

        private List<string> GetAllConnectedCameras()
        {
            //https://stackoverflow.com/questions/19452757/how-can-i-get-a-list-of-camera-devices-from-my-pc-c-sharp
            var cameraNames = new List<string>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
            {
                foreach (var device in searcher.Get())
                {
                    cameraNames.Add(device["Name"].ToString());
                }
            }

            return cameraNames;
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
            //Console.WriteLine("TEST");
        }

        public void OnOverlaySelected(object source, CustomEventArgs.OverlayEventArgs e)
        {
            Oae = e;
            //Console.WriteLine("TEST");
        }

        //Needed to clear the captured bitmap screenshot used to select region of screen
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

                SelectRegion = new Popup(Screens[ScreenNum], ScreenNum, overlayPath!=null? true:false);
                SelectRegion.AreaSelected += OnAreaSelected;
                SelectRegion.OverlaySelected += OnOverlaySelected;

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

                //// Create a bitmap of the appropriate size to receive the full-screen screenshot.
                //bitmap = new Bitmap(rectangle.Width, rectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                //// Draw the screenshot into our bitmap.
                //using (Graphics g = Graphics.FromImage(bitmap))
                //{
                //    g.CopyFromScreen((int)left, (int)top, 0, 0, bitmap.Size);
                //}

                //IntPtr handle = bitmap.GetHbitmap();
                //try
                //{
                //    SelectRegion.CaptureSectionImage.Height = rectangle.Height;
                //    SelectRegion.CaptureSectionImage.Width = rectangle.Width;
                //    SelectRegion.CaptureSectionImage.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                //        BitmapSizeOptions.FromEmptyOptions());
                //    SelectRegion.CaptureSectionImage.Visibility = Visibility.Visible;
                //    SelectRegion.CaptureSectionImage.Opacity = .50;

                //}
                //catch (Exception)
                //{

                //}
                //finally
                //{
                //    DeleteObject(handle);

                //}

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

        private void SelectLow_Click(object sender, RoutedEventArgs e)
        {
            this.SelectHigh.IsChecked = false;
            this.SelectMed.IsChecked = false;
            this.SelectLow.IsChecked = true;
            this.selectedBitRate = 8000 * 1000;
        }

        private void SelectMed_Click(object sender, RoutedEventArgs e)
        {
            this.SelectHigh.IsChecked = false;
            this.SelectMed.IsChecked = true;
            this.SelectLow.IsChecked = false;
            this.selectedBitRate = 16000 * 1000;
        }

        private void SelectHigh_Click(object sender, RoutedEventArgs e)
        {
            this.SelectHigh.IsChecked = true;
            this.SelectMed.IsChecked = false;
            this.SelectLow.IsChecked = false;
            this.selectedBitRate = 32000 * 1000;
        }
        
/*        private void MenuAudioInput_Click(object sender, RoutedEventArgs e)
        {

            this.MenuAudioInput.Items.Clear();
            using (var devices = new MMDeviceEnumerator())
            {
                foreach (var device in devices.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                {
                    System.Windows.Controls.MenuItem AddDevice = new System.Windows.Controls.MenuItem();
                    AddDevice.Header = device.DeviceFriendlyName;
                    this.MenuAudioInput.Items.Add(AddDevice);
                }
            }
        }*/

      /*  private void MenuAudioOutput_Click(object sender, RoutedEventArgs e)
        {
            this.MenuAudioOutput.Items.Clear();

            System.Windows.Controls.MenuItem NoneDevice = new System.Windows.Controls.MenuItem();
            System.Windows.Controls.MenuItem DefaultDevice = new System.Windows.Controls.MenuItem();

            NoneDevice.Header = "None Or Default";
            DefaultDevice.Header = "Default";
            NoneDevice.Click += this.NoAudioOutput_Click;
            DefaultDevice.Click += this.DefaultAudioOutput_Click;
            this.MenuAudioOutput.Items.Add(NoneDevice);
            using (var devices = new MMDeviceEnumerator())
            {
                //Dictionary<string, System.Windows.Controls.MenuItem> existingMenu = new Dictionary<string, System.Windows.Controls.MenuItem>();
                foreach (var device in devices.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                { 
                    System.Windows.Controls.MenuItem AddDevice = new System.Windows.Controls.MenuItem();
                    AddDevice.Header = device.DeviceFriendlyName;
                    if (!this.MenuAudioOutput.Items.Contains(device.DeviceFriendlyName))
                        this.MenuAudioOutput.Items.Add(AddDevice);
                    else
                    {
                        this.MenuAudioOutput.Items.Remove(AddDevice);
                        this.MenuAudioOutput.Items.Add(AddDevice);
                    }
                    
                }
                
            }

        }*/
        private string overlayPath;
        private string overlayFilterSelected;
        private void OverlaySelectBtn_Click_2(object sender, RoutedEventArgs e)
        {

            //root = txtPath.Text;
            if(this.OverlaySelectBtn.IsChecked == false)
            {
                this.overlayPath = "";
            }
            else if(this.overlayPath=="" || this.overlayPath==null)
            {
                SetPath();
            }

        }

        private void SetPath()
        {
            //Opens system window to have user select a file for the overlay
            System.Windows.Forms.FileDialog openFileDlg = new System.Windows.Forms.OpenFileDialog();
            openFileDlg.Filter = this.overlayFilterSelected;
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty && result.ToString() !="Cancel")
            {
                this.overlayPath = openFileDlg.FileName;
            } else
            {
                
                this.OverlaySelectBtn.IsChecked = false;
            }
        }

        private void OverlayTypeMenu_Click(object sender, RoutedEventArgs e)
        {
            //this.overlayFilterSelected = "gif";
           
            //SetPath();
        }

        private void gifOverlaySelected_Click(object sender, RoutedEventArgs e)
        {
            this.pngOverlaySelected.IsChecked = false;
            this.camOverlaySelected.IsChecked = false;
            this.gifOverlaySelected.IsChecked = true;
            this.overlayFilterSelected = "Gifs|*.gif";
            if (this.overlayPath == null || !this.overlayPath.EndsWith("gif"))
            {
                this.overlayPath = "";
                SetPath();
            }
            
            //SetPath();
        }


        private void pngOverlaySelected_Click(object sender, RoutedEventArgs e)
        {
            //
            this.pngOverlaySelected.IsChecked = true;
            this.camOverlaySelected.IsChecked = false;
            this.gifOverlaySelected.IsChecked = false;
            this.overlayFilterSelected = "PNG Images|*.png";
            if (this.overlayPath == null || !this.overlayPath.EndsWith("png"))
            {
                this.overlayPath = "";
                SetPath();
            }
        }


        private void camOverlaySelected_Click(object sender, RoutedEventArgs e)
        {
            this.overlayPath = "Default Webcam";
            this.pngOverlaySelected.IsChecked = false;
            this.camOverlaySelected.IsChecked = true;
            this.gifOverlaySelected.IsChecked = false;
            
            //setOverlay();
        }
        private bool ShowMouse = false;
        private void showMouseTrue_Click(object sender, RoutedEventArgs e)
        {
            this.ShowMouse = true;
            this.ShowClickTrue.IsChecked = true;
            this.ShowClickFalse.IsChecked = false;   
            //SetPath();
        }

        private void showMouseFalse_Click(object sender, RoutedEventArgs e)
        {
            this.ShowMouse = false;
            this.ShowClickTrue.IsChecked = false;
            this.ShowClickFalse.IsChecked = true;
            //SetPath();
        }

    }
}
