using System.Text.Json;
using Confluent.Kafka;
using UserService.Models;
using UserService.Services;

namespace UserService.Utils;

public class KafkaConsumer
{
    private readonly IConsumer<string, string> _consumer;
    private readonly string _topic;

    public KafkaConsumer(IConfiguration configuration)
    {
        var kafkaSettings = configuration.GetSection("Kafka");
        var bootstrapServers = kafkaSettings["BootstrapServers"];
        var groupId = kafkaSettings["GroupId"];
        _topic = kafkaSettings["TopicForListen"];

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    public void StartListening(Action<string, string> handleMessage)
    {
        _consumer.Subscribe(_topic);

        Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    var result = _consumer.Consume();
                    var value = result.Value;
                    var message = JsonSerializer.Deserialize<MessageModel>(value);

                    if (message != null)
                    {
                        handleMessage(message.Id, message.UserId);
                    }
                    else
                    {
                        handleMessage("null", "null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Kafka] Ошибка обработки сообщения: {ex.Message}");
                }
            }
        });
    }
}