using AutoMapper;
using EzAspDotNet.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTest
{
    public class MapperTest
    {
        private class TestModel : EzAspDotNet.Protocols.CommonHeader
        {
            public int TestId { get; set; }
        }

        private class TestProtocol : EzMongoDb.Models.MongoDbHeader
        {
            public int TestId { get; set; }
        }

        private List<TestProtocol> _protocols = new() { new TestProtocol { TestId = 1 } };

        private List<TestProtocol> _emptyProtocols = new() { };

        [SetUp]
        public void Setup()
        {
            MapperUtil.Initialize(
                new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<TestModel, TestProtocol>();
                    cfg.CreateMap<TestProtocol, TestModel>();
                })
            );
        }

        [Test]
        public void Test()
        {
            var testModels = MapperUtil.Map<List<TestProtocol>, List<TestModel>>(_protocols);
            var testEmptyModel = MapperUtil.Map<List<TestProtocol>, List<TestModel>>(_emptyProtocols);
        }
    }
}