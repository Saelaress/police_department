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

            try
            {
                databaseInteraction(connection);
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

        static void databaseInteraction (MySqlConnection connection)
        {
            Console.WriteLine("Что вы хотите сделать? 1 - Поработать с таблицей, " +
                "\n2 - Отобразить содержимое уголовного дела с заданным номером," +
                "\n3 - Отобразить количество преступлений, совершенных заданным преступником");

            var actionNumber = int.Parse(Console.ReadLine());

            if (actionNumber == 1)
                workWithTable(connection);

            else if (actionNumber == 2)
            {
                contentsOfCriminalCase(connection);
            }
            
            else if (actionNumber == 3)
            {

            }

            Console.WriteLine("Хотите продолжить работу? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                databaseInteraction(connection);
        }

        static void workWithTable(MySqlConnection connection)
        {
            Console.WriteLine("С какой таблицей вы хотели бы работать? 1 - Сотрудники, 2 - Должности");
            var tableNumber = int.Parse(Console.ReadLine());

            if (tableNumber < 1 || tableNumber > 2)
                throw new Exception("Входная строка имела неверный формат");

            Console.WriteLine("Какую операцию вы хотите выполнить? 1 - Получить, 2 - Добавить, 3 - Обновить, 4 - Удалить данные");
            var operationType = int.Parse(Console.ReadLine());

            if (operationType < 1 || operationType > 4)
                throw new Exception("Входная строка имела неверный формат");

            using var command = connection.CreateCommand();

            switch (operationType)
            {
                case 1:
                    selectQuery(tableNumber, connection);
                    break;
                case 2:
                    insertQuery(tableNumber, connection);
                    break;
                case 3:
                    updateQuery(tableNumber, connection);
                    break;
                case 4:
                    deleteQuery(tableNumber, connection);
                    break;
            }

            Console.WriteLine("Хотите продолжить работу с таблицами? 1 - Да, 2 - Нет");
            var next = int.Parse(Console.ReadLine());

            if (next < 1 || next > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (next == 1)
                workWithTable(connection);          
        }

        static void contentsOfCriminalCase(MySqlConnection connection)
        {
            Console.WriteLine("Введите номер уголовного дела");
            var numberCriminalCase = int.Parse(Console.ReadLine());

            using var command = connection.CreateCommand();

            command.CommandText = "select criminal_cases.number, criminal_cases.date_of_initiation, crimes.name, " +
                "group_concat(' ', evidences.code), " +
                "concat(' ', criminals.full_name, ', ', criminals.sex, ', ', criminals.date_of_birth), " +
                "articles.number, " +
                "concat(' ', cs.district, ', ', cs.address), " +
                "concat(' ', employees.full_name, ', ', employees.date_of_birth), " +
                "police_statements.text, " +
                "concat(' ', victims.full_name, ', ', victims.date_of_birth) " +
                "from criminal_cases " +
                "natural join crimes " +
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
                    $"\n\nПреступление: {reader.GetString(2)}. \n\nКод улики: {reader.GetString(3)}. " +
                    $"\n\nПреступник: {reader.GetString(4)}. \n\nСтатья УК:{reader.GetFloat(5)}. " +
                    $"\n\nРайон и место преступления: {reader.GetString(6)}. \n\nСотрудник: {reader.GetString(7)}." +
                    $"\n\nЗаявление: {reader.GetString(8)}. \n\nПострадавший: {reader.GetString(8)}.");
            }

            if (reader.HasRows == false)
                Console.WriteLine("Нет таких записей");

            Console.WriteLine("Хотите получить данные по другому делу? 1 - Да, 2 - Нет");
            var solution = int.Parse(Console.ReadLine());

            if (solution < 1 || solution > 2)
                throw new Exception("Входная строка имела неверный формат");

            if (solution == 1)
                contentsOfCriminalCase(connection);
        }

        static void selectQuery(int tableNumber, MySqlConnection connection)
        {
            using var command = connection.CreateCommand();

            if (tableNumber == 1)
                DB.selectEmployees(connection);

            else if (tableNumber == 2)
                DB.selectPositions(connection);
        }

        static void insertQuery(int tableNumber, MySqlConnection connection)
        {
            using var command = connection.CreateCommand();

            if (tableNumber == 1)
                DB.insertEmployees(connection);

            else if (tableNumber == 2)
                DB.insertPositions(connection);
        }

        static void updateQuery(int tableNumber, MySqlConnection connection)
        {
            using var command = connection.CreateCommand();

            if (tableNumber == 1)
                DB.updateEmployees(connection);

            else if (tableNumber == 2)
                DB.updatePositions(connection);
        }

        static void deleteQuery(int tableNumber, MySqlConnection connection)
        {
            using var command = connection.CreateCommand();

            if (tableNumber == 1)
                DB.deleteEmployees(connection);

            else if (tableNumber == 2)
                DB.deletePositions(connection);
        }
    }
}



