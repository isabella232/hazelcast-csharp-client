using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;


namespace Hazelcast.Client.Request.Executor
{
	
    //[System.Serializable]
    //public abstract class LocalTargetCallableRequest : IIdentifiedDataSerializable
    //{
        //private string name;

        //private Callable callable;

        //public LocalTargetCallableRequest()
        //{
        //}

        //public LocalTargetCallableRequest(string name, Callable callable)
        //{
        //    this.name = name;
        //    this.callable = callable;
        //}

        //public int GetFactoryId()
        //{
        //    return ExecutorDataSerializerHook.FId;
        //}

        //public int GetId()
        //{
        //    return ExecutorDataSerializerHook.LocalTargetCallableRequest;
        //}

        ///// <exception cref="System.IO.IOException"></exception>
        //public void WriteData(IObjectDataOutput output)
        //{
        //    output.WriteUTF(name);
        //    output.WriteObject(callable);
        //}

        ///// <exception cref="System.IO.IOException"></exception>
        //public void ReadData(IObjectDataInput input)
        //{
        //    name = input.ReadUTF();
        //    callable = input.ReadObject();
        //}
    //}
}