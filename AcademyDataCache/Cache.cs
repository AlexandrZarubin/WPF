using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;                                                  // Для работы с DataSet и DataTable
using System.Data.SqlClient;                                        // Для работы с SQL Server базой данных
using System.Configuration;                                         // Для чтения строки подключения из конфигурации

namespace AcademyDataCache
{
    public class Cache
    {
		public DataSet Set { get; private set; }                    // Свойство для хранения всех таблиц данных в памяти
		private string connectionString;                            // Приватная переменная для хранения строки подключения к базе данных

		public Cache(string connectionString)
		{
			this.connectionString = connectionString;               // Сохраняем переданную строку подключения в приватную переменную
			Set = new DataSet();                                    // Создаём новый пустой набор данных
		}

		// Метод для добавления таблицы из базы данных в память
		public void AddTable(string tableName, string columns)      
		{
			// Создаём новый объект подключения к базе данных
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();                                  // Открываем подключение к базе данных
				string[] columnArray = columns.Split(',');          // Разбиваем список колонок по запятым
				string safeColumns = string.Join(",", columnArray.Select(c => $"[{c.Trim()}]")); // Для каждой колонки добавляем [скобки]
				string query = $"SELECT {safeColumns} FROM [{tableName}]";// Формируем SQL-запрос для выбора данных

				// Создаём SQL-команду с текстом запроса и открытым подключением
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					SqlDataAdapter adapter = new SqlDataAdapter(command);	// Создаём адаптер данных. Он позволяет автоматически загружать таблицы
					DataTable table = new DataTable(tableName);             // Создаём новую пустую таблицу в памяти
					adapter.Fill(table);                                    // Заполняем таблицу данными из базы через адаптер
					table.PrimaryKey = new DataColumn[] { table.Columns[0] };
					Set.Tables.Add(table);                                  // Добавляем таблицу в общий набор данных
				}
			}
		}
		public void AddRelation(string relation_name, string child, string parent)
		{
			Set.Relations.Add
				(
				relation_name,
				Set.Tables[parent.Split(',')[0]].Columns[parent.Split(',')[1]],
				Set.Tables[child.Split(',')[0]].Columns[child.Split(',')[1]]
				);
		}
		/*
	// Метод для создания связи между двумя таблицами
		public void AddRelation(string relationName, string parentInfo, string childInfo)   
		{
			string[] parentParts = parentInfo.Split(',');                   // Разделяем строку родителя по запятой: сначала имя таблицы, потом имя по
			string[] childParts = childInfo.Split(',');                     // Разделяем строку дочернего элемента по запятой

			DataTable parentTable = Set.Tables[parentParts[0]];             // Получаем родительскую таблицу из набора данных
			DataTable childTable = Set.Tables[childParts[0]];               // Получаем дочернюю таблицу из набора данных

			DataRelation relation = new DataRelation(
				relationName,                                               // Название связи
				parentTable.Columns[parentParts[1]],                        // Родительская колонка
				childTable.Columns[childParts[1]]                           // Дочерняя колонка
				);
			Set.Relations.Add(relation);                                    // Добавляем эту связь в общий набор данных
		}
		*/

		// Метод для проверки: есть ли у таблицы родители (связанные таблицы)
		public bool HasParents(string tableName)
		{
			DataTable table = Set.Tables[tableName];
			return table.ParentRelations.Count > 0;
		}
	}
}
