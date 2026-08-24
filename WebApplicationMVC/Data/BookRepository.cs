using Microsoft.Data.SqlClient;
using System.Data;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Data;

public class BookRepository
{
    private readonly string _connectionString;

    public BookRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("HomeLibrary")
            ?? throw new InvalidOperationException(
                "Connection string 'HomeLibrary' not found."
            );
    }

    public async Task<List<Book>> GetAllAsync()
    {
        var books = new List<Book>();

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand("sp_GetAllBooks", connection);

        command.CommandType = CommandType.StoredProcedure;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            books.Add(MapBook(reader));
        }

        return books;
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand("sp_GetBookById", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(
            "@Id",
            SqlDbType.Int
        ).Value = id;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapBook(reader);
        }

        return null;
    }

    public async Task<int> InsertAsync(Book book)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand("sp_InsertBook", connection);

        command.CommandType = CommandType.StoredProcedure;

        AddBookParameters(command, book);

        var newId = command.Parameters.Add(
            "@NewId",
            SqlDbType.Int
        );

        newId.Direction = ParameterDirection.Output;

        await connection.OpenAsync();

        await command.ExecuteNonQueryAsync();

        return Convert.ToInt32(newId.Value);
    }

    public async Task UpdateAsync(Book book)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand("sp_UpdateBook", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(
            "@Id",
            SqlDbType.Int
        ).Value = book.Id;

        AddBookParameters(command, book);

        await connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand("sp_DeleteBook", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(
            "@Id",
            SqlDbType.Int
        ).Value = id;

        await connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
    }

    private static void AddBookParameters(
        SqlCommand command,
        Book book)
    {
        command.Parameters.Add(
            "@Title",
            SqlDbType.NVarChar,
            255
        ).Value = book.Title;

        command.Parameters.Add(
            "@Author",
            SqlDbType.NVarChar,
            255
        ).Value = book.Author;

        command.Parameters.Add(
            "@YearPublished",
            SqlDbType.Int
        ).Value = (object?)book.YearPublished
                  ?? DBNull.Value;

        command.Parameters.Add(
            "@ISBN",
            SqlDbType.VarChar,
            20
        ).Value = (object?)book.ISBN
                  ?? DBNull.Value;

        command.Parameters.Add(
            "@Description",
            SqlDbType.NVarChar,
            -1
        ).Value = (object?)book.Description
                  ?? DBNull.Value;

        command.Parameters.Add(
            "@TableOfContents",
            SqlDbType.Xml
        ).Value = (object?)book.TableOfContents
                  ?? DBNull.Value;
    }

    private static Book MapBook(
        SqlDataReader reader)
    {
        return new Book
        {
            Id = reader.GetInt32(
                reader.GetOrdinal("Id")),

            Title = reader.GetString(
                reader.GetOrdinal("Title")),

            Author = reader.GetString(
                reader.GetOrdinal("Author")),

            YearPublished =
                reader.IsDBNull(
                    reader.GetOrdinal("YearPublished"))
                    ? null
                    : reader.GetInt32(
                        reader.GetOrdinal("YearPublished")),

            ISBN =
                reader.IsDBNull(
                    reader.GetOrdinal("ISBN"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("ISBN")),

            Description =
                reader.IsDBNull(
                    reader.GetOrdinal("Description"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Description")),

            TableOfContents =
                reader.IsDBNull(
                    reader.GetOrdinal("TableOfContents"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("TableOfContents"))
        };
    }

    public async Task<List<XmlValue>> GetXmlValuesAsync(int bookId)
    {
        var result = new List<XmlValue>();

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand("sp_GetBookXmlValues", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(
            "@BookId",
            SqlDbType.Int
        ).Value = bookId;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new XmlValue
            {
                NodeName = reader["NodeName"].ToString()!,
                NodeValue = reader["NodeValue"]?.ToString()
            });
        }

        return result;
    }

    public async Task<List<Book>> FindByXmlTextAsync(
    string searchText)
    {
        var books = new List<Book>();

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(
                "sp_FindBooksByXmlText",
                connection
            );

        command.CommandType =
            CommandType.StoredProcedure;

        command.Parameters.Add(
            "@SearchText",
            SqlDbType.NVarChar,
            255
        ).Value = searchText;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            books.Add(new Book
            {
                Id = reader.GetInt32(
                    reader.GetOrdinal("Id")),

                Title = reader.GetString(
                    reader.GetOrdinal("Title")),

                Author = reader.GetString(
                    reader.GetOrdinal("Author")),

                YearPublished =
                    reader.IsDBNull(
                        reader.GetOrdinal("YearPublished"))
                        ? null
                        : reader.GetInt32(
                            reader.GetOrdinal("YearPublished")),

                ISBN =
                    reader.IsDBNull(
                        reader.GetOrdinal("ISBN"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal("ISBN"))
            });
        }

        return books;
    }
}