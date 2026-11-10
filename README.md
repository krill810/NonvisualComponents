# NonvisualComponents
Практическая работа № 7: Разработка приложения с невизуальными компонентами в WPF и C#


<Window x:Class="StopwatchJournal.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Иванов Иван — Группа ИС-21 — ПР №7: Невизуальные компоненты WPF"
        Height="560" Width="720"
        WindowStartupLocation="CenterScreen">

    <Grid Margin="15">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Информация о студенте и работе -->
        <StackPanel Grid.Row="0" Margin="0,0,0,10">
            <TextBlock Text="Иванов Иван" FontSize="20" FontWeight="Bold"/>
            <TextBlock Text="Группа: ИС-21" FontSize="14"/>
            <TextBlock Text="ПР №7: Невизуальные компоненты WPF" FontSize="14"/>
            <TextBlock Text="Демонстрация работы с невизуальными компонентами"
                       FontSize="12" FontStyle="Italic" Foreground="Gray"/>
        </StackPanel>

        <!-- Показания секундомера -->
        <Border Grid.Row="1" Background="#F0F0F0" Padding="10" Margin="0,0,0,10">
            <TextBlock x:Name="StopwatchDisplay"
                       Text="00:00:00.000"
                       FontSize="42" FontFamily="Consolas"
                       HorizontalAlignment="Center"/>
        </Border>

        <!-- Кнопки управления (6 кнопок) -->
        <WrapPanel Grid.Row="2" Orientation="Horizontal" Margin="0,0,0,10">
            <Button x:Name="StartButton" Content="Старт" Width="110" Height="35"
                    Margin="0,0,8,8" Click="StartButton_Click"/>
            <Button x:Name="StopButton" Content="Стоп" Width="110" Height="35"
                    Margin="0,0,8,8" Click="StopButton_Click"/>
            <Button x:Name="ResetButton" Content="Сброс" Width="110" Height="35"
                    Margin="0,0,8,8" Click="ResetButton_Click"/>
            <Button x:Name="ShowJournalButton" Content="Показать журнал" Width="140" Height="35"
                    Margin="0,0,8,8" Click="ShowJournalButton_Click"/>
            <Button x:Name="SaveButton" Content="Сохранить в файл" Width="140" Height="35"
                    Margin="0,0,8,8" Click="SaveButton_Click"/>
            <Button x:Name="ExitButton" Content="Выход" Width="110" Height="35"
                    Margin="0,0,8,8" Click="ExitButton_Click"/>
        </WrapPanel>

        <!-- Журнал -->
        <ListBox x:Name="JournalListBox" Grid.Row="3" Margin="0,0,0,10"
                 FontFamily="Consolas" FontSize="13"/>

        <!-- Статус -->
        <TextBlock x:Name="StatusText" Grid.Row="4" Text="Готово. Записей: 0"
                   Foreground="DarkGreen"/>
    </Grid>
</Window>




using System;
using System.Collections.Generic;
using System.IO;

namespace StopwatchJournal
{
    /// <summary>
    /// Менеджер журнала событий. Хранит записи в List<string>,
    /// умеет добавлять, отдавать, очищать и сохранять в файл.
    /// </summary>
    public class JournalManager
    {
        // Невизуальный компонент: List<string>
        private readonly List<string> _entries = new List<string>();

        /// <summary>Количество записей в журнале.</summary>
        public int Count => _entries.Count;

        /// <summary>Добавляет запись в журнал.</summary>
        public void AddEntry(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry))
                return;

            // Невизуальный компонент: DateTime — фиксируем дату/время события
            string stamped = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] {entry}";
            _entries.Add(stamped);
        }

        /// <summary>Возвращает копию всех записей.</summary>
        public List<string> GetAllEntries()
        {
            return new List<string>(_entries);
        }

        /// <summary>Очищает журнал.</summary>
        public void ClearAllEntries()
        {
            _entries.Clear();
        }

        /// <summary>Сохраняет журнал в файл (StreamWriter).</summary>
        public void SaveToFile(string path)
        {
            // Невизуальный компонент: StreamWriter
            using (var writer = new StreamWriter(path, false))
            {
                writer.WriteLine("=== Журнал событий секундомера ===");
                writer.WriteLine($"Сформирован: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                writer.WriteLine($"Всего записей: {_entries.Count}");
                writer.WriteLine(new string('-', 40));

                foreach (string entry in _entries)
                    writer.WriteLine(entry);
            }
        }

        /// <summary>Читает журнал из файла (StreamReader).</summary>
        public List<string> LoadFromFile(string path)
        {
            var result = new List<string>();
            if (!File.Exists(path))
                return result;

            // Невизуальный компонент: StreamReader
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    result.Add(line);
            }
            return result;
        }
    }
}



using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace StopwatchJournal
{
    public partial class MainWindow : Window
    {
        // Невизуальный компонент: DispatcherTimer для отсчёта времени
        private readonly DispatcherTimer _timer;
        private DateTime _startTime;
        private TimeSpan _elapsed;
        private bool _isRunning;

        private readonly JournalManager _journal = new JournalManager();

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // настройка Interval
            };
            _timer.Tick += Timer_Tick;
        }

        // ---------- Тик таймера ----------
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

        // ---------- 1. Старт ----------
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _startTime = DateTime.Now;
            _timer.Start(); // Enabled = true
            StatusText.Text = "Секундомер запущен...";
        }

        // ---------- 2. Стоп ----------
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

            // Требование №4: подтверждение с количеством записей
            MessageBox.Show($"Запись добавлена: {result}. Всего записей: {_journal.Count}",
                "Стоп", MessageBoxButton.OK, MessageBoxImage.Information);

            StatusText.Text = $"Остановлено. Записей: {_journal.Count}";
        }

        // ---------- 3. Сброс ----------
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = false;
            _timer.Stop();
            _elapsed = TimeSpan.Zero;
            StopwatchDisplay.Text = "00:00:00.000";

            _journal.ClearAllEntries();

            // Требование №4
            MessageBox.Show($"Секундомер сброшен, журнал очищен. Всего записей: {_journal.Count}",
                "Сброс", MessageBoxButton.OK, MessageBoxImage.Information);

            JournalListBox.Items.Clear();
            StatusText.Text = "Сброшено. Записей: 0";
        }

        // ---------- 4. Показать журнал ----------
        private void ShowJournalButton_Click(object sender, RoutedEventArgs e)
        {
            JournalListBox.Items.Clear();
            foreach (string entry in _journal.GetAllEntries())
                JournalListBox.Items.Add(entry);

            MessageBox.Show($"Отображено записей: {_journal.Count}",
                "Журнал", MessageBoxButton.OK, MessageBoxImage.Information);

            StatusText.Text = $"Журнал показан. Записей: {_journal.Count}";
        }

        // ---------- 5. Сохранить в файл ----------
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

                // Требование №4
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

        // ---------- 6. Выход ----------
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            Application.Current.Shutdown();
        }
    }
}
