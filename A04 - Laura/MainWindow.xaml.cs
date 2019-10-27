using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace A04_Threads_and_Tasks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static List<Thread> threadList = new List<Thread>();
        private static List<Line> lineList = new List<Line>();
        private static List<Thread> allThreadList = new List<Thread>();
        private static List<Line> allLineList = new List<Line>();
        volatile bool linesMove;

        volatile bool runState;     //to check if the program is not paused

        public MainWindow()
        {
            InitializeComponent();

        }


        void BtnStart_LineDraw(object o, RoutedEventArgs e)
        {
            linesMove = true;
            runState = true;
            lineList = new List<Line>();
            threadList = new List<Thread>();
            Random rnd = new Random();
            Brush brush = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256)));


            // New Lines can only be added if the program is not paused
            if (runState)
            {
                for(int i = 0; i < 40; i++)
                {
                    Line newLine;
                    if (lineList.Count == 0)
                    {

                        newLine = new Line
                        {
                            Stroke = brush,
                            StrokeThickness = 2,

                            X1 = rnd.Next(50, (int)FunCanvas.ActualWidth - 50),
                            Y1 = rnd.Next(10, (int)FunCanvas.ActualHeight),
                            X2 = rnd.Next(200, (int)FunCanvas.ActualWidth - 100),
                            Y2 = rnd.Next(20, (int)FunCanvas.ActualHeight)

                        };
                    }
                    else
                    {
                        newLine = new Line
                        {
                            Stroke = brush,
                            StrokeThickness = 2,
                            X1 = lineList[i-1].X1+5,
                            Y1 = lineList[i - 1].Y1,
                            X2 = lineList[i - 1].X2,
                            Y2 = lineList[i - 1].Y2 + 5

                        };
                    }

                    lineList.Add(newLine);      // LineList keeps track of all Lines added to the Canvas
                    allLineList.Add(newLine);
                    FunCanvas.Children.Add(lineList[i]);

                    Thread move = new Thread(new ParameterizedThreadStart(LineMover));
                    threadList.Add(move);       // ThreadList keeps track of all the started threads
                    allThreadList.Add(move);
                    move.Start(lineList[i]);
                }

            }

        }

        void BtnStop_JoinThreads(object o, RoutedEventArgs e)
        {
            linesMove = false;
            for (int i = 0; i < allLineList.Count; i++)
            {
                FunCanvas.Children.Remove(allLineList[i]);
            }

        }

        void BtnPause_PauseLines(object o, RoutedEventArgs e)
        {
            runState = false;
        }

        void BtnResume_ResumeLines(object o, RoutedEventArgs e)
        {

            if (!runState)
            {
                foreach (Thread t in allThreadList)
                {
                    t.Interrupt();
                }
            }
            runState = true;
        }

        public void LineMover(object l)
        {

            Line newLine = (Line) l;
            bool hasReachWidth = false;
            bool hasReachHeight = false;
            while (linesMove)
            {
                if (!runState)
                {
                    try
                    {
                        Thread.Sleep(Timeout.Infinite);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        // handling this exception is still WIP
                    }
                }
                else
                {
                    //to update the UI from the other thread then we must need Dispatcher.
                    //only Dispatcher can update the objects in the UI from non-UI thread.
                    this.Dispatcher.Invoke(() =>
                        {
                            if(hasReachWidth)
                            {
                                newLine.X1 -= 1;
                            }
                            else
                            {
                                newLine.X1 += 1;
                            }

                            if (newLine.X1 >= myWindow.ActualWidth - 20)
                            {
                                hasReachWidth = true;
                            }
                            else if(newLine.X1 == 0)
                            {
                                hasReachWidth = false;
                            }

                            if (hasReachHeight)
                            {
                                newLine.Y2 -= 1;
                            }
                            else
                            {
                                newLine.Y2 += 1;
                            }

                            if (newLine.Y2 >= myWindow.ActualHeight - 60)
                            {
                                hasReachHeight = true;
                            }
                            else if (newLine.Y2 == 0)
                            {
                                hasReachHeight = false;
                            }

                        });
                    Thread.Sleep(13);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            linesMove = false;
            for (int i = 0; i < allLineList.Count; i++)
            {
                FunCanvas.Children.Remove(allLineList[i]);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
