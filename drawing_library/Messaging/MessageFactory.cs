using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace drawing.Messaging
{
    public static class MessageFactory
    {
        private static BinaryFormatter Formatter { get;}

        static MessageFactory()
        {
            Formatter = new BinaryFormatter();
        }

        public static byte[] Serialize (this object obj)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                Formatter.Serialize(memory, obj);
                memory.Flush();
                return memory.ToArray();
            }
        }

        public static E Deserialize<E>(byte[] data)
        {
            using (MemoryStream memory = new MemoryStream(data))
            {
                return (E) Formatter.Deserialize(memory);
            }
        }
    }
}
