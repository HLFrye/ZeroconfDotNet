using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZeroconfDotNet;
using System.Linq;
using Moq;

namespace ZeroconfServiceTests
{
    [TestClass]
    public class ServicePublisherTests
    {
        [TestMethod]
        public void ConstructPublisher()
        {
        }

        [TestMethod]
        public void ConstructPublisherWithMock()
        {

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
            //var mockListener = new Mock<IServiceListener>();
            //bool callbackReceived = false;
            //using (var service = new ServicePublisher(mockListener.Object))
            //{
            //    service.AddService("test", () =>
            //    {
            //        callbackReceived = true;
            //        return null;
            //    });
            //    service.Start();
            //    mockListener.Raise(x => x.FindService
            //    mockListener.RaiseFindServicesEvent("test").ToList();
            //}
            //Assert.IsTrue(callbackReceived);
        }

        [TestMethod]
        public void EventReturnsRegisteredInfo()
        {
            //var mockListener = new MockServiceListener();
            //var serviceInfo = new ServiceInfo();
            //using (var service = new ServicePublisher(mockListener))
            //{
            //    service.AddService("test", serviceInfo);
            //    service.Start();
            //    Assert.IsTrue(mockListener.RaiseFindServicesEvent("test").Single(x => x == serviceInfo) == serviceInfo);
            //}
        }
    }
}
