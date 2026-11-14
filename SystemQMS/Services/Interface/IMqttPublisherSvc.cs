using System.Threading.Tasks;

namespace SystemQMS.Services.Interface
{
    public interface IMqttPublisherSvc
    {
        Task PublishMessageAsync(string topic, object payload);
    }
}
