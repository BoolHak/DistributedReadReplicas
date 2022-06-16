using Commun.Models;
using System.Collections.Concurrent;

namespace UserComsumerWebservice.InMemoryCache
{
    public class UsersCache
    {
        private static readonly ConcurrentDictionary<int, User> UsersById = new();
        private static readonly ConcurrentDictionary<string, int> UsersByName = new();
        private static readonly ConcurrentDictionary<string, int> UsersByNameAndPassword = new();


        public static void Remove(int id)
        {
            var user = Get(id);
            if (user == null) return;
            UsersById.TryRemove(user.Id, out _);
            UsersByName.TryRemove(user.Name!, out _);
            UsersByNameAndPassword.TryRemove($"{user.Name}-{user.Password}", out _);
        }
        public static bool TryAdd(User user)
        {
            bool added = true;
            added &= UsersById.TryAdd(user.Id, user);
            added &= UsersByName.TryAdd(user.Name!, user.Id);
            added &= UsersByNameAndPassword.TryAdd($"{user.Name}-{user.Password}", user.Id);
            return added;
        }

        public static bool Exists(int id)
        {
            return UsersById.ContainsKey(id);
        }

        public static bool Update(User user)
        {
            var existingUser = Get(user.Id);
            if (existingUser?.Version > user.Version) return true;
            Remove(user.Id);
            return TryAdd(user);
        }

        public static User? Get(string name, string password)
        {
            if (UsersByNameAndPassword.TryGetValue($"{name}-{password}", out int id))
                if (UsersById.TryGetValue(id, out User? user))
                    return user;

            return null;
        }

        public static User? Get(string name)
        {
            if (UsersByName.TryGetValue(name, out int id))
                if (UsersById.TryGetValue(id, out User? user))
                    return user;
            return null;
        }

        public static User? Get(int id)
        {
            if (UsersById.TryGetValue(id, out User? user)) return user;
            return null;
        }
    }
}
