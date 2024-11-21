using System.Reflection;
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
    public User? CurrentUser { get => currentUser; }

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

        User addUser = new(email, name, salt, password);


        string sql = @"INSERT INTO users (email, name, salt, password) VALUES (@email, @name, @salt, @password);";
        await using var connection = (NpgsqlConnection)await database.GetConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@email", addUser.Email);
        command.Parameters.AddWithValue("@name", addUser.Name);
        command.Parameters.AddWithValue("@salt", addUser.Salt);
        command.Parameters.AddWithValue("@password", addUser.PasswordHash);

        try
        {
            if (await command.ExecuteNonQueryAsync() == -1)
            {
                return false;
            }

            userCache.Add(addUser.Name, addUser);

            return true;
        }
        catch (Exception e)
        {
            await Shell.Current.DisplayAlert("Internal User Error", e.Message, "OK");
        }

        return false;

    }

    public async Task<User?> GetUserAsync(string name)
    {
        if (userCache.TryGetValue(name, out User? value))
        {
            return value;
        }


        string sql = @"SELECT * FROM users WHERE name = @name;";
        await using var connection = (NpgsqlConnection)await database.GetConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@name", name);

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
                    reader.GetString(4)
                    );

                userCache.Add(getUser.Name, getUser);

                return getUser;
            }

            return null;

        }
        catch (Exception e)
        {
            throw new Exception("Couldn't GET user. Exception: " + e.Message);
        }


    }

    public async Task<bool> UpdateUserAsync(int id, Dictionary<string, string> columnsValues)
    {
        await using var connection = (NpgsqlConnection)await database.GetConnectionAsync();
        await using var sqlTransaction = await connection.BeginTransactionAsync();

        try
        {
            foreach (KeyValuePair<string, string> entry in columnsValues)
            {
                string sql = @"UPDATE users SET @column = @value WHERE id = @id;";
                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.AddWithValue("@column", entry.Key);
                command.Parameters.AddWithValue("@value", entry.Value);
                command.Parameters.AddWithValue("@id", id);

                await command.ExecuteNonQueryAsync();
            }

            sqlTransaction.Commit();
            return true;

        }
        catch (Exception e)
        {
            await sqlTransaction.RollbackAsync();
            throw new Exception("Update failed, transaction rolled back. Exception: " + e.Message);
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
            await Shell.Current.DisplayAlert("Internal User Error", e.Message, "OK");
        }

        return false;
    }

    public async Task<bool> UserExistsAsync(string name)
    {
        await using var connection = (NpgsqlConnection)await database.GetConnectionAsync();
        string sql = @"SELECT * FROM users WHERE name = @name;";
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@name", name);

        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count) > 0;
    }


}