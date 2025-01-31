using System;
using System.Data.Common;
using Finance.Models;

namespace Finance.Data.Interfaces;

public interface IAccountRepository
{
    Account? CurrentAccount { get; }
    void SetAccount(Guid? id);
    Task<bool> AddAccountAsync(Account account);
    Task<Account> GetAccountAsync(string name);
    Task<List<Account>> GetUserAccountsAsync();
    Task<bool> UpdateAccountAsync(Guid guid, string name);
    Task<bool> DeleteAccountAsync(Guid guid);
    Task<List<Account>?> ExecuteOperationAsync(Func<DbConnection, Task<List<Account>?>> operation);
}
