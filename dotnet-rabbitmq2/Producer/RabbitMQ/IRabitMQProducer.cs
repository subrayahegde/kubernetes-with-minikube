using System.Threading.Tasks;

namespace Producer.RabitMQ
{
    public interface IRabitMQProducer
    {
        Task SendProductMessage<T>(T message);
    }
}
