using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace bdmanager
{
    public class Localization
    {
        private Dictionary<string, string> _localeStrings;

        public event EventHandler LanguageChanged;
        public static readonly string[] AvailableLanguages = { "ru", "en", "tr" };
        public string CurrentLanguage { get; private set; } = "ru";

        public Localization()
        {
            LoadLanguage(Program.settings.Language);
        }

        public string GetString(string key)
        {
            if (_localeStrings.TryGetValue(key, out string value))
            {
                return value;
            }
            return key;
        }

        public string GetLanguageName(string languageCode)
        {
            return GetString($"language.{languageCode}");
        }

        private void LoadLanguage(string languageCode)
        {
            if (!AvailableLanguages.Contains(languageCode))
            {
                languageCode = "ru";
            }

            CurrentLanguage = languageCode;
            LoadLanguageFile(languageCode);
        }

        public void ChangeLanguage(string languageCode)
        {
            if (CurrentLanguage != languageCode)
            {
                LoadLanguage(languageCode);
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void LoadLanguageFile(string languageCode)
        {
            try
            {
                string resourceName = $"bdmanager.locale.{languageCode}.json";
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string json = reader.ReadToEnd();
                            _localeStrings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Файл локализации {resourceName} не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки языка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
