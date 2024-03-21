namespace MassTransit.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using MassTransit.Testing;
    using MassTransit.Testing.Implementations;
    using NUnit.Framework;
    using TestFramework;
    using TestFramework.Messages;


    [TestFixture]
    public class Consume_Observer_Responding_Tests :
        InMemoryTestFixture
    {
        [Test]
        public async Task Should_trigger_the_consume_observer_for_ping()
        {
            await Bus.Publish(new PingMessage());
            IReceivedMessage<PingMessage> context = _consumeObserver.Messages.Select<PingMessage>().First();
        }

        [Test]
        public async Task Should_trigger_the_publish_observer_for_pong()
        {
            await Bus.Publish(new PingMessage());
            IPublishedMessage<PongMessage> context = _publishObserver.Messages.Select<PongMessage>().First();
        }

        TestConsumeObserver _consumeObserver;
        TestPublishObserver _publishObserver;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _consumeObserver = GetConsumeObserver();
            Bus.ConnectConsumeObserver(_consumeObserver);

            _publishObserver = GetPublishObserver();
            Bus.ConnectPublishObserver(_publishObserver);
        }

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            configurator.Consumer<RespondingConsumer>();
        }


        class RespondingConsumer :
            IConsumer<PingMessage>
        {
            public async Task Consume(ConsumeContext<PingMessage> context)
            {
                await context.RespondAsync(new PongMessage(context.Message.CorrelationId));
            }
        }
    }

    [TestFixture]
    public class Consume_Observer_Publishing_Tests :
        InMemoryTestFixture
    {
        [Test]
        public async Task Should_trigger_the_consume_observer_for_ping()
        {
            await Bus.Publish(new PingMessage());
            IReceivedMessage<PingMessage> context = _consumeObserver.Messages.Select<PingMessage>().First();
        }

        [Test]
        public async Task Should_trigger_the_publish_observer_for_pong()
        {
            await Bus.Publish(new PingMessage());
            IPublishedMessage<PongMessage> context = _publishObserver.Messages.Select<PongMessage>().First();
        }

        TestConsumeObserver _consumeObserver;
        TestPublishObserver _publishObserver;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _consumeObserver = GetConsumeObserver();
            Bus.ConnectConsumeObserver(_consumeObserver);

            _publishObserver = GetPublishObserver();
            Bus.ConnectPublishObserver(_publishObserver);
        }

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            configurator.Consumer<PublishingConsumer>();
        }


        class PublishingConsumer :
            IConsumer<PingMessage>
        {
            public async Task Consume(ConsumeContext<PingMessage> context)
            {
                await context.Publish(new PongMessage(context.Message.CorrelationId));
            }
        }
    }
}
