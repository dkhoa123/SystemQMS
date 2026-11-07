using MQTTnet;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SystemQMS.Services.Interface;

public class MqttPublisherSvc : IMqttPublisherSvc
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _options;

    public MqttPublisherSvc(MqttClientFactory factory)
    {
        _mqttClient = factory.CreateMqttClient();

        _options = factory.CreateClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .WithClientId("QmsBackendPublisher_" + Guid.NewGuid().ToString()[..8])
            .Build();
    }

    public async Task PublishMessageAsync(string topic, object payload)
    {
        if (!_mqttClient.IsConnected)
        {
            await _mqttClient.ConnectAsync(_options, CancellationToken.None);
        }

        var jsonPayload = JsonSerializer.Serialize(payload);

        var factory = new MqttClientFactory();
        var message = factory.CreateApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(jsonPayload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message, CancellationToken.None);
    }
}

