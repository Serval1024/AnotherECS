using System.IO;
using System.IO.Compression;

namespace AnotherECS.Serializer
{
    internal static class CompressUtils
    {
        public static byte[] Compress(byte[] data)
        {
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(output, CompressionLevel.Fastest))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }
    }
}