using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using police_department_connection;


namespace TestDBConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Server=127.0.0.1;User ID=root;Password=31415;Database=police_department";

            using var connection = new MySqlConnection(connectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            try
            {
                databaseInteraction(connection, command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.Write($"{Environment.NewLine}Нажмите любую клавишу для выхода...");
                Console.ReadKey(true);
            }

        }

        static void databaseInteraction (MySqlConnection connection, MySqlCommand command)
        {
            Console.WriteLine("Что вы хотите сделать? 1 - Поработать с таблицей, " +
                "\n2 - Отобразить содержимое уголовного дела с заданным номером," +
                "\n3 - Отобразить количество преступлений, совершенных заданным преступником");

            var actionNumber = int.Parse(Console.ReadLine());

            if (actionNumber == 1)
                workWithTable(command);

            else if (actionNumber == 2)
            {
                contentsOfCriminalCase(command);
            }
            
            else if (actionNumber == 3)
            {
                countCrimes(command);
            }

            Console.WriteLine("Хотите продолжить работу? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                databaseInteraction(connection, command);
        }

        static void workWithTable(MySqlCommand command)
        {
            Console.WriteLine("С какой таблицей вы хотели бы работать? 1 - Сотрудники, 2 - Должности");
            var tableNumber = int.Parse(Console.ReadLine());

            if (tableNumber < 1 || tableNumber > 2)
                throw new Exception("Входная строка имела неверный формат");

            Console.WriteLine("Какую операцию вы хотите выполнить? 1 - Получить, 2 - Добавить, 3 - Обновить, 4 - Удалить данные");
            var operationType = int.Parse(Console.ReadLine());

            if (operationType < 1 || operationType > 4)
                throw new Exception("Входная строка имела неверный формат");

            switch (operationType)
            {
                case 1:
                    selectQuery(tableNumber, command);
                    break;
                case 2:
                    insertQuery(tableNumber, command);
                    break;
                case 3:
                    updateQuery(tableNumber, command);
                    break;
                case 4:
                    deleteQuery(tableNumber, command);
                    break;
            }

            Console.WriteLine("Хотите продолжить работу с таблицами? 1 - Да, 2 - Нет");
            var next = int.Parse(Console.ReadLine());

            if (next < 1 || next > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (next == 1)
                workWithTable(command);          

        }

        static void contentsOfCriminalCase(MySqlCommand command)
        {
            Console.WriteLine("\nОтобразить номера уголовных дел? 1 - Да, 2 - Нет");
            var display = int.Parse(Console.ReadLine());

            if (display < 1 || display > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (display == 1)
                displayCriminalCaseNumbers(command);

            Console.WriteLine("\nВведите номер уголовного дела");
            var numberCriminalCase = int.Parse(Console.ReadLine());

            command.CommandText = "select criminal_cases.number, criminal_cases.date_of_initiation, crimes.name, " +
                "group_concat(' ', evidences.code), " +
                "concat(' ', criminals.full_name, ', ', criminals.sex, ', ', criminals.date_of_birth), " +
                "articles.number, " +
                "concat(' ', cs.district, ', ', cs.address), " +
                "concat(' ', employees.full_name, ', ', employees.date_of_birth), " +
                "police_statements.text, " +
                "concat(' ', victims.full_name, ', ', victims.date_of_birth) " +
                "from criminal_cases " +
                "left join crimes " +
                "on criminal_cases.idcriminal_case = crimes.idcrime " +
                "left join evidences " +
                "on crimes.idcrime = evidences.idcrime " +
                "left join criminals_to_crimes as cc " +
                "on crimes.idcrime = cc.idcrime " +
                "left join criminals " +
                "on cc.idcriminal = criminals.idcriminal " +
                "left join articles " +
                "on crimes.idarticle = articles.idarticle " +
                "left join crime_scenes as cs " +
                "on crimes.idcrime_scene = cs.idcrime_scene " +
                "left join employees " +
                "on criminal_cases.idemployee = employees.idemployee " +
                "left join police_statements_to_criminal_cases as pscc " +
                "on criminal_cases.idcriminal_case = pscc.idcriminal_case " +
                "left join police_statements " +
                "on pscc.idpolice_statement = police_statements.idpolice_statement " +
                "left join victims " +
                "on police_statements.idvictim = victims.idvictim " +
                $"where criminal_cases.number = \"{numberCriminalCase}\" " +
                "group by criminals.full_name ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"Номер: {reader.GetInt32(0)} \n\nДата возбуждения: {reader.GetDateTime(1).ToShortDateString()}.  " +
                    $"\n\nПреступление: {reader.GetValue(2)}. \n\nКод улики: {reader.GetValue(3)}. " +
                    $"\n\nПреступник: {reader.GetValue(4)}. \n\nСтатья УК:{reader.GetValue(5)}. " +
                    $"\n\nРайон и место преступления: {reader.GetValue(6)}. \n\nСотрудник: {reader.GetValue(7)}." +
                    $"\n\nЗаявление: {reader.GetValue(8)}. \n\nПострадавший: {reader.GetValue(8)}.");
            }

            if (reader.HasRows == false)
                Console.WriteLine("Нет таких записей");

            reader.Close();

            Console.WriteLine("Хотите получить данные по другому делу? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                contentsOfCriminalCase(command);
        }

        static void displayCriminalCaseNumbers(MySqlCommand command)
        {
            command.CommandText = "select criminal_cases.number from criminal_cases";

            using var reader = command.ExecuteReader();
            Console.WriteLine($"\nНомера дел: ");

            while (reader.Read())
            {
                Console.Write($"{reader.GetInt32(0)}, ");
            }
        }

        static void displayCriminalsName(MySqlCommand command)
        {
            command.CommandText = "select criminals.full_name from criminals";

            using var reader = command.ExecuteReader();
            Console.WriteLine($"\n: ");

            while (reader.Read())
            {
                Console.Write($"{reader.GetString(0)}, ");
            }
        }

        static void countCrimes(MySqlCommand command)
        {
            Console.WriteLine("\nОтобразить ФИО преступников? 1 - Да, 2 - Нет");
            var display = int.Parse(Console.ReadLine());

            if (display < 1 || display > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (display == 1)
                displayCriminals(command);

            Console.WriteLine("Введите ФИО преступника");
            string criminalsFullName = Console.ReadLine();

            command.CommandText = "select count(*) as countCertainCriminal " +
                "from criminals_to_crimes natural join criminals " +
                $"where full_name = \"{criminalsFullName}\"; ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"Количество преступлений: {reader.GetInt64(0)}.");
            }

            if (reader.HasRows == false)
                Console.WriteLine("Нет таких записей");

            reader.Close();

            Console.WriteLine("Хотите узнать количество преступлений другого преступника? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                countCrimes(command);
        }

        static void selectQuery(int tableNumber, MySqlCommand command)
        {
            if (tableNumber == 1)
                DB.selectEmployees(command);

            else if (tableNumber == 2)
                DB.selectPositions(command);
        }

        static void insertQuery(int tableNumber, MySqlCommand command)
        {
            if (tableNumber == 1)
                DB.insertEmployees(command);

            else if (tableNumber == 2)
                DB.insertPositions(command);
        }
                                                 
        static void updateQuery(int tableNumber, MySqlCommand command)
        {
            if (tableNumber == 1)
                DB.updateEmployees(command);

            else if (tableNumber == 2)
                DB.updatePositions(command);
        }
                                                 
        static void deleteQuery(int tableNumber, MySqlCommand command)
        {
            if (tableNumber == 1)
                DB.deleteEmployees(command);

            else if (tableNumber == 2)
                DB.deletePositions(command);
        }
    }
}



