using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Media;

namespace Music_Player_working.UserControls
{
    /// <summary>
    /// Interaction logic for HomeControl.xaml
    /// </summary>
    public partial class HomeControl : UserControl
    {
        public HomeControl()
        {
            InitializeComponent();
        }

        private Size MeasureString(string candidate, TextBlock textBlock)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);

            return new Size(formattedText.Width, formattedText.Height);
        }

        private void Canvas_Initialized(object sender, System.EventArgs e)
        {
            Canvas myCanvas = (Canvas)sender;

            TextBlock myTextBlock = (TextBlock)myCanvas.Children[0];
            Grid myGrid = (Grid)myCanvas.Parent;

            double textBlockWidth = MeasureString(myTextBlock.Text, myTextBlock).Width;

            if (textBlockWidth < 150)
            {
                return;
            }

            var fadeInOutAnimation = new DoubleAnimation()
            {
                From = myGrid.Width - textBlockWidth,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(1),
                Duration = TimeSpan.FromSeconds(1),
            };

            var fadeInOutAnimation2 = new DoubleAnimation()
            {
                From = 0,
                To = myGrid.Width - textBlockWidth,
                BeginTime = TimeSpan.FromSeconds(3),
                Duration = TimeSpan.FromSeconds(1),
            };

            BeginStoryboard beginStoryboard = new BeginStoryboard();
            var myStoryboard = new Storyboard
            {
                Name = "myStoryBoard",
                BeginTime = TimeSpan.FromSeconds(0),
                RepeatBehavior = RepeatBehavior.Forever

            };

            Storyboard.SetTargetName(fadeInOutAnimation, "SideToSideWithTrigger");
            Storyboard.SetTargetProperty(fadeInOutAnimation, new PropertyPath(Canvas.RightProperty));

            Storyboard.SetTargetName(fadeInOutAnimation2, "SideToSideWithTrigger");
            Storyboard.SetTargetProperty(fadeInOutAnimation2, new PropertyPath(Canvas.RightProperty));


            myStoryboard.Children.Add(fadeInOutAnimation);
            myStoryboard.Children.Add(fadeInOutAnimation2);

            beginStoryboard.Storyboard = myStoryboard;

            EventTrigger myMouseEnterTrigger = new EventTrigger();
            myMouseEnterTrigger.RoutedEvent = Canvas.MouseEnterEvent;
            myMouseEnterTrigger.Actions.Add(beginStoryboard);

            myCanvas.Triggers.Add(myMouseEnterTrigger);

            myCanvas.MouseEnter += MyCanvas_MouseEnter;
            myCanvas.MouseLeave += MyCanvas_MouseLeave;
        }

        private void MyCanvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Canvas myCanvas = (Canvas)sender;
            EventTrigger myMouseEnterTrigger = (EventTrigger)myCanvas.Triggers[0];
            BeginStoryboard myBeginStoryBoard = (BeginStoryboard)myMouseEnterTrigger.Actions[0];
            Storyboard myStoryBoard = (Storyboard)myBeginStoryBoard.Storyboard;

            myStoryBoard.Stop();
        }

        private void MyCanvas_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Canvas myCanvas = (Canvas)sender;
            EventTrigger myMouseEnterTrigger = (EventTrigger)myCanvas.Triggers[0];
            BeginStoryboard myBeginStoryBoard = (BeginStoryboard)myMouseEnterTrigger.Actions[0];
            Storyboard myStoryBoard = (Storyboard)myBeginStoryBoard.Storyboard;

            myStoryBoard.Begin();
        }
    }
}
