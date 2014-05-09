/*
 * "Gesture Recognition With The Microsoft Kinect"
 * -> Senior Capstone Project [Computer Science]
 * 
 * By: Nishant Mohanchandra
 * Project Supervisor: Jay Chen
*/

/*

 * "Body Language" Presentation Ideas

>>> Introduction to the Kinect - What is it, How does it work, what can it do, what does it have
>>> Introduction to the problem landscape - programming environment (C#, VS2013 with WPF, Kinect SDK)
>>> What's out there (lit review / approaches to gesture recognition problems)

>>> How to parameterize a pointing gesture? (mathematical definition)
--- Go deeper: how is the input tracked? (skeleton data, rgb, depth)
-------------- what is pointing in this context?

>>> Face Tracking and Finger Tracking: Agreement implies a pointing gesture? What about disagreement?
>>> A word on object recognition (Its hard: you need better hardware, DirectX 11, GPU, etc.)
>>> Code Demo

>>> The Leap Motion - what it means for the project
    (Can we get a Leap & Kinect to talk to each other, minority report style?
     Short answer: Not in WPF C#, but maybe with WCF C++ or the Processing/Arduino libraries)

>>> The Kinect 2 - What it means for the project

>>> Shoutout to Jay (for the Kinect / Mentorship / etc.) 
    * and the MS Kinect team (for good ref material & sample code)
    * and David Catuhe (for a great reference book & sample code)

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

        // Initializing the Kinect Sensor
        KinectSensor kinectSensor;


        // -- COLOR MANAGER --

        Kinect.Toolbox.ColorStreamManager colorManager = new Kinect.Toolbox.ColorStreamManager(); // VAR LATER

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

        // -- COLOR MANAGER --


        // -- DEPTH MANAGER --

        Kinect.Toolbox.DepthStreamManager depthManager = new Kinect.Toolbox.DepthStreamManager(); // VAR LATER

        // Initializing a depth stream manager {Intensity mapping}
        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;

                depthManager.Update(frame);
            }
        }

        // -- DEPTH MANAGER --



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

    // Color Stream Manager class (for handling RGB + YUV input)
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

    // Depth Stream Manager class (for handling RGB + YUV input)
    public class DepthStreamManager : Notifier
    {
        byte[] depthFrame32;

        public WriteableBitmap Bitmap { get; private set; }

        public void Update(DepthImageFrame frame)
        {
            var pixelData = new short[frame.PixelDataLength];
            frame.CopyPixelDataTo(pixelData);

            if (depthFrame32 == null)
                depthFrame32 = new byte[frame.Width * frame.Height * 4];
            
            if (Bitmap == null)
                Bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgra32, null);

            ConvertDepthFrame(pixelData);

            int stride = Bitmap.PixelWidth * Bitmap.Format.BitsPerPixel / 8;
            Int32Rect dirtyRect = new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight);

            Bitmap.WritePixels(dirtyRect, depthFrame32, stride, 0);

            RaisePropertyChanged(() => Bitmap);
        }

        void ConvertDepthFrame(short[] depthFrame16)
        {
            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 ++, i32 += 4)
            {
                int user = depthFrame16[i16] & 0x07;        // 111 bitmask
                int realDepth = (depthFrame16[i16] >> 3);   // Bitshift; 3x to the right

                byte intensity = (byte)(255 - (255 * realDepth / 0x1fff)); // 255 = Close, 0 = Far

                depthFrame32[i32] = 0;
                depthFrame32[i32 + 1] = 0;
                depthFrame32[i32 + 2] = 0;
                depthFrame32[i32 + 3] = 255;

                switch (user)
                { 
                    case 0: // No one is nearby
                        depthFrame32[i32] = (byte)(intensity / 2);
                        depthFrame32[i32 + 1] = (byte)(intensity / 2);
                        depthFrame32[i32 + 2] = (byte)(intensity / 2);
                        break;

                    case 1: 
                        depthFrame32[i32] = intensity;
                        break;

                    case 2:
                        depthFrame32[i32 + 1] = intensity;
                        break;

                    case 3:
                        depthFrame32[i32 + 2] = intensity;
                        break;

                    case 4:
                        depthFrame32[i32] = intensity;
                        depthFrame32[i32 + 1] = intensity;
                        break;

                    case 5:
                        depthFrame32[i32] = intensity;
                        depthFrame32[i32 + 2] = intensity;
                        break;

                    case 6:
                        depthFrame32[i32 + 1] = intensity;
                        depthFrame32[i32 + 2] = intensity;
                        break;

                    case 7:
                        depthFrame32[i32] = intensity;
                        depthFrame32[i32 + 1] = intensity;
                        depthFrame32[i32 + 2] = intensity;
                        break;
                }
            }
        }
    }
}