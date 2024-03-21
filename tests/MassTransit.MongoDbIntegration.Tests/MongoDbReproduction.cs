namespace MassTransit.MongoDbIntegration.Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TestFramework.Messages;
    using Testing;
    using Testing.Implementations;


    [TestFixture]
    public class MongoDBOutboxIssueTests
    {
        [Test]
        public async Task Should_trigger_the_publish_observer_for_pong([Values(true, false)] bool useInMemoryOutbox)
        {
            await using var provider = CreateServiceProvider(useInMemoryOutbox);

            var harness = provider.GetTestHarness();

            await harness.Start();

            var bus = harness.Bus;
            var consumeObserver = new TestConsumeObserver(TimeSpan.FromSeconds(5), CancellationToken.None);
            var publishObserver = new TestPublishObserver(TimeSpan.FromSeconds(5), CancellationToken.None);

            bus.ConnectConsumeObserver(consumeObserver);
            bus.ConnectPublishObserver(publishObserver);

            await bus.Publish(new PingMessage());

            IPublishedMessage<PongMessage> context = publishObserver.Messages.Select<PongMessage>().First();

            await harness.Stop();
        }

        static ServiceProvider CreateServiceProvider(bool useInMemoryOutbox = false)
        {
            var services = new ServiceCollection();
            services
                .AddMassTransitTestHarness(x =>
                {
                    x.SetTestTimeouts(testInactivityTimeout: TimeSpan.FromSeconds(10));
                    if (useInMemoryOutbox)
                    {
                        x.AddInMemoryInboxOutbox();
                    }
                    else
                    {
                        x.AddMongoDbOutbox(r =>
                        {
                            r.Connection = "mongodb://127.0.0.1:27017";
                            r.DatabaseName = "consumeTest";
                        });
                    }

                    x.AddConsumer<PingConsumer>();

                    x.AddConfigureEndpointsCallback((sp, name, configurator) =>
                    {
                        if (useInMemoryOutbox)
                        {
                            configurator.UseInMemoryOutbox(sp);
                        }
                        else
                        {
                            configurator.UseMongoDbOutbox(sp);
                        }
                    });
                });

            services.AddOptions<TextWriterLoggerOptions>().Configure(options => options.Disable("Microsoft"));

            return services.BuildServiceProvider(true);
        }
    }

    class PingConsumer :
        IConsumer<PingMessage>
    {
        public async Task Consume(ConsumeContext<PingMessage> context)
        {
            await context.Publish(new PongMessage(context.Message.CorrelationId));
        }
    }
}
