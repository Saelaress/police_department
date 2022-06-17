using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace police_department_connection
{
    class DB
    {
        public static string stringForSortByFieldEmployees(MySqlCommand command)
        {
            Console.WriteLine("Введите номер поля для сортировки: 1 - ФИО, 2 - Пол, 3 - Номер телефона, 4 - Дата рождения, " +
                        "5 - Должность, 6 - Звание");

            var fieldForSorting = int.Parse(Console.ReadLine());

            if (fieldForSorting < 1 || fieldForSorting > 6)
                throw new Exception("Входная строка имела неверный формат");

            var employeesAttribute = new Dictionary<int, string>()
            {
                { 1, "full_name"},
                { 2, "sex"},
                { 3, "number_of_phone"},
                { 4, "date_of_birth"},
                { 5, "idposition"},
                { 6, "idrank"}
            };

            var param1 = employeesAttribute[fieldForSorting];

            if (fieldForSorting == 5) //Если выбрано поле с id - должность
            {
                Console.WriteLine("Введите номер должности: ");
                command.CommandText = "SELECT idposition, title FROM positions ORDER BY idposition ASC";

                using var readerPositions = command.ExecuteReader();
                int countEntries = 0;

                while (readerPositions.Read())
                {
                    Console.WriteLine($"{readerPositions.GetInt32(0)} - {readerPositions.GetString(1)}");
                    countEntries++;
                }

                var param2 = int.Parse(Console.ReadLine());

                if (param2 < 1 || param2 > countEntries)
                    throw new Exception("Входная строка имела неверный формат");

                command.CommandText += $" employees.{param1} = \"{param2}\"";
            }

            else if (fieldForSorting == 6) //Если выбрано поле с id - звание
            {
                Console.WriteLine("Введите номер звания: ");
                command.CommandText = "SELECT idrank, title FROM ranks ORDER BY idrank ASC";

                using var readerRanks = command.ExecuteReader();
                int countEntries = 0;

                while (readerRanks.Read())
                {
                    Console.WriteLine($"{readerRanks.GetInt32(0)} - {readerRanks.GetString(1)}");
                    countEntries++;
                }

                var param2 = int.Parse(Console.ReadLine());

                if (param2 < 1 || param2 > countEntries)
                    throw new Exception("Входная строка имела неверный формат");

                command.CommandText += $" employees.{param1} = \"{param2}\"";
            }

            else
            {
                Console.WriteLine("Введите значение поля:");
                var param2 = Console.ReadLine();
                command.CommandText += $" employees.{param1} = \"{param2}\"";
            }

            Console.WriteLine("Хотите отсортировать еще по какому-либо полю? 1 - Да, 2 - Нет ");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
            {
                command.CommandText += " AND ";
                command.CommandText = stringForSortByFieldEmployees(command);
            }

            return command.CommandText;
        }

        public static string stringForSortByFieldPositions(MySqlCommand command)
        {
            Console.WriteLine("Введите номер поля для сортировки: 1 - Название должности, 2 - Зарплата");

            var fieldForSorting = int.Parse(Console.ReadLine());

            if (fieldForSorting < 1 || fieldForSorting > 2)
                throw new Exception("Входная строка имела неверный формат");

            var positionsAttribute = new Dictionary<int, string>()
            {
                { 1, "title"},
                { 2, "salary"}
            };

            var param1 = positionsAttribute[fieldForSorting];

            Console.WriteLine("Введите значение поля:");
            var param2 = Console.ReadLine();

            command.CommandText += $" positions.{param1} = \"{param2}\"";

            Console.WriteLine("Хотите отсортировать еще по какому-либо полю? 1 - Да, 2 - Нет ");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
            {
                command.CommandText += " AND ";
                command.CommandText = stringForSortByFieldPositions(command);
            }

            return command.CommandText;
        }

        public static void selectEmployees(MySqlCommand command)
        {
            Console.WriteLine("Хотите ли вы отсортировать записи по полю? 1 - Да, 2 - Нет ");
            var sortOfEntries = int.Parse(Console.ReadLine());

            if (sortOfEntries < 1 || sortOfEntries > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (sortOfEntries == 1)
            {
                command.CommandText = "SELECT full_name, sex, number_of_phone, date_of_birth, positions.title, ranks.title " +
                "FROM employees NATURAL JOIN positions LEFT JOIN ranks ON ranks.idrank = employees.idrank WHERE ";

                command.CommandText = stringForSortByFieldEmployees(command);
            }

            else if (sortOfEntries == 2)
            {
                command.CommandText = "SELECT full_name, sex, number_of_phone, date_of_birth, positions.title, ranks.title " +
                "FROM employees NATURAL JOIN positions LEFT JOIN ranks ON ranks.idrank = employees.idrank";
            }

            Console.WriteLine("Задать промежуток времени для поиска записей? 1 - Да, 2 - Нет: ");
            var addTimeInterval = int.Parse(Console.ReadLine());

            if (addTimeInterval < 1 || addTimeInterval > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (addTimeInterval == 1)
            {
                if (sortOfEntries == 1)
                {
                    command.CommandText += " AND ";
                }

                else
                {
                    command.CommandText += " WHERE ";
                }

                Console.WriteLine("Введите дату начала: ");
                var date1 = DateTime.Parse(Console.ReadLine()).ToString("yyyy'-'MM'-'dd");

                Console.WriteLine("Введите дату конца: ");
                var date2 = DateTime.Parse(Console.ReadLine()).ToString("yyyy'-'MM'-'dd");

                command.CommandText += $" date_of_birth BETWEEN \"{@date1}\" AND \"{date2}\" ";
            }

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"Сотрудник: {reader.GetString(0)} {reader.GetString(1)}  " +
                    $"{reader.GetString(2)} {reader.GetDateTime(3).ToShortDateString()}. " +
                    $"Должность: {reader.GetString(4)}. Звание:{reader.GetString(5)}");
            }

            if (reader.HasRows == false)
                Console.WriteLine("Нет таких записей");

            reader.Close();

            Console.WriteLine("Хотите получить другие данные по сотрудникам? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                selectEmployees(command);
        }

        public static void selectPositions(MySqlCommand command)
        {
            Console.WriteLine("Хотите ли вы отсортировать записи по полю? 1 - Да, 2 - Нет ");
            var sortOfEntries = int.Parse(Console.ReadLine());

            if (sortOfEntries < 1 || sortOfEntries > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (sortOfEntries == 1)
            {
                command.CommandText = "SELECT title, salary FROM positions WHERE ";

                command.CommandText = stringForSortByFieldPositions(command);
            }

            else if (sortOfEntries == 2)
                command.CommandText = "SELECT title, salary FROM positions";

            var reader = command.ExecuteReader();

            while (reader.Read())
                Console.WriteLine($"Должность: {reader.GetString(0)}. Зарплата: {reader.GetDecimal(1)} ");

            if (reader.HasRows == false)
                Console.WriteLine("Нет таких записей");

            reader.Close();

            Console.WriteLine("Хотите получить другие данные по должностям? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                selectPositions(command);
        }

        public static void insertEmployees(MySqlCommand command)
        {
            command.CommandText = "INSERT INTO employees (full_name, sex, number_of_phone, date_of_birth, idposition, idrank) "
                                                 + " values (@full_name, @sex, @number_of_phone, @date_of_birth, @idposition, @idrank) ";

            //Ввод параметров для вставки
            Console.WriteLine("Введите ФИО сотрудника: ");
            command.Parameters.Add("@full_name", MySqlDbType.String).Value = Console.ReadLine();

            Console.WriteLine("Введите пол сотрудника: м или ж");
            command.Parameters.Add("@sex", MySqlDbType.VarChar).Value = Console.ReadLine();

            Console.WriteLine("Введите номер телефона сотрудника: ");
            command.Parameters.Add("@number_of_phone", MySqlDbType.String).Value = Console.ReadLine();

            Console.WriteLine("Введите дату рождения сотрудника: ");
            command.Parameters.Add("@date_of_birth", MySqlDbType.Date).Value = DateTime.Parse(Console.ReadLine()).ToString("yyyy'-'MM'-'dd");

            Console.WriteLine("Введите id должности сотрудника: ");
            command.Parameters.Add("@idposition", MySqlDbType.Int32).Value = int.Parse(Console.ReadLine());

            Console.WriteLine("Введите id звания сотрудника: ");
            command.Parameters.Add("@idrank", MySqlDbType.Int32).Value = int.Parse(Console.ReadLine());

            int rowCount = command.ExecuteNonQuery();

            command.Parameters.Clear();

            Console.WriteLine("Хотите добавить еще одного сотрудника? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                insertEmployees(command);
        }
                                          
        public static void insertPositions(MySqlCommand command)
        {
            command.CommandText = "INSERT INTO positions (title, salary) "
                                                 + " values (@title, @salary) ";

            //Ввод параметров для вставки
            Console.WriteLine("Введите название должности: ");
            command.Parameters.Add("@title", MySqlDbType.String).Value = Console.ReadLine();

            Console.WriteLine("Введите зарплату");
            command.Parameters.Add("@salary", MySqlDbType.Decimal).Value = int.Parse(Console.ReadLine());

            int rowCount = command.ExecuteNonQuery();

            command.Parameters.Clear();

            Console.WriteLine("Хотите добавить еще одну должность? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                insertPositions(command);
        }
                                          
        public static void updateEmployees(MySqlCommand command)
        {
            Console.WriteLine("Значение какого поля вы хотите поменять? \n 1 - ФИО, \n 2 - Пол, \n 3 - Номер телефона," +
                "\n 4 - Дата рождения, \n 5 - Должность, \n 6 - Звание");

            var fieldToUpdate = int.Parse(Console.ReadLine());

            if (fieldToUpdate < 1 || fieldToUpdate > 6)
                throw new Exception("Входная строка имела неверный формат");

            var employeesAttribute = new Dictionary<int, string>()
            {
                { 1, "full_name"},
                { 2, "sex"},
                { 3, "number_of_phone"},
                { 4, "date_of_birth"},
                { 5, "idposition"},
                { 6, "idrank"}
            };

            var param1 = employeesAttribute[fieldToUpdate];


            if (fieldToUpdate == 5) //Если выбрано поле с id - должность
            {
                using var reader = command.ExecuteReader();

                Console.WriteLine("Введите номер должности: ");
                command.CommandText = "SELECT idposition, title FROM positions ORDER BY idposition ASC";

                int countEntries = 0;

                while (reader.Read())
                {
                    Console.WriteLine($"{reader.GetInt32(0)} - {reader.GetString(1)}");
                    countEntries++;
                }

                var value1 = int.Parse(Console.ReadLine());

                if (value1 < 1 || value1 > countEntries)
                {
                    throw new Exception("Входная строка имела неверный формат");
                }

                command.CommandText = $"UPDATE employees set {param1} = \"{value1}\" WHERE ";

                reader.Close();
            }

            else if (fieldToUpdate == 6) //Если выбрано поле с id - звание
            {
                using var reader = command.ExecuteReader();

                Console.WriteLine("Введите номер звания: ");
                command.CommandText = "SELECT idrank, title FROM ranks ORDER BY idrank ASC";

                int countEntries = 0;

                while (reader.Read())
                {
                    Console.WriteLine($"{reader.GetInt32(0)} - {reader.GetString(1)}");
                    countEntries++;
                }

                var value1 = int.Parse(Console.ReadLine());

                if (value1 < 1 || value1 > countEntries)
                    throw new Exception("Входная строка имела неверный формат");
                
                command.CommandText = $"UPDATE employees set {param1} = \"{value1}\" WHERE ";

                reader.Close();
            }

            else
            {
                Console.WriteLine("Введите значение для выбранного поля: ");
                var value1 = Console.ReadLine();

                command.CommandText = $"UPDATE employees set {param1} = \"{value1}\" WHERE ";
            }

            command.CommandText = stringForSortByFieldEmployees(command);

            Console.WriteLine("Задать промежуток времени для поиска записей? 1 - Да, 2 - Нет ");
            var addTimeInterval = int.Parse(Console.ReadLine());

            if (addTimeInterval < 1 || addTimeInterval > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (addTimeInterval == 1)
            {
                command.CommandText += " AND ";

                Console.WriteLine("Введите дату начала: ");
                var date1 = DateTime.Parse(Console.ReadLine()).ToString("yyyy'-'MM'-'dd");

                Console.WriteLine("Введите дату конца: ");
                var date2 = DateTime.Parse(Console.ReadLine()).ToString("yyyy'-'MM'-'dd");

                command.CommandText += $" date_of_birth BETWEEN \"{@date1}\" AND \"{date2}\" ";
            }

            int rowCount = command.ExecuteNonQuery();

            Console.WriteLine("Количество затронутых строк = " + rowCount);

            Console.WriteLine("Хотите поменять еще одно поле? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                updateEmployees(command);
        }

        public static void updatePositions(MySqlCommand command)
        {
            Console.WriteLine("Значение какого поля вы хотите поменять? \n 1 - Название, \n 2 - Зарплата");
            var fieldToUpdate = int.Parse(Console.ReadLine());

            if (fieldToUpdate < 1 || fieldToUpdate > 2)
                throw new Exception("Входная строка имела неверный формат");

            var positionsAttribute = new Dictionary<int, string>()
            {
                { 1, "title"},
                { 2, "salary"}
            };

            var param1 = positionsAttribute[fieldToUpdate];

            Console.WriteLine("Введите значение для выбранного поля: ");
            var value1 = Console.ReadLine();

            command.CommandText = $"UPDATE positions set {param1} = \"{value1}\" WHERE ";

            //Сортировка записей
            command.CommandText = stringForSortByFieldPositions(command);

            int rowCount = command.ExecuteNonQuery();

            Console.WriteLine("Количество затронутых строк = " + rowCount);

            Console.WriteLine("Хотите поменять еще одно поле? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                updatePositions(command);
        }

        public static void deleteEmployees(MySqlCommand command)
        {
            command.CommandText = $"DELETE FROM employees WHERE ";

            command.CommandText = stringForSortByFieldEmployees(command);

            Console.WriteLine("Задать промежуток времени для поиска записей? 1 - Да, 2 - Нет ");
            var addTimeInterval = int.Parse(Console.ReadLine());

            if (addTimeInterval < 1 || addTimeInterval > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (addTimeInterval == 1)
            {
                command.CommandText += " AND ";

                Console.WriteLine("Введите дату начала: ");
                var date1 = DateTime.Parse(Console.ReadLine()).ToString("yyyy'-'MM'-'dd");

                Console.WriteLine("Введите дату конца: ");
                var date2 = DateTime.Parse(Console.ReadLine()).ToString("yyyy'-'MM'-'dd");

                command.CommandText += $" date_of_birth BETWEEN \"{@date1}\" AND \"{date2}\" ";
            }

            int rowCount = command.ExecuteNonQuery();

            Console.WriteLine("Количество затронутых строк = " + rowCount);

            Console.WriteLine("Хотите удалить еще одного сотрудника? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (addTimeInterval < 1 || addTimeInterval > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                deleteEmployees(command);
        }

        public static void deletePositions(MySqlCommand command)
        {
            command.CommandText = $"DELETE FROM positions WHERE ";

            command.CommandText = stringForSortByFieldPositions(command);

            int rowCount = command.ExecuteNonQuery();

            Console.WriteLine("Количество затронутых строк = " + rowCount);

            Console.WriteLine("Хотите удалить еще одну должность? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                deletePositions(command);
        }
    }
}
