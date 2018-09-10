using Microsoft.VisualStudio.TestTools.UnitTesting;
using DCAdam;
using Contracts;
using CommunicationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationService.Tests
{
    [TestClass()]
    public class CommServiceTests
    {
#if DEBUG
        [TestMethod()]
        public void IsParamAllowedTest()
        {
            ICommunicationModule commModule = new AdamMock();
            var _server = new CommService(commModule);
            _server._currentCommand = new CommandData();
            _server._currentCommand.Type = CommandTypes.ArtNo;

            Assert.IsTrue(_server.IsParamAllowed(48));
            Assert.IsTrue(_server.IsParamAllowed(49));
            Assert.IsTrue(_server.IsParamAllowed(50));
            Assert.IsTrue(_server.IsParamAllowed(51));
            Assert.IsTrue(_server.IsParamAllowed(52));
            Assert.IsTrue(_server.IsParamAllowed(53));
            Assert.IsTrue(_server.IsParamAllowed(54));
            Assert.IsTrue(_server.IsParamAllowed(55));
            Assert.IsTrue(_server.IsParamAllowed(54));
            Assert.IsTrue(_server.IsParamAllowed(57));
            Assert.IsFalse(_server.IsParamAllowed(58));
            Assert.IsFalse(_server.IsParamAllowed(255));
            Assert.IsTrue(_server.IsParamAllowed(3));
        }

#endif
    }
}