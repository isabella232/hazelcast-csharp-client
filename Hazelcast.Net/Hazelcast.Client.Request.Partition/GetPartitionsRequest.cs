using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Partition
{
    internal sealed class GetPartitionsRequest : ClientRequest, IRetryableRequest
    {
        public override int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public override int GetClassId()
        {
            return ClientPortableHook.GetPartitions;
        }

        public override void Write(IPortableWriter writer)
        {
        }


    }
}