using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AcademyWpApp
{
	public static class ThemeManager
	{
		public static void ApplyTheme(Window window)
		{
			try
			{
				// Открываем ключ реестра, где хранится настройка темы для приложений
				using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
				{
					if(key != null)// Проверяем, что ключ существует
					{
						object registryValueObject = key.GetValue("AppsUseLightTheme");				// Получаем значение параметра "AppsUseLightTheme"
						if (registryValueObject != null)
						{
							int registryValue = (int)registryValueObject;
							if (registryValue == 0)													// Задаём тёмный фон окна
							{
								window.Background = new SolidColorBrush(Color.FromRgb(32, 32, 32)); // Очень тёмный серый цвет
								window.Foreground = Brushes.White;                                  // Текст делаем белым для контраста
							}
							else                                                                    // Значение 1 — значит выбрана СВЕТЛАЯ тема
							{
                                window.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)); // Белый цвет
								window.Foreground = Brushes.Black;
							}
                        }
					}
				}
			}
			catch 
			{
				window.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
				window.Foreground = Brushes.Black;
			}
		}
	}
}
