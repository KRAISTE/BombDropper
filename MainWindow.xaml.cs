using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BombDropper
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer bombTimer = new DispatcherTimer();
        private int droppedCount = 0;
        private int savedCount = 0;
        private double initialSecondsBetweenBombs = 1.3;
        private double initialSecondsToFall = 3.5;
        private double secondsBetweenBombs;
        private double secondsToFall;
        private Dictionary<Bomb, Storyboard> storyboards = new Dictionary<Bomb, Storyboard>();
        private DateTime lastAdjustmentTime = DateTime.MinValue;
        private const int maxDropped = 5;

        public MainWindow()
        {
            InitializeComponent();
            bombTimer.Tick += bombTimer_Tick;
        }

        private void cmdStart_Click(object sender, RoutedEventArgs e)
        {
            droppedCount = 0;
            savedCount = 0;
            secondsBetweenBombs = initialSecondsBetweenBombs;
            secondsToFall = initialSecondsToFall;
            bombTimer.Interval = TimeSpan.FromSeconds(secondsBetweenBombs);
            bombTimer.Start();
            cmdStart.IsEnabled = false;
        }

        private void canvasBackground_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var clip = new RectangleGeometry();
            clip.Rect = new Rect(0, 0, canvasBackground.ActualWidth, canvasBackground.ActualHeight);
            canvasBackground.Clip = clip;
        }

        private void bombTimer_Tick(object sender, EventArgs e)
        {
            var bomb = new Bomb();
            bomb.IsFalling = true;
            Random random = new Random();
            Canvas.SetLeft(bomb, random.Next(0, (int)(canvasBackground.ActualWidth - 50)));
            Canvas.SetTop(bomb, -100);

            // Обработка клика по бомбе
            bomb.MouseLeftButtonDown += (s, args) =>
            {
                if (bomb.IsFalling)
                {
                    bomb.IsFalling = false;
                    var storyboard = storyboards[bomb];
                    storyboard.Stop();

                    // Анимация выбрасывания бомбы за экран
                    DoubleAnimation rise = new DoubleAnimation
                    {
                        To = 0,
                        Duration = TimeSpan.FromSeconds(2)
                    };
                    Storyboard.SetTarget(rise, bomb);
                    Storyboard.SetTargetProperty(rise, new PropertyPath("(Canvas.Top)"));

                    DoubleAnimation slide = new DoubleAnimation
                    {
                        To = Canvas.GetLeft(bomb) < canvasBackground.ActualWidth / 2 ? -100 : canvasBackground.ActualWidth + 100,
                        Duration = TimeSpan.FromSeconds(1)
                    };
                    Storyboard.SetTarget(slide, bomb);
                    Storyboard.SetTargetProperty(slide, new PropertyPath("(Canvas.Left)"));

                    Storyboard exitStoryboard = new Storyboard();
                    exitStoryboard.Children.Add(rise);
                    exitStoryboard.Children.Add(slide);

                    // 🔴 Добавьте обработчик события Completed для exitStoryboard
                    exitStoryboard.Completed += storyboard_Completed;

                    exitStoryboard.Completed += (s2, e2) =>
                    {
                        canvasBackground.Children.Remove(bomb);
                        storyboards.Remove(bomb);
                    };
                    exitStoryboard.Begin();
                }
            };

            canvasBackground.Children.Add(bomb);

            // Анимация падения
            Storyboard fallStoryboard = new Storyboard(); // Новое имя

            DoubleAnimation fallAnimation = new DoubleAnimation
            {
                To = canvasBackground.ActualHeight,
                Duration = TimeSpan.FromSeconds(secondsToFall)
            };
            Storyboard.SetTarget(fallAnimation, bomb);
            Storyboard.SetTargetProperty(fallAnimation, new PropertyPath("(Canvas.Top)"));
            fallStoryboard.Children.Add(fallAnimation);

            // Добавьте обработчик завершения
            fallStoryboard.Completed += storyboard_Completed;
            fallStoryboard.Begin();

            storyboards.Add(bomb, fallStoryboard);

            // Усложнение игры
            if ((DateTime.Now - lastAdjustmentTime).TotalSeconds > 15)
            {
                secondsBetweenBombs -= 0.1;
                secondsToFall -= 0.1;
                bombTimer.Interval = TimeSpan.FromSeconds(secondsBetweenBombs);
                lastAdjustmentTime = DateTime.Now;
                lblRate.Text = $"Бомбы падают каждые {secondsBetweenBombs:F1} сек.";
                lblSpeed.Text = $"Время падения: {secondsToFall:F1} сек.";
            }
        }

        private void storyboard_Completed(object sender, EventArgs e)
        {
            var clockGroup = (ClockGroup)sender;
            var completedAnimation = (DoubleAnimation)clockGroup.Children[0].Timeline;
            var completedBomb = (Bomb)Storyboard.GetTarget(completedAnimation);

            if (completedBomb.IsFalling)
                droppedCount++;
            else
                savedCount++;

            lblStatus.Text = $"Поймано: {savedCount}, Упущено: {droppedCount}";

            if (droppedCount >= maxDropped)
            {
                bombTimer.Stop();
                lblStatus.Text += "\n\nИгра окончена!";
                cmdStart.IsEnabled = true;
            }
        }
    }
}