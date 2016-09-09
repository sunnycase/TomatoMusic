using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Tomato.TomatoMusic.Core
{
    public static class AudioRpc
    {
        public const string RpcMessageTag = "Tomato.TomatoMusic.RpcCall";

        private const string MessageTag = "MessageTag";
        private const string MessageContent = "MessageContent";

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
}
