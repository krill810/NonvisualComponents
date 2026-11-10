using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace NonvisualComponents
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer; private DateTime _startTime; private TimeSpan _elapsed; private bool _isRunning;

        private readonly JournalManager _journal = new JournalManager();

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_isRunning)
            {
                TimeSpan current = _elapsed + (DateTime.Now - _startTime);
                StopwatchDisplay.Text = FormatTime(current);
            }
        }

        private static string FormatTime(TimeSpan t)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _startTime = DateTime.Now;
            _timer.Start();
            StatusText.Text = "Секундомер запущен...";
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning)
            {
                MessageBox.Show("Секундомер не запущен.", "Стоп",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _isRunning = false;
            _timer.Stop();
            _elapsed += DateTime.Now - _startTime;

            string result = StopwatchDisplay.Text;
            _journal.AddEntry($"Остановка: {result}");

            MessageBox.Show($"Запись добавлена: {result}. Всего записей: {_journal.Count}",
                "Стоп", MessageBoxButton.OK, MessageBoxImage.Information);

            StatusText.Text = $"Остановлено. Записей: {_journal.Count}";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = false;
            _timer.Stop();
            _elapsed = TimeSpan.Zero;
            StopwatchDisplay.Text = "00:00:00.000";

            _journal.ClearAllEntries();

            MessageBox.Show($"Секундомер сброшен, журнал очищен. Всего записей: {_journal.Count}",
                "Сброс", MessageBoxButton.OK, MessageBoxImage.Information);

            JournalListBox.Items.Clear();
            StatusText.Text = "Сброшено. Записей: 0";
        }

        private void ShowJournalButton_Click(object sender, RoutedEventArgs e)
        {
            JournalListBox.Items.Clear();
            foreach (string entry in _journal.GetAllEntries())
                JournalListBox.Items.Add(entry);

            MessageBox.Show($"Отображено записей: {_journal.Count}",
                "Журнал", MessageBoxButton.OK, MessageBoxImage.Information);

            StatusText.Text = $"Журнал показан. Записей: {_journal.Count}";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_journal.Count == 0)
            {
                MessageBox.Show("Журнал пуст — сохранять нечего. Всего записей: 0",
                    "Сохранение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $"stopwatch_journal_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            try
            {
                _journal.SaveToFile(path);

                MessageBox.Show($"Журнал сохранён:\n{path}\nВсего записей: {_journal.Count}",
                    "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);

                StatusText.Text = $"Сохранено в файл. Записей: {_journal.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            Application.Current.Shutdown();
        }
    }
}