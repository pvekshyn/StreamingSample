using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace SourceUserService
{
    public interface IUserRepository
    {
        Task BulkInsertUsers(IEnumerable<User> users);
    }

    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _dbContext;

        public UserRepository(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task BulkInsertUsers(IEnumerable<User> users)
        {
            var records = users.Select(MapToSqlDataRecord);
            var param = new SqlParameter("@tvp", SqlDbType.Structured)
            {
                TypeName = "dbo.UserType",
                Value = records
            };

            var sql = @"INSERT INTO [Users] (FirstName, LastName, Email, PhoneNumber) 
                    SELECT FirstName, LastName, Email, PhoneNumber FROM @tvp;";

            await _dbContext.Database.ExecuteSqlRawAsync(sql, param);
        }

        private static SqlDataRecord MapToSqlDataRecord(User user)
        {
            var record = new SqlDataRecord(
                new SqlMetaData("Id", SqlDbType.UniqueIdentifier),
                new SqlMetaData("FirstName", SqlDbType.VarChar, 100),
                new SqlMetaData("LastName", SqlDbType.VarChar, 100),
                new SqlMetaData("Email", SqlDbType.VarChar, 100),
                new SqlMetaData("PhoneNumber", SqlDbType.VarChar, 100)
            );

            record.SetSqlGuid(0, user.Id);
            record.SetString(1, user.FirstName);
            record.SetString(2, user.LastName);
            record.SetString(3, user.Email);
            record.SetString(4, user.PhoneNumber);

            return record;
        }
    }
}
