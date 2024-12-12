using System;
﻿using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Watermark.Services;

public class RabbitMqService:IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IChannel _channel;
    public static string exchange ="direct-exchange";
    public static string routingKey ="watermark-route";
    public static string queue ="queue-watermark";
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(ConnectionFactory connectionFactory, ILogger<RabbitMqService> logger)
    {
        _connectionFactory=connectionFactory;
        _logger=logger;
        // Connect();
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
        await _channel.QueueBindAsync(exchange,queue,routingKey);

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
