﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using System.Windows.Threading;

namespace LibraryDeweyDecimalApp.MVVM.View
{
    /// <summary>
    /// Interaction logic for ReplacingBooksView.xaml
    /// </summary>
    public partial class ReplacingBooksView : UserControl
    {
        private List<string> callNumbers;
        private List<string> correctOrder;

        private bool isDragging = false;
        private string itemBeingDragged = null;

        // Define the timer
        private DispatcherTimer timer;
        private TimeSpan elapsedTime;

        public ReplacingBooksView()
        {
            InitializeComponent();
            callNumbers = new List<string>();
            correctOrder = new List<string>();

            // Enable drag-and-drop
            CallNumbersListBox.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            CallNumbersListBox.MouseMove += ListBox_MouseMove;
            CallNumbersListBox.Drop += ListBox_Drop;

            // Initialize the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            elapsedTime = TimeSpan.Zero;
        }

        private void InitializeTimer()
        {
            // Initialize the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            elapsedTime = TimeSpan.Zero;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateCallNumbers();
            CheckButton.Visibility= Visibility.Visible;

            // Initialize the timer
            InitializeTimer();
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime = elapsedTime.Add(TimeSpan.FromMilliseconds(100)); // Update elapsed time
            TimerLabel.Content = $"{elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}.{elapsedTime.Milliseconds:D3}";
        }


        private void GenerateCallNumbers()
        {
            // Clear previous data
            callNumbers.Clear();
            correctOrder.Clear();
            CallNumbersListBox.Items.Clear();

            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                string topic = $"{random.Next(100, 999):D3}";
                string author = GenerateRandomAuthorName(random);
                string callNumber = $"{topic} {author}";
                callNumbers.Add(callNumber);
                correctOrder.Add(callNumber);
            }

            // Sort the correct order
            BubbleSort(correctOrder);

            // Display the sorted call numbers
            foreach (string callNumber in correctOrder)
            {
                CallNumbersListBox.Items.Add(callNumber);
            }

            // Shuffle the call numbers for user interaction
            ShuffleCallNumbers();
        }

        private string GenerateRandomAuthorName(Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, 3)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private void BubbleSort(List<string> list)
        {
            int n = list.Count;
            bool swapped;
            do
            {
                swapped = false;
                for (int i = 1; i < n; i++)
                {
                    if (string.Compare(list[i - 1], list[i]) > 0)
                    {
                        // Swap elements if they are in the wrong order
                        string temp = list[i - 1];
                        list[i - 1] = list[i];
                        list[i] = temp;
                        swapped = true;
                    }
                }
            } while (swapped);
        }


        private void ShuffleCallNumbers()
        {
            Random random = new Random();
            int n = callNumbers.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                string value = callNumbers[k];
                callNumbers[k] = callNumbers[n];
                callNumbers[n] = value;
            }

            // Display shuffled call numbers
            CallNumbersListBox.Items.Clear();
            foreach (string callNumber in callNumbers)
            {
                CallNumbersListBox.Items.Add(callNumber);
            }
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the user's order matches the correct order
            bool isCorrect = callNumbers.SequenceEqual(correctOrder);

            // Provide feedback based on correctness (customize as needed)
            if (isCorrect)
            {
                MessageBox.Show("Congratulations! You sorted the call numbers correctly.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                timer.Stop();
                SaveTime();
                GenerateCallNumbers(); // Generate a new set of call numbers
            }
            else
            {
                MessageBox.Show("Oops! Your sorting is incorrect. Try again.", "Incorrect", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveTime()
        {
            // Write the time to a text file
            string elapsedTimeString = TimerLabel.Content.ToString();
            string filePath = "ReplacingBooksTimes.txt"; // Adjust the file path as needed
            File.AppendAllLines(filePath, new[] { elapsedTimeString });
        }

            private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;

            if (listBox != null)
            {
                itemBeingDragged = GetItemAtMousePosition(listBox, e.GetPosition(listBox));

                if (!string.IsNullOrEmpty(itemBeingDragged))
                {
                    isDragging = true;
                    DragDrop.DoDragDrop(listBox, itemBeingDragged, DragDropEffects.Move);
                }
            }
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                isDragging = false;
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            string droppedItem = e.Data.GetData(DataFormats.Text) as string;

            if (!string.IsNullOrEmpty(droppedItem))
            {
                int dropIndex = GetIndexOfItem(listBox, e.GetPosition(listBox));

                if (dropIndex >= 0)
                {
                    callNumbers.Remove(droppedItem);
                    callNumbers.Insert(dropIndex, droppedItem);
                    RefreshListBox();
                }
            }
        }

        private string GetItemAtMousePosition(ListBox listBox, Point mousePosition)
        {
            HitTestResult hitTest = VisualTreeHelper.HitTest(listBox, mousePosition);
            if (hitTest != null && hitTest.VisualHit is TextBlock)
            {
                ListBoxItem listBoxItem = FindParent<ListBoxItem>(hitTest.VisualHit);
                if (listBoxItem != null)
                {
                    return listBoxItem.Content.ToString();
                }
            }
            return null;
        }

        private int GetIndexOfItem(ListBox listBox, Point mousePosition)
        {
            int index = -1;
            HitTestResult hitTest = VisualTreeHelper.HitTest(listBox, mousePosition);
            if (hitTest != null && hitTest.VisualHit is TextBlock)
            {
                ListBoxItem listBoxItem = FindParent<ListBoxItem>(hitTest.VisualHit);
                if (listBoxItem != null)
                {
                    index = listBox.Items.IndexOf(listBoxItem.Content.ToString());
                }
            }
            return index;
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                {
                    return parent;
                }
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        private void RefreshListBox()
        {
            CallNumbersListBox.Items.Clear();
            foreach (string callNumber in callNumbers)
            {
                CallNumbersListBox.Items.Add(callNumber);
            }
        }
    }
}
