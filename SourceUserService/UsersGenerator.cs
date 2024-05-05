using Bogus;

namespace SourceUserService
{
    public static class UsersGenerator
    {
        public static IEnumerable<User> GenerateUsers(int count)
        {
            var testUsers = new Faker<User>()
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.PhoneNumber, (f, u) => f.Phone.PhoneNumber());

            return testUsers.GenerateLazy(count);
        }
    }
}
