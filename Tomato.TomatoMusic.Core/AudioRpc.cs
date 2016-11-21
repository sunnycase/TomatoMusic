using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Rpc.Core;
using Windows.Foundation.Collections;

namespace Tomato.TomatoMusic.Core
{
    public static class AudioRpc
    {
        public const string RpcMessageTagAudioController = "Tomato.TomatoMusic.RpcCall.AudioController";
        public const string RpcMessageTagAudioControllerHandler = "Tomato.TomatoMusic.RpcCall.AudioControllerHandler";

        private const string MessageTag = "MessageTag";
        private const string MessageContent = "MessageContent";

        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static readonly JsonSerializerSettings AnswerPacketJsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new[] { new UnknownObjectConverter() }
        };

        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

        public static void DestructMessage(ValueSet valueSet, out string tag, out string content)
        {
            object tagObj;
            valueSet.TryGetValue(AudioRpc.MessageTag, out tagObj);
            object contentObj;
            valueSet.TryGetValue(AudioRpc.MessageContent, out contentObj);

            tag = tagObj as string ?? string.Empty;
            content = contentObj as string ?? string.Empty;
        }

        public static ValueSet ConstructMessage(string tag, string content)
        {
            var valueSet = new ValueSet();
            valueSet.Add(MessageTag, tag);
            valueSet.Add(MessageContent, content);

            return valueSet;
        }
    }

    class UnknownObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RpcAnswerPacket);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var theValue = (RpcAnswerPacket)value;
            JObject jo = new JObject();
            jo["CallId"] = theValue.CallId;
            jo["ReturnType"] = theValue.Return == null ? null : JToken.FromObject(theValue.Return?.GetType(), serializer);
            jo["Return"] = theValue.Return == null ? null : JToken.FromObject(theValue.Return, serializer);
            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var returnType = jo["ReturnType"].ToObject<Type>(serializer);
            return new RpcAnswerPacket
            {
                CallId = (int)jo["CallId"],
                Return = jo["Return"].ToObject(returnType, serializer)
            };
        }
    }
}
