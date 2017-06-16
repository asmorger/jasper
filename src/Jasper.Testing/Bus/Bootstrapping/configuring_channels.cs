﻿using System.Linq;
using Jasper.Bus.Runtime.Routing;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using StructureMap.TypeRules;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class configuring_channels : BootstrappingContext
    {
        [Fact]
        public void listen_for_messages_on_a_channel_positive()
        {
            theRegistry.ListenForMessagesFrom(Uri1);

            // Send-only channel
            theRegistry.SendMessage<Message1>().To(Uri2);

            ShouldBeBooleanExtensions.ShouldBeTrue(theChannels[Uri1].Incoming);
            ShouldBeBooleanExtensions.ShouldBeFalse(theChannels[Uri2].Incoming);
        }

        [Fact]
        public void place_a_specific_type_routing_rule_on_a_channel()
        {
            theRegistry.SendMessage<Message1>().To(Uri2);

            theChannels[Uri2].Rules.Single().ShouldBeOfType<SingleTypeRoutingRule<Message1>>();
        }

        [Fact]
        public void configure_messages_in_namespace()
        {
            theRegistry.SendMessagesInNamespace("Foo")
                .To(Uri1);

            theRegistry.SendMessagesInNamespaceContaining<Message1>()
                .To(Uri1);

            theChannels[Uri1].Rules.OfType<NamespaceRule>().Select(x => x.Namespace)
                .ShouldHaveTheSameElementsAs("Foo", typeof(Message1).Namespace);
        }

        [Fact]
        public void configure_assembly_routing_rules()
        {
            theRegistry.SendMessagesFromAssembly(typeof(NewUser).GetAssembly())
                .To(Uri1);

            theRegistry.SendMessagesFromAssemblyContaining<Message1>()
                .To(Uri2);

            theChannels[Uri1].Rules.Single()
                .ShouldBeOfType<AssemblyRule>()
                .Assembly.ShouldBe(typeof(NewUser).GetAssembly());

            theChannels[Uri2].Rules.Single()
                .ShouldBeOfType<AssemblyRule>()
                .Assembly.ShouldBe(typeof(Message1).GetAssembly());
        }

    }
}