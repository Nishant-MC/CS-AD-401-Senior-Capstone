/*
 * "Gesture Recognition With The Microsoft Kinect"
 * -> Senior Capstone Project [Computer Science]
 * 
 * By: Nishant Mohanchandra
 * Project Supervisor: Jay Chen
*/

// ##### BITS AND BOBS #####
// kinectDisplay.DataContext = colorManager;
// ##### BITS AND BOBS #####

// Including the Kinect NuiAPI
using Microsoft.Kinect;

// Critical System includes
using System;
using System.Windows;
using System.Windows.Controls;

// RGB System includes
using System.ComponentModel;
using System.Linq.Expressions;

// ColorStreamManager includes
using System.Windows.Media.Imaging;
using System.Windows.Media;


/*

 ### Extra includes for later if needed ###
 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;

 ### Extra includes for later if needed ###
 
*/

namespace KinectGestureRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    // Main Window class (where the magic happens)
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        KinectSensor kinectSensor;
        Kinect.Toolbox.ColorStreamManager colorManager = new Kinect.Toolbox.ColorStreamManager();
        

        // Initializing a color stream manager {RGB input}
        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                    return;
                colorManager.Update(frame);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Listen to any status change for my Kinect(s)
                KinectSensor.KinectSensors.StatusChanged += Kinects_StatusChanged;

                // In case there is more than 1 Kinect, loop through all of them
                foreach (KinectSensor kinect in KinectSensor.KinectSensors)
                {
                    if (kinect.Status == KinectStatus.Connected)
                    {
                        kinectSensor = kinect;
                        break;
                    }
                }

                if (KinectSensor.KinectSensors.Count == 0)
                {
                    MessageBox.Show("No Kinect found, sorry!");
                }

                else
                {
                    Initialize(); // Initialize the current Kinect sensor!
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (kinectSensor == null)
                    {
                        kinectSensor = e.Sensor;
                        Initialize();
                    }
                    break;

                case KinectStatus.Disconnected:
                    if (kinectSensor == null)
                    {
                        Clean();
                        MessageBox.Show("The Kinect was dis-kinect-ed! (Geddit? :P)");
                    }
                    break;

                case KinectStatus.NotReady:
                    break;

                case KinectStatus.NotPowered:
                    if (kinectSensor == null)
                    {
                        Clean();
                        MessageBox.Show("The Kinect needs MORE POWER!");
                    }
                    break;

                default:
                    MessageBox.Show("Dude I don't know what's wrong. Unhandled Status: " + e.Status);
                    break;
            }

        }

        // Constructor
        private void Initialize()
        {
            if (kinectSensor == null)
                return;
            kinectSensor.Start();
        }

        // Destructor
        private void Clean()
        {
            if (kinectSensor != null)
            {
                kinectSensor.Stop();
                kinectSensor = null;
            }
        }

    }
}


// Utility toolbox for things I don't want to repeat over and over for different classes
namespace Kinect.Toolbox
{
    // Notifier class (for handling when properties change)
    public abstract class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null) return;

            string propertyName = memberExpression.Member.Name;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Color Stream Manager class (for handling RGB input)
    public class ColorStreamManager : Notifier
    {
        public WriteableBitmap Bitmap { get; private set; }
        int[] yuvTemp; // For 16b YUV format

        static double Clamp(double value)
        {
            return Math.Max(0, Math.Min(value, 255));
        }

        // YUV-RGB conversion function
        static int ConvertFromYUV(byte y, byte u, byte v)
        {
            byte b = (byte)Clamp(1.164 * (y - 16) + 2.018 * (u - 128));
            byte g = (byte)Clamp(1.164 * (y - 16) - 0.813 * (v - 128) - 0.391 * (u - 128));
            byte r = (byte)Clamp(1.164 * (y - 16) + 1.596 * (v - 128));

            return (r << 16) + (g << 8) + (b);
        }

        public void Update(ColorImageFrame frame)
        {
            var pixelData = new byte[frame.PixelDataLength];
            frame.CopyPixelDataTo(pixelData);

            if (Bitmap == null) // This bitmap is BGR with 96 DPI
                Bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);

            int stride = Bitmap.PixelWidth * Bitmap.Format.BitsPerPixel / 8;
            Int32Rect dirtyRect = new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight);

            if (frame.Format == ColorImageFormat.RawYuvResolution640x480Fps15)
            {
                if (yuvTemp == null)
                    yuvTemp = new int[frame.Width * frame.Height];

                int current = 0;
                for (int uyvyIndex = 0; uyvyIndex < pixelData.Length; uyvyIndex += 4)
                {
                    byte u = pixelData[uyvyIndex];
                    byte y1 = pixelData[uyvyIndex + 1];
                    byte v = pixelData[uyvyIndex + 2];
                    byte y2 = pixelData[uyvyIndex + 3];

                    yuvTemp[current++] = ConvertFromYUV(y1, u, v);
                    yuvTemp[current++] = ConvertFromYUV(y2, u, v);
                }

                Bitmap.WritePixels(dirtyRect, yuvTemp, stride, 0);
            }


            else
            {
                Bitmap.WritePixels(dirtyRect, pixelData, stride, 0);
            }

            RaisePropertyChanged(() => Bitmap);
        }
    }

}


