using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf_binding
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Furniture> Furniture { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        

        public MainWindow()
        {

            Furniture = new ObservableCollection<Furniture>();
            InitializeComponent();
            Furniture.Add(new Furniture()
            {
                Name = "FEJAN",
                Code = @"404.690.19",
                NumberOfAvailableUnits = 5,
                UsersScore = 3,
                Price = 95,
                MainImage = "1.png",
                Dimensions = new Dimensions()
                {
                    Height = 71,
                    Length = 50,
                    Width = 44
                }
            });
            Furniture.Add(new Furniture()
            {
                Name = "NICKEBO",
                Code = "505.377.20",
                NumberOfAvailableUnits = 0,
                UsersScore = 5,
                Price = 350,
                MainImage = "1.png",
                Dimensions = new Dimensions()
                {
                    Width = 60,
                    Height = 80,
                    Length = 5
                }
            });
            Furniture.Add(new Furniture()
            {
                Name = "KNORRIG",
                Code = "602.604.48",
                NumberOfAvailableUnits = 100,
                UsersScore = 5,
                Price = 29.99f,
                MainImage = "Knorrig.png",
                Dimensions = new Dimensions()
            });
            Furniture.Add(new Furniture()
            {
                Name = "NICKEBO",
                Code = "505.377.20",
                NumberOfAvailableUnits = 0,
                UsersScore = 5,
                Price = 350,
                MainImage = "1.png",
                Dimensions = new Dimensions()
                {
                    Width = 60,
                    Height = 80,
                    Length = 5
                }
            });
            Furniture.Add(new Furniture()
            {
                Name = "NICKEBO",
                Code = "505.377.20",
                NumberOfAvailableUnits = 0,
                UsersScore = 5,
                Price = 350,
                MainImage = "other.png",
                Dimensions = new Dimensions()
                {
                    Width = 60,
                    Height = 80,
                    Length = 5
                }
            });
            Furniture.Add(new Furniture()
            {
                Name = "DYPSIS_LUTESCENS",
                Code = "468.040.05",
                NumberOfAvailableUnits = 12,
                UsersScore = 4,
                Price = 119,
                MainImage = "plant.png",
                Dimensions = new Dimensions()
                {
                    Width = 0,
                    Height = 24,
                    Length = 0
                }
            });
            Furniture.Add(new Furniture()
            {
                Name = "HANDSKALAD",
                Code = "904.241.46",
                NumberOfAvailableUnits = 0,
                UsersScore = 6,
                Price = 59.99f,
                MainImage = "hand.png",
                Dimensions = new Dimensions()
                {
                    Width = 9,
                    Height = 30,
                    Length = 7
                }
            });
            Furniture.Add(new Furniture()
            {
                Name = "SJALSLIGT",
                Code = "003.432.82",
                NumberOfAvailableUnits = 1202,
                UsersScore = 3,
                Price = 45.99f,
                MainImage = "cactus.png",
                Dimensions = new Dimensions()
                {
                    Width = 20,
                    Height = 10,
                    Length = 28
                }
            });
            DataContext = Furniture;
        }
    }
}
