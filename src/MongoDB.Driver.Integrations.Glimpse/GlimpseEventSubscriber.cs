using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glimpse.Core.Extensions;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Message;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Integrations.Glimpse.Messages;

namespace MongoDB.Driver.Integrations.Glimpse
{
    public class GlimpseEventSubscriber : IEventSubscriber
    {
        private static IMessageBroker __messageBroker;
        private static Func<IExecutionTimer> __executionTimerFactory;

        internal static void Initialize(IMessageBroker messageBroker, Func<IExecutionTimer> executionTimerFactory)
        {
            __messageBroker = messageBroker;
            __executionTimerFactory = executionTimerFactory;
        }

        private readonly IEventSubscriber _eventSubscriber;

        public GlimpseEventSubscriber()
        {
            _eventSubscriber = new ReflectionEventSubscriber(this);
        }

        /// <inheritdoc />
        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            return _eventSubscriber.TryGetEventHandler<TEvent>(out handler);
        }

        public void Handle(ConnectionReceivedMessageEvent @event)
        {
            PointOnTimeline(string.Format("Message of size {0} sent.", @event.Length));
        }

        public void Handle(ConnectionSentMessagesEvent @event)
        {
            PointOnTimeline(string.Format("Message of size {0} sent.", @event.Length));
        }

        private static void PointOnTimeline(string message)
        {
            var timer = __executionTimerFactory();
            var result = timer.Point();
            Timeline(message, result);
        }

        private static void Timeline(string message, TimerResult result)
        {
            Publish(new MongoTimelineMessage()
                .AsTimelineMessage(message, MongoTimelineMessage.MongoDBTimelineCategory)
                .AsTimedMessage(result));
        }

        private static void Publish<T>(T message)
        {
            if(__messageBroker != null)
            {
                __messageBroker.Publish(message);
            }
        }
    }
}
