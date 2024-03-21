namespace MassTransit.Testing.Implementations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class TestPublishObserver :
        IPublishObserver
    {
        readonly PublishedMessageList _messages;

        public TestPublishObserver(TimeSpan timeout, CancellationToken inactivityToken)
        {
            _messages = new PublishedMessageList(timeout, inactivityToken);
        }

        public IPublishedMessageList Messages => _messages;

        Task IPublishObserver.PrePublish<T>(PublishContext<T> context)
        {
            return Task.CompletedTask;
        }

        Task IPublishObserver.PostPublish<T>(PublishContext<T> context)
        {
            _messages.Add(context);

            return Task.CompletedTask;
        }

        Task IPublishObserver.PublishFault<T>(PublishContext<T> context, Exception exception)
        {
            _messages.Add(context, exception);

            return Task.CompletedTask;
        }
    }
}
