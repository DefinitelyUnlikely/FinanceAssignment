using Finance.Data.Database;
using Finance.Data.Interfaces;
using Finance.Models;
using Npgsql;

namespace Finance.Data.Repositories;

// TODO: change the catch to throwing an exception and let the thing trying to do the action 
// show the DisplayAlert. Keeping it for now for easier debugging.
public class UserRepository : IUserRepository
{

    private readonly IFinanceDatabase database;

    private User? currentUser;
    public User? CurrentUser { get => currentUser; } // Keep the id instead? Easier when updating.

    public Dictionary<string, User> userCache = [];

    public UserRepository(IFinanceDatabase database)
    {
        this.database = database;
    }


    public void SetUser(User? user)
    {
        currentUser = user;
    }

    public void ResetUser()
    {
        currentUser = null;
    }

    public async Task<bool> AddUserAsync(string email, string name, string salt, string password)
    {

        string sql = @"INSERT INTO users (email, display_name, salt, password) VALUES (@email, @name, @salt, @password);";
        await using var connection = (NpgsqlConnection)await database.GetConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@salt", salt);
        command.Parameters.AddWithValue("@password", password);

        try
        {
            if (await command.ExecuteNonQueryAsync() == -1)
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            throw new Exception("Couldn't add user: " + e.Message + " UserRepository.cs\\AddUserAsync \n");
        }

    }

    public async Task<User?> GetUserAsync(string name)
    {
        if (userCache.TryGetValue(name.ToUpper(), out User? value))
        {
            return value;
        }

        string sql = @"SELECT * FROM users WHERE user_name = @name;";
        await using var connection = (NpgsqlConnection)await database.GetConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@name", name.ToUpper());

        try
        {

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                User getUser = new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5)
                    );

                userCache.Add(getUser.UserName, getUser);

                return getUser;
            }

            return null;

        }
        catch (Exception e)
        {
            throw new Exception("Couldn't GET user. Exception: " + e.Message + " UserRepository.cs\\GetUserAsync \n");
        }


    }

    public async Task<bool> UpdateUserAsync(int id, Dictionary<string, string> columnsValues)
    {
        await using var connection = (NpgsqlConnection)await database.GetConnectionAsync();
        await using var sqlTransaction = await connection.BeginTransactionAsync();

        try
        {
            // As I cannot parameterize column names, claude.ai gave me the idea that one 
            // can instead check that the incoming column name is in an allowed list of names
            // to highten security somewhat.
            List<string> allowedColumns = ["email", "display_name", "salt", "password"];

            // One can do several columns in one update, but I could not come up with a 
            // way to make that approach work for an unknown amount of columns to update.
            // So we are using a forloop instead. 
            foreach (KeyValuePair<string, string> entry in columnsValues)
            {

                if (!allowedColumns.Contains(entry.Key))
                {
                    await sqlTransaction.RollbackAsync();
                    return false;
                }

                string sql = $"UPDATE users SET {entry.Key} = @value WHERE id = @id;";
                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.AddWithValue("@value", entry.Value);
                command.Parameters.AddWithValue("@id", id);

                await command.ExecuteNonQueryAsync();
            }

            await sqlTransaction.CommitAsync();
            return true;

        }
        catch (Exception e)
        {
            await sqlTransaction.RollbackAsync();
            throw new Exception("Update failed, transaction rolled back: " + e.Message + " UserRepository.cs\\UpdateUserAsync \n");
        }


    }

    public async Task<bool> RemoveUserAsync(int id)
    {
        await using var connection = (NpgsqlConnection)await database.GetConnectionAsync();
        string sql = @"DELETE FROM users WHERE id = @id;";
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", id);
        try
        {
            return await command.ExecuteNonQueryAsync() != -1;
        }
        catch (Exception e)
        {
            throw new Exception("Internal User Error: " + e.Message);
        }

    }

    public async Task<bool> UserExistsAsync(string name)
    {
        await using var connection = (NpgsqlConnection)await database.GetConnectionAsync();
        string sql = @"SELECT * FROM users WHERE user_name = @name;";
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@name", name.ToUpper());

        try
        {
            var count = await command.ExecuteScalarAsync();
            return Convert.ToInt32(count) > 0;
        }
        catch (Exception e)
        {
            throw new Exception("Internal User Error: " + e.Message + " UserRepository.cs\\UserExistsAsync \n");
        }

    }

}