﻿using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;
using Jasper.Testing.Bus;
using Jasper.Testing.Bus.Runtime;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class registering_and_discovering_custom_readers_and_writers : IntegrationContext
    {
        private SerializationGraph theSerialization;

        public registering_and_discovering_custom_readers_and_writers()
        {
            withAllDefaults();

            theSerialization = Runtime.Container.GetInstance<SerializationGraph>();
        }

        [Fact]
        public void scans_for_custom_writers_in_the_app_assembly()
        {
            theSerialization.WriterFor(typeof(Message5)).ContentTypes
                .ShouldHaveTheSameElementsAs("application/json", "green", "blue");
        }

        [Fact]
        public void scans_for_custom_readers_in_the_app_assembly()
        {
            theSerialization.ReaderFor(typeof(Message1).ToTypeAlias())
                .ContentTypes.ShouldContain("green");
        }

        [Fact]
        public void can_override_json_serialization_for_a_mesage()
        {
            // Not overridden, so it should be the default
            theSerialization.WriterFor(typeof(Message1))["application/json"]
                .ShouldBeOfType<NewtonsoftJsonWriter<Message1>>();

            // Overridden
            theSerialization.WriterFor(typeof(OverriddenJsonMessage))["application/json"]
                .ShouldBeOfType<OverrideJsonWriter>();
        }

        [Fact]
        public void can_override_json_serialization_reader_for_a_message_type()
        {
            // Not overridden, so it should be the default
            theSerialization.ReaderFor(typeof(Message4).ToTypeAlias())["application/json"]
                .ShouldBeOfType<NewtonsoftJsonReader<Message4>>();

            // Overridden
            theSerialization.ReaderFor(typeof(OverriddenJsonMessage).ToTypeAlias())["application/json"]
                .ShouldBeOfType<OverrideJsonReader>();
        }
    }

    public class OverriddenJsonMessage{}

    public class OverrideJsonWriter : IMediaWriter
    {
        public Type DotNetType { get; } = typeof(OverriddenJsonMessage);
        public string ContentType { get; } = "application/json";
        public byte[] Write(object model)
        {
            throw new NotImplementedException();
        }

        public Task Write(object model, Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class OverrideJsonReader : IMediaReader
    {
        public string MessageType { get; } = typeof(OverriddenJsonMessage).ToTypeAlias();
        public Type DotNetType { get; } = typeof(OverriddenJsonMessage);
        public string ContentType { get; } = "application/json";
        public object Read(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<T> Read<T>(Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class GreenMessage1Reader : IMediaReader
    {
        public string MessageType { get; } = typeof(Message1).ToTypeAlias();
        public Type DotNetType { get; } = typeof(Message1);
        public string ContentType { get; } = "green";
        public object Read(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<T> Read<T>(Stream stream)
        {
            throw new NotImplementedException();
        }
    }


    public class GreenMessage1Writer : IMediaWriter
    {
        public Type DotNetType { get; } = typeof(Message5);
        public string ContentType { get; } = "green";
        public byte[] Write(object model)
        {
            throw new NotImplementedException();
        }

        public Task Write(object model, Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class BlueMessage1Writer : IMediaWriter
    {
        public Type DotNetType { get; } = typeof(Message5);
        public string ContentType { get; } = "blue";
        public byte[] Write(object model)
        {
            throw new NotImplementedException();
        }

        public Task Write(object model, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}