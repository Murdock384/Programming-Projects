using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using WpfLab3;

namespace WpfLab3
{
    public class HorizontalPlugin : ISlideshowEffect
    {
        public string Name
        {
            get { return "Horizontal Effect"; }
        }

        public void PlaySlideshow(Image imageIn, Image imageOut, double windowWidth, double windowHeight)
        {
            Storyboard storyboard;
            Storyboard storyboard2;

            imageIn.HorizontalAlignment = HorizontalAlignment.Right;
            imageOut.HorizontalAlignment = HorizontalAlignment.Left;

            storyboard = new Storyboard();
            storyboard2 = new Storyboard();

            DoubleAnimation animation = new DoubleAnimation(0.0, windowWidth, new TimeSpan(0, 0, 0, 0, 500));
            Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.WidthProperty));
            Storyboard.SetTarget(animation, imageIn);
            DoubleAnimation animation2 = new DoubleAnimation(windowWidth, 0.0, new TimeSpan(0, 0, 0, 0, 500));
            Storyboard.SetTargetProperty(animation2, new PropertyPath(FrameworkElement.WidthProperty));
            Storyboard.SetTarget(animation2, imageOut);

            storyboard.Children.Add(animation);
            storyboard2.Children.Add(animation2);
            storyboard.Begin();
            storyboard2.Begin();
        }

        /*https://learn.microsoft.com/pl-pl/dotnet/desktop/wpf/graphics-multimedia/storyboards-overview?view=netframeworkdesktop-4.8*/
        /* https://www.codeproject.com/Articles/364529/Animation-using-Storyboards-in-WPF*/

    }
}
