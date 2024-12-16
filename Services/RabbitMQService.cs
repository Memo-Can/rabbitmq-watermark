using System;
﻿using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Watermark.Services;

public class RabbitMQService:IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IChannel _channel;
    public static string exchange ="direct-exchange-image";
    public static string routingKey ="watermark-route-image";
    public static string queue ="queue-watermark-image";
    private readonly ILogger<RabbitMQService> _logger;

    public RabbitMQService(ConnectionFactory connectionFactory, ILogger<RabbitMQService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public  async Task<IChannel> Connect()
    {
        _connection = await _connectionFactory.CreateConnectionAsync();

        //check if channel is  open return current channel
        if(_channel is { IsOpen:true }){
            return _channel;
        }
        
    
        _channel = await _connection.CreateChannelAsync();
        await _channel.ExchangeDeclareAsync(exchange,type: "direct",true,false);
        await _channel.QueueDeclareAsync(queue,true,false,false,null);
        await _channel.QueueBindAsync(queue,exchange,routingKey);

        _logger.LogInformation("RabbitMQ connection is ok");

        return _channel;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();

          _logger.LogInformation("RabbitMQ connection is disposed");
    }
}
