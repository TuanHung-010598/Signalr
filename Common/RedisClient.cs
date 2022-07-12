using StackExchange.Redis;

namespace ezCloud.SignalR.Common
{
    public class RedisClient
    {
        private IConfiguration _configuration;
        private ConnectionMultiplexer _redis;
        private IDatabase _database;
        public RedisClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _redis = ConnectionMultiplexer.Connect(_configuration["Redis:Connection"]);
            _database = _redis.GetDatabase();
        }

        public bool HashSet(string hash, string name, string value)
        {
            try
            {
                _database.HashSet(hash, new HashEntry[] { new HashEntry(name, value) });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string HashGet(string hash, string name)
        {
            return _database.HashGet(hash, name).ToString();
        }

        public bool HashDelete(string hash, string name)
        {
            try
            {
                if (_database.HashExists(hash, name))
                {
                    _database.HashDelete(hash, name);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
