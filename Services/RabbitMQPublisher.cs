using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Watermark.Services;

public class RabbitMQPublisher
{
    private readonly RabbitMQService _rabbitMQService;

    public RabbitMQPublisher(RabbitMQService rabbitMQService)
    {
        _rabbitMQService = rabbitMQService;
    }

    public  async void Publish(ProductImageCreatedEvent productImageCreatedEvent)
    {
        var channel = await _rabbitMQService.Connect();

        var body  = JsonSerializer.Serialize(productImageCreatedEvent);
        var bodyByte = Encoding.UTF8.GetBytes(body);

        await channel.BasicPublishAsync(RabbitMQService.exchange, RabbitMQService.routingKey,true, bodyByte);
    }
}
