using System;
using System.Collections.Generic;
using System.IO;

namespace NonvisualComponents
{
    public class JournalManager
    {
        private readonly List<string> _entries = new List<string>();
        public int Count => _entries.Count;

            public void AddEntry(string entry)
            {
                if (string.IsNullOrWhiteSpace(entry))
                    return;

                string stamped = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] {entry}";
                _entries.Add(stamped);
            }

            public List<string> GetAllEntries()
            {
                return new List<string>(_entries);
            }

            public void ClearAllEntries()
            {
                _entries.Clear();
            }

            public void SaveToFile(string path)
            {
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

            public List<string> LoadFromFile(string path)
            {
                var result = new List<string>();
                if (!File.Exists(path))
                    return result;

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

