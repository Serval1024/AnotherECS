using System.IO;
using System.IO.Compression;

namespace AnotherECS.Serializer
{
    internal static class CompressUtils
    {
        public static byte[] Compress(byte[] data, int offset)
        {
            var output = new MemoryStream();
            output.Write(data, 0, offset);
            using (var dstream = new DeflateStream(output, CompressionLevel.Fastest))
            {
                dstream.Write(data, offset, data.Length - offset);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data, int offset)
        {
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(new MemoryStream(data, offset, data.Length - offset), CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }
    }
}