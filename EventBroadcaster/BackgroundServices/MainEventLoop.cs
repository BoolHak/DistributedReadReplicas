using Commun;
using Commun.Models;
using EventBroadcaster.Entities;
using EventBroadcaster.LocalDb;
using LiteDB;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Text;

namespace EventBroadcaster.BackgroundServices
{
    public class MainEventLoop : BackgroundService
    {

        private readonly Dictionary<string, object> args = new Dictionary<string, object>();
        private readonly IServiceScopeFactory _scopeFactory;
        private const int SleepTime = 500;

        public MainEventLoop(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            args.Add("x-message-ttl", 10000);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var connection = Commun.Constants.GetConnection();

            var channelUser = connection.CreateModel();
            channelUser.ExchangeDeclare(Commun.Constants.ExchangeName, ExchangeType.Topic);
            channelUser.QueueDeclare(Commun.Constants.QueueNameUser, true, false, true, args);
            channelUser.QueueBind(Commun.Constants.QueueNameUser, Commun.Constants.ExchangeName, Commun.Constants.KeyUser);

            using (var scope = _scopeFactory.CreateScope())
            {
                using var dbContext = scope.ServiceProvider.GetRequiredService<DataDbContext>();
                var lastItem = await dbContext.EventLog.OrderBy(m => m.Id).LastOrDefaultAsync(stoppingToken);
                int lastIndex = 0;
                int previousIndex = 0;

                if(lastItem != null) lastIndex = lastItem.Id;

                var userIndex = new Dictionary<int, string>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    userIndex.Clear();

                    var changes = await dbContext.EventLog.Where(m => m.Id > lastIndex).AsNoTracking().ToListAsync(stoppingToken);
                    if (changes.Any()) lastIndex = changes.OrderBy(m => m.Id).Last().Id;

                    if (changes.Count == 0 || previousIndex == lastIndex)
                    {
                        await Task.Delay(SleepTime, stoppingToken);
                        continue;
                    }

                    foreach (var change in changes.OrderByDescending(m => m.Id))
                    {
                        switch (change.TableName)
                        {
                            case Tables.User:

                                if (int.TryParse(change.TableId, out int userId))
                                {
                                    userIndex.TryAdd(userId, change.EventType);
                                }

                                break;
                            default:
                                break;

                        }
                    }

                    await CollectUsers(dbContext, userIndex, channelUser, stoppingToken);

                    previousIndex = lastIndex;

                    await Task.Delay(SleepTime, stoppingToken);
                }


            } 
        }

        private static async Task CollectUsers(DataDbContext dbContext, Dictionary<int, string> userIndex, IModel mqChannel, CancellationToken stoppingToken)
        {
            if (userIndex.Any())
            {
                var userSequenceNumber = await dbContext.SequenceNumber.Where(m => m.TableName == Tables.User).FirstOrDefaultAsync(stoppingToken);

                if (userSequenceNumber == null)
                {
                    userSequenceNumber = new SequenceNumber
                    {
                        CurrentIndex = 0,
                        TableName = Tables.User,
                        TimeStamp = DateTime.UtcNow
                    };

                    await dbContext.SequenceNumber.AddAsync(userSequenceNumber, stoppingToken);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }


                var listModified = new List<int>();
                var listDeleted = new List<int>();

                foreach (var userChange in userIndex)
                {
                    switch (userChange.Value)
                    {
                        case "update":
                            listModified.Add(userChange.Key);
                            break;

                        case "insert":
                            listModified.Add(userChange.Key);
                            break;

                        case "delete":
                            listDeleted.Add(userChange.Key);
                            break;

                        default:
                            break;
                    }
                }

                var haveUsersChanges = false;
                var users = new List<User>();
                if (listModified.Any())
                {
                    users = await dbContext.User.AsNoTracking().Where(m => listModified.Contains(m.Id)).ToListAsync(stoppingToken);
                    foreach (var user in users)
                        Console.WriteLine($"{user.Id} => {user.Code}");
                    haveUsersChanges = true;
                }

                if (listDeleted.Any())
                {
                    haveUsersChanges = true;
                }

                if (haveUsersChanges)
                {
                    var userMessage = new RBMessage<User>();

                    userMessage.SequenceNumber = userSequenceNumber.CurrentIndex;
                    userMessage.Models = users;
                    userMessage.Deleted = listDeleted.Select(m=> $"{m}").ToList();
                    var payload = userMessage.GetData();
                    mqChannel.BasicPublish(Commun.Constants.ExchangeName, Commun.Constants.KeyUser, null, payload);
                    SaveChangesInLocalDb(userSequenceNumber, payload);
                }

                userSequenceNumber.CurrentIndex++;
                userSequenceNumber.TimeStamp = DateTime.UtcNow;
                dbContext.SequenceNumber.Update(userSequenceNumber);
                await dbContext.SaveChangesAsync(stoppingToken); 

            }
        }

        private static void SaveChangesInLocalDb(SequenceNumber sequenceNumber, byte[] payload)
        {
            using (var db = new LiteDatabase(@"Filename=C:\Temp\LocalCache.db;connection=shared"))
            {
                var col = db.GetCollection<Sequences>("sequences");
                var sequence = new Sequences
                {
                    Id = ObjectId.NewObjectId(),
                    TableName = sequenceNumber.TableName,
                    Value = Encoding.UTF8.GetString(payload),
                    Version = sequenceNumber.CurrentIndex
                };

                col.Insert(sequence);
                col.EnsureIndex(x => x.TableName);
                col.EnsureIndex(x => x.Version);
            }
        }
    }
}
