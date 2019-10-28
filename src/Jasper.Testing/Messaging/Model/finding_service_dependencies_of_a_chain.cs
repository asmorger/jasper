using System.Linq;
using Jasper.Configuration;
using Jasper.Messaging.Model;
using Lamar;
using NSubstitute;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging.Model
{
    public class finding_service_dependencies_of_a_chain
    {
        public class FakeHandler
        {
            public FakeHandler(IService3 three, IService5 five)
            {
            }

            public void Handle(Message1 message, IService1 one, IService2 two)
            {

            }
        }

        public interface IService1{}
        public interface IService2{}
        public interface IService3{}
        public interface IService4{}
        public interface IService5{}

        public class Service5 : IService5
        {
            public Service5(IService4 four)
            {
            }
        }

        private IContainer theContainer = new Container(x =>
        {
            x.For<IService1>().Use(Substitute.For<IService1>());
            x.For<IService2>().Use(Substitute.For<IService2>());
            x.For<IService3>().Use(Substitute.For<IService3>());
            x.For<IService4>().Use(Substitute.For<IService4>());

            x.For<IService5>().Use<Service5>();



        });

        private HandlerChain theChain = HandlerChain.For<FakeHandler>(x => x.Handle(null, null, null));

        [Fact]
        public void find_dependencies_in_parameter_list()
        {
            var dependencies = theChain.ServiceDependencies(theContainer).ToArray();

            dependencies.ShouldContain(typeof(IService1));
            dependencies.ShouldContain(typeof(IService2));
        }

        [Fact]
        public void find_dependencies_of_ctor()
        {
            var dependencies = theChain.ServiceDependencies(theContainer).ToArray();

            dependencies.ShouldContain(typeof(IService3));
            dependencies.ShouldContain(typeof(IService5));
        }

        [Fact]
        public void find_dependencies_deep()
        {
            var dependencies = theChain.ServiceDependencies(theContainer).ToArray();

            dependencies.ShouldContain(typeof(IService5));
        }
    }
}
