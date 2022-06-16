using Commun;
using Commun.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserComsumerWebservice.InMemoryCache;

namespace UserComsumerWebservice.BackgroundServices
{
    public class MainLoopService: BackgroundService
    {
        private readonly HttpClient _syncHttpClient;
        private int sequenceNumber = 0;
        public MainLoopService(IHttpClientFactory httpClientFactory)
        {
            _syncHttpClient = httpClientFactory.CreateClient("SyncClient");
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var taskSync = Task.Run(async () =>
            {
                while (Config.State != "OK")
                {
                    try
                    {
                        await FullSyncUsers(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed Sync users");
                        Console.WriteLine(ex);
                        await Task.Delay(5000, stoppingToken);
                    }
                }
            }, stoppingToken);

            var connection = Commun.Constants.GetConnection();

            var channelUser = connection.CreateModel();
            channelUser.ExchangeDeclare(Commun.Constants.ExchangeName, ExchangeType.Topic);
            channelUser.QueueDeclare($"user-mico-{Config.MircoServiceId}", false, true, true);
            channelUser.QueueBind($"user-mico-{Config.MircoServiceId}", Commun.Constants.ExchangeName, Commun.Constants.KeyUser);

            var consumer = new EventingBasicConsumer(channelUser);
            consumer.Received += (sender, eventArgs) =>
            {
                var message = RBMessage<User>.GetObject(eventArgs.Body.ToArray());
                if (message == null) return;

                if (sequenceNumber > message!.SequenceNumber) return;
                sequenceNumber = message!.SequenceNumber;

                Console.WriteLine($"Sequence number: {sequenceNumber}");

                if (message.Models.Count > 0)
                {
                    foreach (var user in message.Models)
                    {
                        user.Version = sequenceNumber;

                        if (UsersCache.Exists(user.Id))
                        {

                            UsersCache.Update(user);
                            Console.WriteLine($"{user.Code} updated");
                        }
                        else
                        {
                            UsersCache.TryAdd(user);
                            Console.WriteLine($"{user.Code} created");
                        }
                    }
                }

                if (message.Deleted.Count > 0)
                {
                    foreach (var id in message.Deleted)
                    {
                        if (int.TryParse(id, out int userId))
                        {
                            UsersCache.Remove(userId);
                            Console.WriteLine($"{userId} deleted");
                        }
                    }
                }

            };

            channelUser.BasicConsume($"user-mico-{Config.MircoServiceId}", true, consumer);
            await taskSync;
            while (!stoppingToken.IsCancellationRequested) await Task.Delay(1000, stoppingToken);

        }

        private async Task FullSyncUsers(CancellationToken stoppingToken)
        {
            Console.WriteLine("Main loop started");

            var done = false;
            var page = 1;

            Config.State = "Sync";

            while (!done)
            {
                using var targetRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Commun.Constants.EventBrodcasterIp}/api/Users?page={page}&size=100");
                using var responseMessage = await _syncHttpClient.SendAsync(targetRequestMessage,
                        HttpCompletionOption.ResponseHeadersRead,
                        stoppingToken);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var resultString = await responseMessage.Content.ReadAsStringAsync(stoppingToken);
                    var result = JsonConvert.DeserializeObject<PagedResult<User>>(resultString);
                    if (result?.Data?.Count == 0)
                    {
                        done = true;
                        Config.State = "OK";
                    }
                    else
                    {
                        page++;
                        Console.WriteLine($"Page {page}");

                        var users = result?.Data;

                        if (users != null)
                        {
                            foreach (var user in users)
                                UsersCache.TryAdd(user);
                        }

                    }
                }
            }
        }
    }
}