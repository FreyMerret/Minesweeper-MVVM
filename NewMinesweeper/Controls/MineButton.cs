using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NewMinesweeper.Controls
{
    /// <summary> Типы кнопок </summary>
    public enum CellType
    {
        //нажатая кнопка, показывающая количество мин вокруг
        Near0 = 0,
        Near1 = 1,
        Near2 = 2,
        Near3 = 3,
        Near4 = 4,
        Near5 = 5,
        Near6 = 6,
        Near7 = 7,
        Near8 = 8,

        //ненажатая кнопка в состояниях: просто ненажатая, с флагом, со спрятанной бомбой
        Button = 9,
        Flagged = 10,
        Mine = 11,

        //для отображения мин после конца игры при проигрыше
        PressedMine = 12,
        WasNoMine = 13
    }

    public class MineButton : ContentControl
    {
        public int X;
        public int Y;

        public CellType CurrentCellType
        {
            get => (CellType)GetValue(CurrentCellTypeProperty);
            set => SetValue(CurrentCellTypeProperty, value);
        }

        public static readonly DependencyProperty CurrentCellTypeProperty =
            DependencyProperty.Register(
                nameof(CurrentCellType),
                typeof(CellType),
                typeof(MineButton),
                new PropertyMetadata(CellType.Button,CellTypeChanged));

        private static void CellTypeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue != args.NewValue)
            {
                ((MineButton)dependencyObject).UpdateButtonImage();
            }
        }

        public MineButton(int x, int y)
        {
            DataContext = this;
            X = x;
            Y = y;
            Content = new Image { Source = new BitmapImage(new Uri($"/Images/MineButtonImages/Button.bmp", UriKind.Relative)) };
        }

        public void UpdateButtonImage()
        {
            Content = new Image { Source = new BitmapImage(new Uri($"/Images/MineButtonImages/{CurrentCellType}.bmp", UriKind.Relative)) };
        }
    }
}
