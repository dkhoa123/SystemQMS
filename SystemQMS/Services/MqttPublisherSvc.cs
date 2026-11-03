/*using MQTTnet;
using System.Text.Json;
using SystemQMS.Services.Interface;
using System.Threading.Tasks;

public class MqttPublisherSvc : IMqttPublisherSvc
{
    private readonly IMqttClient _mqttClient;

    public MqttPublisherSvc()
    {
        // 1. Tạo factory (cú pháp v5)
        var factory = new MQTTnet.MqttFactory();
        // 2. Tạo client
        _mqttClient = factory.CreateMqttClient();

        // 3. Cấu hình kết nối (Sử dụng cú pháp mới của v5.2)
        var options = factory.CreateClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .WithClientId("QmsBackendPublisher")
            .Build();

        // 4. Kết nối đồng bộ (tạm thời)
        _mqttClient.ConnectAsync(options).Wait();
    }

    public async Task PublishMessageAsync(string topic, object payload)
    {
        if (!_mqttClient.IsConnected) return;

        var jsonPayload = JsonSerializer.Serialize(payload);

        var factory = new MqttFactory();
        var message = factory.CreateApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(jsonPayload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message);
    }
}
*/