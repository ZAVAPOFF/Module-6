using Biblioteka;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

public class LibraryDatabase
{
    private string _connectionString;

    public LibraryDatabase(string databasePath)
    {
        _connectionString = $"Data Source={databasePath};Version=3;";
    }

    // Получить книги с возможностью поиска
    public List<Book> GetBooks(string searchQuery = "")
    {
        var books = new List<Book>();
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            string sql = "SELECT * FROM Books WHERE Title LIKE @searchQuery";
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@searchQuery", $"%{searchQuery}%");
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            BookID = reader.GetInt32(0),            // BookID
                            Title = reader.GetString(1),            // Title
                            Author = reader.GetString(2),           // Author
                            Genre = reader.IsDBNull(3) ? null : reader.GetString(3),  // Genre (может быть NULL)
                            Year = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),       // Year (может быть NULL)
                            IsAvailiable = Convert.ToBoolean(reader.GetInt32(5))      // Преобразование из INTEGER в bool
                        });
                    }
                }
            }
        }
        return books;
    }


    // Аренда книги
    public void RentBook(int bookID, int userID)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Обновляем статус книги в таблице Books
                    string updateBookSql = "UPDATE Books SET IsAvailiable = 0 WHERE BookID = @bookID";
                    using (var updateCommand = new SQLiteCommand(updateBookSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@bookID", bookID);
                        updateCommand.ExecuteNonQuery();
                    }

                    // Добавляем запись в таблицу Rentals
                    string rentBookSql = "INSERT INTO Rentals (BookID, UserID, RentDate) VALUES (@bookID, @userID, @rentDate)";
                    using (var insertCommand = new SQLiteCommand(rentBookSql, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@bookID", bookID);
                        insertCommand.Parameters.AddWithValue("@userID", userID);
                        insertCommand.Parameters.AddWithValue("@rentDate", DateTime.Now.ToString("yyyy-MM-dd"));
                        insertCommand.ExecuteNonQuery();
                    }

                    // Подтверждаем транзакцию
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Откат транзакции при ошибке
                    transaction.Rollback();
                    Console.WriteLine("Ошибка при аренде книги: " + ex.Message);
                }
            }
        }
    }




    // Получить список арендованных книг пользователем
    public List<Book> GetRentedBooks()
    {
        var books = new List<Book>();
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            string sql = @"
            SELECT b.BookID, b.Title, b.Author, b.Genre, b.Year, b.IsAvailiable 
            FROM Books b
            INNER JOIN Rentals r ON b.BookID = r.BookID
            WHERE r.ReturnDate IS NULL";
            using (var command = new SQLiteCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            BookID = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Author = reader.GetString(2),
                            Genre = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Year = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                            IsAvailiable = Convert.ToBoolean(reader.GetInt32(5))
                        });
                    }
                }
            }
        }
        return books;
    }


}
