using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZeroconfDotNet;
using System.Linq;

namespace ZeroconfServiceTests
{
    [TestClass]
    public class ServicePublisherTests
    {
        [TestMethod]
        public void ConstructPublisher()
        {
            using (var service = new ServicePublisher())
            {
                service.Start();
            }            
        }

        [TestMethod]
        public void ConstructPublisherWithMock()
        {
            using (var service = new ServicePublisher(new MockServiceListener()))
            {
                service.Start();
            }
        }

        [TestMethod]
        public void StartingPublisherCallsStartListener()
        {
            var mockListener = new Moq.Mock<IServiceListener>();
            mockListener.Setup(x => x.Start()).Verifiable();
            using (var service = new ServicePublisher(mockListener.Object))
            {
                service.Start();
            }
            mockListener.VerifyAll();
        }

        [TestMethod]
        public void EventCallsCallback()
        {
            var mockListener = new MockServiceListener();
            bool callbackReceived = false;
            using (var service = new ServicePublisher(mockListener))
            {
                service.AddService("test", () =>
                {
                    callbackReceived = true;
                    return null;
                });
                service.Start();
                mockListener.RaiseFindServicesEvent("test").ToList();
            }
            Assert.IsTrue(callbackReceived);
        }

        [TestMethod]
        public void EventReturnsRegisteredInfo()
        {
            var mockListener = new MockServiceListener();
            var serviceInfo = new ServiceInfo();
            using (var service = new ServicePublisher(mockListener))
            {
                service.AddService("test", serviceInfo);
                service.Start();
                Assert.IsTrue(mockListener.RaiseFindServicesEvent("test").Single(x => x == serviceInfo) == serviceInfo);
            }
        }
    }
}
