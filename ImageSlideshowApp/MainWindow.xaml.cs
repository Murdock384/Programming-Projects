using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace WpfLab3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> AvailableSlideshowEffects { get; set; }
        public string SelectedSlideshowEffect { get; set; }
        string folderPath = " ";


        public MainWindow()
        {
            InitializeComponent();
            


            DataContext = this; 

           
            AvailableSlideshowEffects = new ObservableCollection<string>
            {
                "Horizontal Effect",
                "Opacity Effect",
                "Vertical Effect"
            };
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadDrivers();
        }

        private string selectedFolderPath;
        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                selectedFolderPath = dialog.SelectedPath;
                string[] imageFiles = Directory.GetFiles(selectedFolderPath, "*.jpg");
                ListV.ItemsSource = imageFiles;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SlideshowEffectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = (System.Windows.Controls.MenuItem)sender;
            string selectedEffect = menuItem.Header.ToString();
          
            
            StartSlideshow(selectedEffect);
        }

        private void StartSlideshowButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedEffect = EffectComboBox.SelectedItem as string;
            if (selectedEffect != null)
            {
                StartSlideshow(selectedEffect);
            }
        }


        private void StartSlideshow(string effect)
        {
            
            Slideshow slideshowWindow = new Slideshow(effect);

            
            slideshowWindow.InitializeSlideshow(selectedFolderPath);

           
            slideshowWindow.Loaded += (sender, e) =>
            {
                ApplyTransitionEffects(slideshowWindow);
            };

           
            slideshowWindow.ShowDialog();


        }

        private void ApplyTransitionEffects(Slideshow slideshowWindow)
        {
          
            int currentIndex = slideshowWindow.CurrentIndex;

         
            switch (slideshowWindow.SelectedEffect)
            {
                case "Horizontal Effect":
                    slideshowWindow.ApplyHorizontalEffect(currentIndex);
                    break;
                case "Vertical Effect":
                    slideshowWindow.ApplyVerticalEffect(currentIndex);
                    break;
                case "Opacity Effect":
                    slideshowWindow.ApplyOpacityEffect(currentIndex);
                    break;
                default:
                    
                    break;
            }
        }






        private void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Simple Image Explorer Application", "About");
        }

        private void LoadDrivers()
        {
            FolderTreeView.Items.Clear();

            string[] partitions = Directory.GetLogicalDrives();

            foreach (string partition in partitions)
            {
                TreeViewItem partitionItem = new TreeViewItem();
                partitionItem.Header = partition;
                partitionItem.Tag = partition;
                partitionItem.Expanded += Drivers_Expanded;
                FolderTreeView.Items.Add(partitionItem);

            }
        }

        private void Drivers_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem partitionItem = (TreeViewItem)sender;

            if (partitionItem.Items.Count == 0)
            {
                partitionItem.Items.Clear();

                string partitionPath = (string)partitionItem.Tag;
                string[] directories = Directory.GetDirectories(partitionPath);

                foreach (string directory in directories)
                {
                    TreeViewItem directoryItem = new TreeViewItem();
                    directoryItem.Header = new DirectoryInfo(directory).Name;
                    directoryItem.Tag = directory;
                    directoryItem.Expanded += Directory_Expanded;
                    partitionItem.Items.Add(directoryItem);
                }
            }
        }

        private void Directory_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem directoryItem = (TreeViewItem)sender;

            if (directoryItem.Items.Count == 0)
            {
                directoryItem.Items.Clear();
                string directoryPath = (string)directoryItem.Tag;
                string[] directories = Directory.GetDirectories(directoryPath);
                string[] imageFiles = Directory.GetFiles(directoryPath, "*.jpg");

                foreach (string directory in directories)
                {
                    TreeViewItem subDirectoryItem = new TreeViewItem();
                    subDirectoryItem.Header = new DirectoryInfo(directory).Name;
                    subDirectoryItem.Tag = directory;
                    subDirectoryItem.Expanded += Directory_Expanded;
                    directoryItem.Items.Add(subDirectoryItem);
                }

                
                selectedFolderPath = directoryPath;

               
                ListV.ItemsSource = imageFiles;
            }
        }
        private void ImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListV.SelectedItem != null)
            {
                string imagePath = (string)ListV.SelectedItem;
                string fileName = System.IO.Path.GetFileName(imagePath);
                FileTextBlock.Text = $"File Name: {fileName}";

                BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));

                int width = bitmapImage.PixelWidth;
                int height = bitmapImage.PixelHeight;
                FileInfo fileInfo = new FileInfo(imagePath);
                long fileSize = fileInfo.Length / 1024;
                WidthTextBlock.Text = $"Width: {width}px";
                HeightTextBlock.Text = $"Height: {height}px";
                SizeTextBlock.Text = $"Size: {fileSize} KB";

            }
            else
            {
                FileTextBlock.Text = "No file selected";
                WidthTextBlock.Text = string.Empty;
                HeightTextBlock.Text = string.Empty;
                SizeTextBlock.Text = string.Empty;
            }
        }
    }
    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string filePath = value as string;
            if (!string.IsNullOrEmpty(filePath))
            {
                return System.IO.Path.GetFileName(filePath);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
