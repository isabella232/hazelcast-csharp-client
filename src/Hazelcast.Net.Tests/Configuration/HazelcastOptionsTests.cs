﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Configuration;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Networking;
using Hazelcast.Security;
using Hazelcast.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Hazelcast.Tests.Configuration
{
    [TestFixture]
    public class HazelcastOptionsTests
    {
        [Test]
        public void BuildExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => HazelcastOptionsBuilder.Build((Action<IConfigurationBuilder>) null));
            Assert.Throws<ArgumentNullException>(() => HazelcastOptionsBuilder.Build(null, null, null, "key"));
        }

        [Test]
        public void ServiceProvider()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var options = new HazelcastOptions { ServiceProvider = serviceProvider };
            Assert.That(options.ServiceProvider, Is.SameAs(serviceProvider));
        }

        [Test]
        public void EmptyOptionsFile()
        {
            var json = Resources.Empty;

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.SectionNameConstant, options);

            Assert.AreEqual("dev", options.ClusterName);
        }

        [Test]
        public void EmptyOptionsFileWithComments()
        {
            var json = Resources.EmptyWithComments;

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.SectionNameConstant, options);

            Assert.AreEqual("dev", options.ClusterName);
        }

        private static HazelcastOptions ReadResource(string json)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.SectionNameConstant, options);

            return options;
        }

        [Test]
        public void HazelcastOptionsRoot()
        {
            var options = ReadResource(Resources.HazelcastOptions);

            Assert.AreEqual("cluster", options.ClusterName);
            Assert.AreEqual("client", options.ClientName);
            Assert.AreEqual(2, options.Labels.Count);

            Assert.IsTrue(options.Labels.Contains("label_1"));
            Assert.IsTrue(options.Labels.Contains("label_2"));

            Assert.AreEqual(1, options.Subscribers.Count);
            var subscriber = options.Subscribers[0];
            Assert.IsInstanceOf<HazelcastClientEventSubscriber>(subscriber);

            TestSubscriber.Ctored = false;
            subscriber.Build(null);
            Assert.IsTrue(TestSubscriber.Ctored);

            var loadBalancer = options.LoadBalancer.Service;
            Assert.IsInstanceOf<RandomLoadBalancer>(loadBalancer);
        }

        [Test]
        public void CoreOptionsSection()
        {
            var options = ReadResource(Resources.HazelcastOptions).Core;

            Assert.AreEqual(1000, options.Clock.OffsetMilliseconds);
        }

        [Test]
        public void MessagingOptionsSection()
        {
            var options = ReadResource(Resources.HazelcastOptions).Messaging;

            // internal, cannot change
            Assert.AreEqual(5, options.MaxFastInvocationCount);

            Assert.AreEqual(1001, options.MinRetryDelayMilliseconds);
        }

        [Test]
        public void HeartbeatOptionsSection()
        {
            var options = ReadResource(Resources.HazelcastOptions).Heartbeat;

            Assert.AreEqual(1000, options.PeriodMilliseconds);
            Assert.AreEqual(1001, options.TimeoutMilliseconds);
        }

        [Test]
        public void PreviewOptionsSection()
        {
            var options = ReadResource(Resources.HazelcastOptions).Preview;

            Assert.That(options.EnableNewReconnectOptions, Is.False);
            Assert.That(options.EnableNewRetryOptions, Is.False);
        }

        [Test]
        public void NetworkingOptionsSection()
        {
            var options = ReadResource(Resources.HazelcastOptions).Networking;

            Assert.AreEqual(2, options.Addresses.Count);
            Assert.IsTrue(options.Addresses.Contains("localhost"));
            Assert.IsTrue(options.Addresses.Contains("otherhost"));
            Assert.IsFalse(options.ShuffleAddresses);
            Assert.IsFalse(options.SmartRouting);
            Assert.IsFalse(options.RedoOperations);
            Assert.AreEqual(ReconnectMode.DoNotReconnect, options.ReconnectMode);
            Assert.IsTrue(options.Reconnect);
            Assert.IsFalse(options.ShuffleAddresses);

            var sslOptions = options.Ssl;
            Assert.IsTrue(sslOptions.Enabled);
            Assert.IsFalse(sslOptions.ValidateCertificateChain);
            Assert.IsTrue(sslOptions.ValidateCertificateName);
            Assert.IsTrue(sslOptions.CheckCertificateRevocation);
            Assert.AreEqual("cert", sslOptions.CertificateName);
            Assert.AreEqual("path", sslOptions.CertificatePath);
            Assert.AreEqual("password", sslOptions.CertificatePassword);
            Assert.AreEqual(SslProtocols.Tls11, sslOptions.Protocol);
            Console.WriteLine(sslOptions.ToString());

#if NETCOREAPP
#pragma warning disable CS0618 // Type or member is obsolete
#endif
            // testing obsolete Ssl2, Default protocols
            Assert.Throws<ConfigurationException>(() => sslOptions.Protocol = SslProtocols.Ssl2);
            Assert.Throws<ConfigurationException>(() => sslOptions.Protocol = SslProtocols.Default);
#if NETCOREAPP
#pragma warning restore CS0618
#endif

            var cloudOptions = options.Cloud;
            Assert.IsTrue(cloudOptions.Enabled);
            Assert.AreEqual("token", cloudOptions.DiscoveryToken);

            // constant
            Assert.AreEqual(new Uri("https://coordinator.hazelcast.cloud/"), cloudOptions.Url);

            var socketOptions = options.Socket;
            Assert.AreEqual(1000, socketOptions.BufferSizeKiB);
            Assert.IsFalse(socketOptions.KeepAlive);
            Assert.AreEqual(1001, socketOptions.LingerSeconds);
            Assert.IsTrue(socketOptions.TcpNoDelay);

            var retryOptions = options.ConnectionRetry;
            Assert.AreEqual(1000, retryOptions.InitialBackoffMilliseconds);
            Assert.AreEqual(1001, retryOptions.MaxBackoffMilliseconds);
            Assert.AreEqual(1002, retryOptions.Multiplier);
            Assert.AreEqual(1003, retryOptions.ClusterConnectionTimeoutMilliseconds);
            Assert.AreEqual(1004, retryOptions.Jitter);
        }

        [Test]
        public void AuthenticationOptionsFile()
        {
            var options = ReadResource(Resources.HazelcastOptions).Authentication;

            var credentialsFactory = options.CredentialsFactory.Service;
            Assert.IsInstanceOf<TestCredentialsFactory>(credentialsFactory);

            var testCredentialsFactory = (TestCredentialsFactory)credentialsFactory;
            Assert.AreEqual("arg", testCredentialsFactory.Arg1);
            Assert.AreEqual(1000, testCredentialsFactory.Arg2);
        }

        [Test]
        public void AuthenticationUsernamePassword()
        {
            const string json = @"{ ""hazelcast"": {
""authentication"" : {
    ""username-password"": { ""username"": ""bob"", ""password"": ""secret"" }
}
}}";

            var options = ReadResource(json);
            var iCredsFactory = options.Authentication.CredentialsFactory.Service;

            Assert.That(iCredsFactory, Is.InstanceOf<UsernamePasswordCredentialsFactory>());
            var credsFactory = iCredsFactory as UsernamePasswordCredentialsFactory;
            Assert.That(credsFactory, Is.Not.Null);

            var iCreds = credsFactory.NewCredentials();

            Assert.That(iCreds, Is.InstanceOf<UsernamePasswordCredentials>());
            var creds = iCreds as UsernamePasswordCredentials;
            Assert.That(creds, Is.Not.Null);

            Assert.That(creds.Name, Is.EqualTo("bob"));
            Assert.That(creds.Password, Is.EqualTo("secret"));
        }

        [Test]
        public void AuthenticationToken()
        {
            const string json = @"{ ""hazelcast"": {
""authentication"" : {
    ""token"": { ""data"": ""some-secret-password"" }
}
}}";

            var options = ReadResource(json);
            var iCredsFactory = options.Authentication.CredentialsFactory.Service;

            Assert.That(iCredsFactory, Is.InstanceOf<TokenCredentialsFactory>());
            var credsFactory = iCredsFactory as TokenCredentialsFactory;
            Assert.That(credsFactory, Is.Not.Null);

            var iCreds = credsFactory.NewCredentials();

            Assert.That(iCreds, Is.InstanceOf<TokenCredentials>());
            var creds = iCreds as TokenCredentials;
            Assert.That(creds, Is.Not.Null);

            Assert.That(creds.Name, Is.EqualTo("<token>"));
            Assert.That(Encoding.UTF8.GetString(creds.GetToken()), Is.EqualTo("some-secret-password"));
        }

        [Test]
        public void AuthenticationKerberos()
        {
            const string json = @"{ ""hazelcast"": {
""authentication"" : {
    ""kerberos"": { ""spn"": ""service-provider-name"" }
}
}}";

            var options = ReadResource(json);
            var iCredsFactory = options.Authentication.CredentialsFactory.Service;

            Assert.That(iCredsFactory, Is.InstanceOf<KerberosCredentialsFactory>());
            var credsFactory = iCredsFactory as KerberosCredentialsFactory;
            Assert.That(credsFactory, Is.Not.Null);

            // do not test creating a token as that requires Kerberos
            // really, we just want to be sure that the factory has the SPN

            var spnField = credsFactory.GetType().GetField("_spn", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(spnField, Is.Not.Null);
            var spn = spnField.GetValue(credsFactory);

            Assert.That(spn, Is.EqualTo("service-provider-name"));
        }

        [Test]
        public void LoadBalancingOptions1()
        {
            const string json = @"{ ""hazelcast"": {
""loadBalancer"" : {
    ""typeName"": ""random""
}
}}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.SectionNameConstant, options);

            Assert.IsInstanceOf<RandomLoadBalancer>(options.LoadBalancer.Service);
        }

        [Test]
        public void LoadBalancingOptions2()
        {
            const string json = @"{ ""hazelcast"": {
""loadBalancer"" : {
    ""typeName"": ""ROUNDROBIN""
}
}}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.SectionNameConstant, options);

            Assert.IsInstanceOf<RoundRobinLoadBalancer>(options.LoadBalancer.Service);
        }

        [Test]
        public void LoadBalancingOptions3()
        {
            const string json = @"{ ""hazelcast"": {
""loadBalancer"" : {
    ""typeName"": ""Hazelcast.Clustering.LoadBalancing.RandomLoadBalancer, Hazelcast.Net""
}
}}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.SectionNameConstant, options);

            Assert.IsInstanceOf<RandomLoadBalancer>(options.LoadBalancer.Service);
        }

        [Test]
        public void Clone()
        {
            var options = ReadResource(Resources.HazelcastOptions);

            // TODO: find a way to ensure that *everything* is non-default

            options.Networking.Addresses.Add("127.0.0.1:11001");
            options.Networking.Addresses.Add("127.0.0.1:11002");
            options.Events.SubscriptionCollectDelay = TimeSpan.FromSeconds(4);
            options.Events.SubscriptionCollectPeriod = TimeSpan.FromSeconds(5);
            options.Events.SubscriptionCollectTimeout = TimeSpan.FromSeconds(6);

            // clone
            var clone = options.Clone();

            // perform a complete comparison of the clone
            AssertSameOptions(clone, options);
        }

        // this is a poor man's objects comparison
        // we used to depend on the ExpectedObjects library, but let's try to keep things simple
        // (see https://github.com/derekgreer/expectedObjects)
        private static void AssertSameOptions(object obj, object expected)
        {
            // fast path
            if (ReferenceEquals(obj, expected)) return;

            // objects must be of the same type
            Assert.That(obj.GetType() == expected.GetType());

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // compare public instance properties
            foreach (var property in properties)
            {
                // SingletonServiceFactory<>.OwnsService is not cloned
                if (property.Name == "OwnsService")
                {
                    Console.WriteLine($"Skipping non-cloned property {type.Name}::{property.Name}");
                    continue;
                }

                // I don't know how to handle indexed properties yet
                if (property.GetIndexParameters().Length > 0)
                    throw new NotSupportedException("Indexed properties are not supported here.");

                var oValue = property.GetValue(obj);
                var eValue = property.GetValue(expected);

                if (type == typeof (string)) // string is enumerable, must test first
                {
                    Assert.That(oValue, Is.EqualTo(eValue), $"{type}::{property.Name}");
                }
                else if (property.PropertyType.GetInterfaces().Contains(typeof (System.Collections.IEnumerable)))
                {
                    var oValues = ((System.Collections.IEnumerable) oValue).Cast<object>().ToList();
                    var eValues = ((System.Collections.IEnumerable) eValue).Cast<object>().ToList();
                    Assert.That(oValues.Count, Is.EqualTo(eValues.Count), $"{type}::{property.Name}");
                    for (var i = 0; i < oValues.Count; i++) AssertSameOptions(oValues[i], eValues[i]);
                }
                else if (property.PropertyType.IsValueType)
                {
                    Assert.That(oValue, Is.EqualTo(eValue), $"{type}::{property.Name}");
                }
                else
                {
                    AssertSameOptions(oValue, eValue);
                }
            }
        }

        [Test]
        public void AddSubscriber()
        {
            var options = new HazelcastOptions();

            options.AddSubscriber(new TestSubscriber());
            options.AddSubscriber("TestSubscriber");
            options.AddSubscriber(typeof(TestSubscriber));
            options.AddSubscriber<TestSubscriber>();
            options.AddSubscriber(x => x.StateChanged((sender, args) => { }));

            Assert.That(options.Subscribers.Count, Is.EqualTo(5));

            Assert.Throws<ArgumentNullException>(() => options.AddSubscriber((Type)null));
            Assert.Throws<ArgumentException>(() => options.AddSubscriber((string)null));
            Assert.Throws<ArgumentNullException>(() => options.AddSubscriber((IHazelcastClientEventSubscriber)null));
        }

        public class TestSubscriber : IHazelcastClientEventSubscriber
        {
            public static bool Ctored { get; set; }

            public void Build(HazelcastClientEventHandlers events)
            {
                Ctored = true;
            }
        }
        public class TestCredentialsFactory : ICredentialsFactory
        {
            public TestCredentialsFactory(string arg1, int arg2)
            {
                Arg1 = arg1;
                Arg2 = arg2;
            }

            public string Arg1 { get; }

            public int Arg2 { get; }

            public ICredentials NewCredentials()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            { }
        }

        [Test]
        public void SerializationOptionsFile()
        {
            var options = ReadResource(Resources.HazelcastOptions).Serialization;

            Assert.AreEqual(Endianness.LittleEndian, options.Endianness);
            Assert.AreEqual(1000, options.PortableVersion);
            Assert.IsFalse(options.ValidateClassDefinitions);

            Assert.AreEqual(1, options.PortableFactories.Count);
            var portableFactoryOptions = options.PortableFactories.First();
            Assert.AreEqual(1001, portableFactoryOptions.Id);
            Assert.IsInstanceOf<TestPortableFactory>(portableFactoryOptions.Service);

            Assert.AreEqual(1, options.DataSerializableFactories.Count);
            var dataSerializableFactoryOptions = options.DataSerializableFactories.First();
            Assert.AreEqual(1002, dataSerializableFactoryOptions.Id);
            Assert.IsInstanceOf<TestDataSerializableFactory>(dataSerializableFactoryOptions.Service);

            Assert.IsNotNull(options.GlobalSerializer);
            Assert.IsTrue(options.GlobalSerializer.OverrideClrSerialization);
            Assert.IsInstanceOf<TestDefaultSerializer>(options.GlobalSerializer.Service);

            Assert.AreEqual(1, options.Serializers.Count);
            var serializerOptions = options.Serializers.First();
            Assert.AreEqual(typeof(HazelcastClient), serializerOptions.SerializedType);
            Assert.IsInstanceOf<TestSerializer>(serializerOptions.Service);
        }

        [Test]
        public void NearCacheOptionsFile()
        {
            var options = ReadResource(Resources.HazelcastOptions);

            Assert.AreEqual(2, options.NearCaches.Count);

            Assert.IsTrue(options.NearCaches.TryGetValue("default", out var defaultNearCache));
            Assert.AreEqual(EvictionPolicy.Lru, defaultNearCache.EvictionPolicy);
            Assert.AreEqual(InMemoryFormat.Binary, defaultNearCache.InMemoryFormat);
            Assert.AreEqual(1000, defaultNearCache.MaxIdleSeconds);
            Assert.AreEqual(1001, defaultNearCache.MaxSize);
            Assert.AreEqual(1002, defaultNearCache.TimeToLiveSeconds);
            Assert.IsTrue(defaultNearCache.InvalidateOnChange);

            Assert.IsTrue(options.NearCaches.TryGetValue("other", out var otherNearCache));
            Assert.AreEqual(EvictionPolicy.Lfu, otherNearCache.EvictionPolicy);
            Assert.AreEqual(InMemoryFormat.Object, otherNearCache.InMemoryFormat);
            Assert.AreEqual(2000, otherNearCache.MaxIdleSeconds);
            Assert.AreEqual(2001, otherNearCache.MaxSize);
            Assert.AreEqual(2002, otherNearCache.TimeToLiveSeconds);
            Assert.IsFalse(otherNearCache.InvalidateOnChange);

            // TODO: whatever keys?
        }
     

        public class TestPortableFactory : IPortableFactory
        {
            public IPortable Create(int classId)
            {
                throw new NotSupportedException();
            }
        }

        public class TestDataSerializableFactory : IDataSerializableFactory
        {
            public IIdentifiedDataSerializable Create(int typeId)
            {
                throw new NotSupportedException();
            }
        }

        public class TestDefaultSerializer : ISerializer
        {
            public void Dispose()
            { }

            public int TypeId => throw new NotSupportedException();
        }

        public class TestSerializer : ISerializer
        {
            public void Dispose()
            { }

            public int TypeId => throw new NotSupportedException();
        }

        [Test]
        public void AltKey()
        {
            const string json1 = @"{
    ""hazelcast"": {
        ""clientName"": ""client"",
        ""clusterName"": ""cluster"",
        ""networking"": {
            ""addresses"": [
                ""127.0.0.1""
            ]
        }
    }
}";

            const string json2 = @"{
    ""alt"": {
        ""clientName"": ""altClient"",
        ""networking"": {
            ""addresses"": [
                ""127.0.0.2""
            ]
        }
    }
}";

            var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(json1));
            var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(json2));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream1);
            builder.AddJsonStream(stream2);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.SectionNameConstant, options);
            configuration.HzBind("alt", options);

            Assert.AreEqual("altClient", options.ClientName);
            Assert.AreEqual("cluster", options.ClusterName);

            Assert.That(options.Networking.Addresses.Count, Is.EqualTo(2));
            Assert.That(options.Networking.Addresses, Does.Contain("127.0.0.1"));
            Assert.That(options.Networking.Addresses, Does.Contain("127.0.0.2"));

            // or, more simply (only in tests):

            stream1 = new MemoryStream(Encoding.UTF8.GetBytes(json1));
            stream2 = new MemoryStream(Encoding.UTF8.GetBytes(json2));

            options = HazelcastOptionsBuilder.Build(x => x.AddJsonStream(stream1).AddJsonStream(stream2),
                null, null, "alt");

            Assert.AreEqual("altClient", options.ClientName);
            Assert.AreEqual("cluster", options.ClusterName);

            Assert.That(options.Networking.Addresses.Count, Is.EqualTo(2));
            Assert.That(options.Networking.Addresses, Does.Contain("127.0.0.1"));
            Assert.That(options.Networking.Addresses, Does.Contain("127.0.0.2"));
        }

  
    }
}
