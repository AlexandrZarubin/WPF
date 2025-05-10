using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
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
using AcademyDataCache;     //DLL Cache

namespace AcademyWpApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow:Window
	{
		Cache cache;
		DispatcherTimer reloadTimer;        // Таймер для автообновления данных
		int reloadIntervalSeconds;     // Интервал обновления (в секундах)
		private int timeLeftSeconds;        // Секунд до следующего обновления
		const byte ALL_ID = 255;
		[DllImport("kernel32.dll")]
		public static extern bool AllocConsole();

		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();
		public MainWindow()
		{
			InitializeComponent();
			AllocConsole();
			string connectionString = "Data Source=USER-PC\\SQLEXPRESS;" +
				"Initial Catalog=VPD_311_Import;" +
				"Integrated Security=True;" +
				"Connect Timeout=30;" +
				"Encrypt=False;" +
				"TrustServerCertificate=False;" +
				"ApplicationIntent=ReadWrite;" +
				"MultiSubnetFailover=False";
			cache = new Cache(connectionString);
			//ThemeManager.ApplyTheme(this);
			ReloadData();                           // Первая загрузка данных в кэш и интерфейс
			SetReloadInterval(1, 0);                // Устанавливаем интервал автообновления (1 минута)
			InitReloadTimer();                      // Запускаем таймер автообновления
		}
		void LoadData()
		{
			// Загружаем нужные таблицы из базы данных
			cache.AddTable("Directions", "direction_id,direction_name");
			cache.AddTable("Groups", "group_id,group_name,direction");
			cache.AddRelation("GroupsDirections", "Groups,direction", "Directions,direction_id");

			cache.AddTable("Students", "stud_id,last_name,first_name,middle_name,birth_date,group");
			cache.AddRelation("StudentsGroups", "Students,group", "Groups,group_id");

			AddAllRowsIfMissing();

			GroupComboBox.ItemsSource = cache.Set.Tables["Groups"].DefaultView;
			GroupComboBox.DisplayMemberPath = "group_name";
			GroupComboBox.SelectedValuePath = "group_id";

			DirectionComboBox.ItemsSource = cache.Set.Tables["Directions"].DefaultView;
			DirectionComboBox.DisplayMemberPath = "direction_name";
			DirectionComboBox.SelectedValuePath = "direction_id";

			StudentsDataGrid.ItemsSource = cache.Set.Tables["Students"].DefaultView;
		}

		void AddAllRowsIfMissing()
		{
			DataTable dirTab = cache.Set.Tables["Directions"];
			if (!dirTab.AsEnumerable().Any(r => (byte)r["direction_id"] == ALL_ID))
			{
				DataRow allDir = dirTab.NewRow();
				allDir["direction_id"] = ALL_ID;
				allDir["direction_name"] = "Все направления";
				dirTab.Rows.InsertAt(allDir, 0);
			}
			DataTable grpTab = cache.Set.Tables["Groups"];
			if (!grpTab.AsEnumerable().Any(r => (int)r["group_id"] == ALL_ID))
			{
				DataRow allGrp = grpTab.NewRow();
				allGrp["group_id"] = ALL_ID;
				allGrp["group_name"] = "Все группы";
				allGrp["direction"] = DBNull.Value;
				grpTab.Rows.InsertAt(allGrp, 0);
			}
		}
		void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
				this.DragMove(); // Двигаем окно мышкой за шапку
		}
		void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized; // Сворачиваем окно
		}

		void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close(); // Закрываем окно
		}
		
		void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cache.Set.Tables.Contains("Students"))
			{
				ComboBox comboBox = sender as ComboBox;
				if (comboBox != null && comboBox.SelectedItem != null)
				{
					DataRowView rowView = comboBox.SelectedItem as DataRowView;
					if (rowView != null)
					{
						string selectedGroupId = rowView["group_id"].ToString();

						if (selectedGroupId == ALL_ID.ToString())
						{
							cache.Set.Tables["Students"].DefaultView.RowFilter = "";
						}
						else
						{
							cache.Set.Tables["Students"].DefaultView.RowFilter = $"group = {selectedGroupId}";
						}
						
					}
				}
			}
		}

		private void DirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;
			if (comboBox != null && comboBox.SelectedItem != null)
			{
				DataRowView rowView = comboBox.SelectedItem as DataRowView;
				if (rowView != null)
				{
					string selectedValue = rowView["direction_id"].ToString();
					DataTable groupsTable = cache.Set.Tables["Groups"];

					if(selectedValue == ALL_ID.ToString())
						groupsTable.DefaultView.RowFilter = "";
					else
					{
						groupsTable.DefaultView.RowFilter = $"(group_id = {ALL_ID}) OR (direction = {selectedValue})";
					}
					

					if (GroupComboBox.Items.Count > 0)
					{
						GroupComboBox.SelectedIndex = 0;                          // Выбираем первую группу после фильтрации
						GroupComboBox_SelectionChanged(GroupComboBox, null);
					}
				}
			}
		}
		
		void ReloadData()
		{
			// Сохраняем текущие выбранные значения ComboBox
			object savedDirection = DirectionComboBox.SelectedValue;
			object savedGroup = GroupComboBox.SelectedValue;

			// Отключаем обработчики ComboBox
			GroupComboBox.SelectionChanged -= GroupComboBox_SelectionChanged;
			DirectionComboBox.SelectionChanged -= DirectionComboBox_SelectionChanged;

			// Очищаем старые данные из кэша (DataSet)
			ClearDataSet(cache.Set);

			LoadData();
			DataTable directionsTable = cache.Set.Tables["Directions"];
			DataTable groupsTable = cache.Set.Tables["Groups"];

			

				// Восстанавливаем выбор ранее выбранных элементов ComboBox
			if (savedDirection != null)
				DirectionComboBox.SelectedValue = savedDirection;
			else
				DirectionComboBox.SelectedValue = ALL_ID;

			if (savedGroup != null)
				GroupComboBox.SelectedValue = savedGroup;
			else
				GroupComboBox.SelectedValue = ALL_ID;

			// Возобновляем обработчики событий ComboBox
			GroupComboBox.SelectionChanged += GroupComboBox_SelectionChanged;
			DirectionComboBox.SelectionChanged += DirectionComboBox_SelectionChanged;

			// Применяем фильтрацию студентов согласно текущему выбору группы
			if (GroupComboBox.SelectedItem != null)
			{
				DataRowView selectedGroupRow = GroupComboBox.SelectedItem as DataRowView;
				if (selectedGroupRow != null)
				{
					string groupId = selectedGroupRow["group_id"].ToString();
					cache.Set.Tables["Students"].DefaultView.RowFilter = $"group = {groupId}";
				}
			}
			Console.WriteLine(new string('-', 50));
			Console.WriteLine($"Кэш обновлен в {DateTime.Now:HH:mm:ss}");
			Console.WriteLine();
			foreach (DataTable table in cache.Set.Tables)
			{
				Console.WriteLine($"Таблица {table.TableName}: {table.Rows.Count} записей");
			}
		}
		void SetReloadInterval(int minutes, int seconds = 0)
		{
			reloadIntervalSeconds = minutes * 60 + seconds;
			timeLeftSeconds = reloadIntervalSeconds;
			
		}
		void InitReloadTimer()
		{
			reloadTimer = new DispatcherTimer();
			reloadTimer.Interval = TimeSpan.FromSeconds(1);
			reloadTimer.Tick += ReloadTimer_Tick;
			reloadTimer.Start();
		}
		void ReloadTimer_Tick(object sender, EventArgs e)
		{
			timeLeftSeconds--;
			if (timeLeftSeconds <= 0)
			{
				ReloadData();
				timeLeftSeconds = reloadIntervalSeconds;
			}
			UpdateLabelTimer();
		}
		void UpdateLabelTimer()
		{
			int minutes = timeLeftSeconds / 60;
			int seconds = timeLeftSeconds % 60;
			UpdateTimerLabel.Content = $"Обновление через: {minutes:D2}:{seconds:D2}";
		}
		void ClearDataSet(DataSet set)
		{
			// Удаляем все существующие связи между таблицами
			for (int i = set.Relations.Count - 1; i >= 0; i--)
				set.Relations.RemoveAt(i);

			// Удаляем все ограничения внешних ключей в таблицах
			foreach (DataTable table in set.Tables)
			{
				for (int i = table.Constraints.Count - 1; i >= 0; i--)
				{
					if (table.Constraints[i] is ForeignKeyConstraint)
						table.Constraints.RemoveAt(i);
				}
			}
			// Очищаем остальные ограничения в каждой таблице
			foreach (DataTable table in set.Tables)
			{
				table.Constraints.Clear();
			}
			// Удаляем все таблицы из набора данных
			for (int i = set.Tables.Count - 1; i >= 0; i--)
			{
				set.Tables.RemoveAt(i);
			}
		}
	}
}
