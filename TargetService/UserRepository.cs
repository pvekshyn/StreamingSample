using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace TargetService
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAllUsers();
        Task BulkInsertUsers(IEnumerable<User> users);
        Task BulkSyncUsers(IEnumerable<UserAction> userActions);
    }

    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _dbContext;

        public UserRepository(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _dbContext.Users.OrderBy(x => x.Id).AsNoTracking();
        }

        public async Task BulkInsertUsers(IEnumerable<User> users)
        {
            var records = users.Select(MapUserToSqlDataRecord);
            var param = new SqlParameter("@tvp", SqlDbType.Structured)
            {
                TypeName = "dbo.UserType",
                Value = records
            };

            var sql = @"INSERT INTO [Users] (Id, FirstName, LastName, Email) 
                    SELECT Id, FirstName, LastName, Email FROM @tvp;";

            await _dbContext.Database.ExecuteSqlRawAsync(sql, param);
        }

        public async Task BulkSyncUsers(IEnumerable<UserAction> userActions)
        {
            var records = userActions.Select(MapUserActionToSqlDataRecord);

            var param = new SqlParameter("@tvp", SqlDbType.Structured)
            {
                TypeName = "dbo.UserActionType",
                Value = records
            };

            var sql = @"INSERT INTO [Users] (Id, FirstName, LastName, Email) 
                SELECT Id, FirstName, LastName, Email FROM @tvp WHERE Action = 0;

                DELETE FROM [Users]
                WHERE Id IN(SELECT Id FROM @tvp WHERE Action = 1)";

            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                connection.Open();
                using (var tran = connection.BeginTransaction())
                using (var command = new SqlCommand(sql, connection, tran))
                {
                    command.Parameters.Add(param);
                    try
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                    tran.Commit();
                }
            }
        }

        private static SqlDataRecord MapUserToSqlDataRecord(User user)
        {
            var record = new SqlDataRecord(
                new SqlMetaData("Id", SqlDbType.UniqueIdentifier),
                new SqlMetaData("FirstName", SqlDbType.VarChar, 100),
                new SqlMetaData("LastName", SqlDbType.VarChar, 100),
                new SqlMetaData("Email", SqlDbType.VarChar, 100)
            );

            record.SetSqlGuid(0, user.Id);
            record.SetString(1, user.FirstName);
            record.SetString(2, user.LastName);
            record.SetString(3, user.Email);

            return record;
        }

        private static SqlDataRecord MapUserActionToSqlDataRecord(UserAction userAction)
        {
            var record = new SqlDataRecord(
                new SqlMetaData("Id", SqlDbType.UniqueIdentifier),
                new SqlMetaData("FirstName", SqlDbType.VarChar, 100),
                new SqlMetaData("LastName", SqlDbType.VarChar, 100),
                new SqlMetaData("Email", SqlDbType.VarChar, 100),
                new SqlMetaData("Action", SqlDbType.Int)
            );

            record.SetSqlGuid(0, userAction.User.Id);
            record.SetString(1, userAction.User.FirstName);
            record.SetString(2, userAction.User.LastName);
            record.SetString(3, userAction.User.Email);
            record.SetInt32(4, (int)userAction.Action);

            return record;
        }
    }
}
