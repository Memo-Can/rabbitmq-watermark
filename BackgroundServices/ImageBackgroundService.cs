using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Watermark.Services;


namespace Watermark.BackgroundServices;

public class ImageBackgroundService : BackgroundService
{
    private readonly RabbitMQService _rabbitMQService;
    private readonly ILogger<ImageBackgroundService> _logger;
    private IChannel _channel;

    public ImageBackgroundService(RabbitMQService rabbitMQService, ILogger<ImageBackgroundService> logger)
    {
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    public override async Task StartAsync (CancellationToken cancellationToken)
    {
        _channel = await _rabbitMQService.Connect();
        await _channel.BasicQosAsync(0,1,false);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += (model, e) =>
        {

            try
            {
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(e.Body.ToArray()));
                var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot/images",productImageCreatedEvent.ImageName);
            
                using var image = Image.Load(path);

                //DejaVu Sans for linux font.
                image.Mutate(x=>x.DrawText(
                    "memo-test",
                    SystemFonts.CreateFont("DejaVu Sans", 24, FontStyle.Bold),
                    Color.Black,
                    new PointF(image.Width-150,image.Height-40)
                ));

                image.Save("wwwroot/images/watermarks/"+ productImageCreatedEvent.ImageName);
                
                image.Dispose();

                _channel.BasicAckAsync(e.DeliveryTag,false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(RabbitMQService.queue,false,consumer);
    }
}
