﻿using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using JasperBus.Configuration;
using JasperBus.Runtime.Subscriptions;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Runtime.Subscriptions
{

    public class When_creating_grouped_subscriptions
    {
        public readonly BusSettings theSettings = new BusSettings
        {
            Upstream = new Uri("memory://upstream"),
            Incoming = new Uri("memory://incoming")
        };
        private readonly ChannelGraph theGraph;
        private readonly IEnumerable<Subscription> theSubscriptions;

        public When_creating_grouped_subscriptions()
        {
            theGraph = new ChannelGraph { Name = "FooNode" };

            var requirement = new GroupSubscriptionRequirement(theSettings.Upstream, theSettings.Incoming);
            requirement.AddType(typeof(FooMessage));
            requirement.AddType(typeof(BarMessage));

            theSubscriptions = requirement.Determine(theGraph);
        }

        [Fact]
        public void should_set_the_receiver_uri_to_the_explicitly_chosen_uri()
        {
            theSubscriptions.First().Receiver
                .ShouldBe(theSettings.Incoming);
        }

        [Fact]
        public void sets_the_node_name_from_the_channel_graph()
        {
            theSubscriptions.Select(x => x.NodeName).Distinct()
                .Single().ShouldBe(theGraph.Name);
        }

        [Fact]
        public void should_set_the_source_uri_to_the_requested_source_from_settings()
        {
            theSubscriptions.First().Source
                .ShouldBe(theSettings.Upstream);
        }

        [Fact]
        public void should_add_a_subscription_for_each_type()
        {
            theSubscriptions.Select(x => x.MessageType)
                .ShouldHaveTheSameElementsAs(typeof(FooMessage).GetFullName(), typeof(BarMessage).GetFullName());
        }

        public class BusSettings
        {
            public Uri Outbound { get; set; }
            public Uri Incoming { get; set; }
            public Uri Upstream { get; set; }
        }

        public class FooMessage { }
        public class BarMessage { }
    }
}