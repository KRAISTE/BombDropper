using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace BombDropper
{
    /// <summary>
    /// Логика взаимодействия для Bomb.xaml
    /// </summary>
    public partial class Bomb : UserControl
    {
        public static readonly DependencyProperty IsFallingProperty =
            DependencyProperty.Register("IsFalling", typeof(bool), typeof(Bomb), new PropertyMetadata(false));

        public bool IsFalling
        {
            get => (bool)GetValue(IsFallingProperty);
            set => SetValue(IsFallingProperty, value);
        }

        public Bomb()
        {
            InitializeComponent();
        }
    }
}