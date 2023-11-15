using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfLab3
{
    public partial class Slideshow : Window
    {
        private DispatcherTimer slideshowTimer;
        private bool isSlideshowPaused = false;
        public string SelectedEffect { get; set; }
        public List<Image> Images { get; set; }
        public int CurrentIndex { get; set; }

        public Slideshow(string effect)
        {
            InitializeComponent();
            SelectedEffect = effect;
            Images = new List<Image>();
            CurrentIndex = 0;
        }

        public void InitializeSlideshow(string fp)
        {
            
            if (string.IsNullOrEmpty(fp) || !Directory.Exists(fp))
            {
                MessageBox.Show("Invalid directory path.");
                return;
            }

            
            string[] imageFiles = Directory.GetFiles(fp, "*.jpg");

            
            foreach (string imagePath in imageFiles)
            {
                BitmapImage imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.UriSource = new Uri(imagePath, UriKind.Absolute);
                imageSource.EndInit();

                Image image = new Image();
                image.Source = imageSource;
                image.Tag = Images.Count;
                image.Stretch = Stretch.Fill;

                Images.Add(image);
                canvas.Children.Add(image);
            }

   
            if (Images.Count > 0)
            {
                slideshowImage.Source = Images[0].Source;
                CurrentIndex = -1;

            }
                

            slideshowTimer = new DispatcherTimer();
            slideshowTimer.Interval = TimeSpan.FromSeconds(3);

            slideshowTimer.Tick += (sender, e) =>
            {
                CurrentIndex = (CurrentIndex + 1) % Images.Count;
                if (CurrentIndex == 0)
                {
                  
                    slideshowImage.Source = Images[Images.Count - 1].Source;
                }
                else
                {
                    slideshowImage.Source = Images[CurrentIndex - 1].Source;
                }

                slideshowImage.Source = Images[CurrentIndex].Source;
                switch (SelectedEffect)
                {
                    case "Horizontal Effect":
                        ApplyHorizontalEffect(CurrentIndex);
                        break;
                    case "Vertical Effect":
                        ApplyVerticalEffect(CurrentIndex);
                        break;
                    case "Opacity Effect":
                        ApplyOpacityEffect(CurrentIndex);
                        break;
                    default:

                        break;
                }
            };

            slideshowTimer.Start();
        }

        public void ApplyHorizontalEffect(int currentIndex)
        {
            double canvasWidth = canvas.ActualWidth;
            double initialX = canvasWidth;
            double finalX = 0;
            try
            {
                foreach (Image image in canvas.Children.OfType<Image>())
                {
                    if (image.Tag != null && int.TryParse(image.Tag.ToString(), out int index))
                    {
                        if (index == currentIndex)
                        {
                           
                            DoubleAnimation horizontalAnimation = new DoubleAnimation();
                            horizontalAnimation.From = initialX;
                            horizontalAnimation.To = finalX;
                            horizontalAnimation.Duration = new Duration(TimeSpan.FromSeconds(3));

                            image.BeginAnimation(Canvas.LeftProperty, horizontalAnimation);
                        }
                        else if (index > currentIndex)
                        {
                           
                            Canvas.SetLeft(image, initialX);
                        }
                        else
                        {
                            
                            Canvas.SetLeft(image, finalX);
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

           
        }


        public void ApplyVerticalEffect(int currentIndex)
        {
            double canvasHeight = canvas.ActualHeight;
            double initialY = canvasHeight;
            double finalY = 0;

            foreach (Image image in canvas.Children.OfType<Image>())
            {
                if (image.Tag != null && int.TryParse(image.Tag.ToString(), out int index))
                {
                    if (index == currentIndex)
                    {
                       
                        DoubleAnimation verticalAnimation = new DoubleAnimation();
                        verticalAnimation.From = initialY;
                        verticalAnimation.To = finalY;
                        verticalAnimation.Duration = new Duration(TimeSpan.FromSeconds(3));

                        image.BeginAnimation(Canvas.TopProperty, verticalAnimation);
                    }
                    else if (index > currentIndex)
                    {
                       
                        Canvas.SetTop(image, initialY);
                    }
                    else
                    {
                      
                        Canvas.SetTop(image, finalY);
                    }
                }
            }
        }

        public void ApplyOpacityEffect(int currentIndex)
        {
            foreach (Image image in canvas.Children.OfType<Image>())
            {
                if (image.Tag != null && int.TryParse(image.Tag.ToString(), out int index))
                {
                    if (index == currentIndex)
                    {
                       
                        DoubleAnimation opacityAnimation = new DoubleAnimation();
                        opacityAnimation.From = 0;
                        opacityAnimation.To = 1;
                        opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));

                        image.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                    }
                    else
                    {
                        
                        image.Opacity = 0;
                    }
                }
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
           
            if (isSlideshowPaused)
            {
                
                ResumeSlideshow();
            }
            else
            {
                
                PauseSlideshow();
            }
        }

        private void PauseSlideshow()
        {
           
            slideshowTimer.Stop();
            isSlideshowPaused = true;
        }

        private void ResumeSlideshow()
        {
           
            slideshowTimer.Start();
            isSlideshowPaused = false;
        }


      


        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EffectButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SelectedEffect = button.Content.ToString();
        }
    }
}
