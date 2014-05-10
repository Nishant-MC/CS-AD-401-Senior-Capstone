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

// Including our toolbox
using Kinect.Toolbox;


namespace KinectGestureRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    // Main Window class (where the magic happens... eh, hopefully)
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            // Selecting the first (and only) Kinect 
            var kinectSensor = KinectSensor.KinectSensors[0];

            // Enabling RGB, Depth & Skeleton tracking
            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.DepthStream.Enable();
            kinectSensor.SkeletonStream.Enable();
            kinectSensor.Start();
                        
            // Grabbing frame data with our event signatures (event model, we're not polling for frames)
            // kinectSensor.ColorFrameReady += kinectSensor_ColorFrameReady;
            // kinectSensor.DepthFrameReady += kinectSensor_DepthFrameReady;

      
        }


        // Initializing the Kinect Sensor
        KinectSensor kinectSensor;

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
                    MessageBox.Show("BATTLECRUISER OPERATIONAL!");
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