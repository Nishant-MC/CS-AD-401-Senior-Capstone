using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

/*

        // Color stream manager {RGB / YUV input}
        var colorManager = new ColorStreamManager();
        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                    return;

                colorManager.Update(frame);
            }
        }
 
        // Initializing a depth stream manager {Intensity mapping}
        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            var depthManager = new DepthStreamManager();

            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;

                depthManager.Update(frame);
            }
        }

        // FIX THE SKELLY TRACKER
        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame frame = e.OpenSkeletonFrame();
            if (frame == null)
                return;

            Skeleton[] skeletons = frame.GetSkeletons();

            //if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
            return;
        } 
 
*/


namespace KinectGestureRecognition
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

    }
}
