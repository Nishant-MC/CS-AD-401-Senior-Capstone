// Including the Kinect NuiAPI
using Microsoft.Kinect;

// Toolbox includes
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.ComponentModel;


// Utility toolbox for things I don't want to repeat over and over for different classes
namespace Kinect.Toolbox
{
    public static class Tools
    {
        public static Skeleton[] GetSkeletons(this SkeletonFrame frame)
        {
            if (frame == null) 
                return null;

            var skeletons = new Skeleton[frame.SkeletonArrayLength];
            frame.CopySkeletonDataTo(skeletons);

            return skeletons;
        }
    }

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
}