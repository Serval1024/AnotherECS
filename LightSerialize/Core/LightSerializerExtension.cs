using System.IO;
using UnityEngine;

namespace AnotherECS.Serializer
{
    public static partial class LightSerializerExtension
    {
        public static void WriteUTF8(this BinaryWriter stream, string[] @value)
        {
            var count = (ushort)@value.Length;
            stream.Write(count);
            for (int i = 0; i < count; ++i)
                stream.WriteUTF8(@value[i]);
        }

        public static string[] ReadUTF8Array(this BinaryReader stream)
        {
            var count = stream.ReadUInt16();
            var result = new string[count];
            for (int i = 0; i < count; ++i)
                result[i] = stream.ReadUTF8();

            return result;
        }

        public static void WriteUTF8(this BinaryWriter stream, string @value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(@value);
            stream.Write((byte)bytes.Length);
            stream.Write(bytes);
        }

        public static string ReadUTF8(this BinaryReader stream)
        {
            var count = stream.ReadByte();
            var bytes = stream.ReadBytes(count);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public static unsafe void Write(this BinaryWriter stream, byte* value, uint length)
        {
            for (int i = 0; i < length; ++i)
            {
                stream.Write(value[i]);
            }
        }

        public static unsafe void ReadBytePtr(this BinaryReader stream, byte* buffer, uint length)
        {
            for (int i = 0; i < length; ++i)
            {
                buffer[i] = stream.ReadByte();
            }
        }


        public static void Write(this BinaryWriter stream, string[] @value)
        {
            var count = (ushort)@value.Length;
            stream.Write(count);
            for (int i = 0; i < count; ++i)
                stream.Write(@value[i]);
        }

        public static string[] ReadStringArray(this BinaryReader stream)
        {
            var count = stream.ReadUInt16();
            var result = new string[count];
            for (int i = 0; i < count; ++i)
                result[i] = stream.ReadString();

            return result;
        }


        public static void Write(this BinaryWriter stream, float[] @value)
        {
            var count = (ushort)@value.Length;
            stream.Write(count);
            for (int i = 0; i < count; ++i)
                stream.Write(@value[i]);
        }

        public static float[] ReadSingleArray(this BinaryReader stream)
        {
            var count = stream.ReadUInt16();
            var result = new float[count];
            for (int i = 0; i < count; ++i)
                result[i] = stream.ReadSingle();

            return result;
        }

        public static void Write(this BinaryWriter stream, double[] @value)
        {
            var count = (ushort)@value.Length;
            stream.Write(count);
            for (int i = 0; i < count; ++i)
                stream.Write(@value[i]);
        }

        public static double[] ReadDoubleArray(this BinaryReader stream)
        {
            var count = stream.ReadUInt16();
            var result = new double[count];
            for (int i = 0; i < count; ++i)
                result[i] = stream.ReadDouble();

            return result;
        }

        public static void Write(this BinaryWriter stream, int[] @value)
        {
            var count = (ushort)@value.Length;
            stream.Write(count);
            for (int i = 0; i < count; ++i)
                stream.Write(@value[i]);
        }

        public static void Write(this BinaryWriter stream, ushort[] @value)
        {
            var count = (ushort)@value.Length;
            stream.Write(count);
            for (int i = 0; i < count; ++i)
                stream.Write(@value[i]);
        }

        public static ushort[] ReadUInt16Array(this BinaryReader stream)
        {
            var count = stream.ReadUInt16();
            var result = new ushort[count];
            for (int i = 0; i < count; ++i)
                result[i] = stream.ReadUInt16();

            return result;
        }

        public static int[] ReadIntArray(this BinaryReader stream)
        {
            var count = stream.ReadUInt16();
            var result = new int[count];
            for (int i = 0; i < count; ++i)
                result[i] = stream.ReadInt32();

            return result;
        }

        public static void Write(this BinaryWriter stream, uint[] @value)
        {
            var count = (ushort)@value.Length;
            stream.Write(count);
            for (int i = 0; i < count; ++i)
                stream.Write(@value[i]);
        }

        public static uint[] ReadUIntArray(this BinaryReader stream)
        {
            var count = stream.ReadUInt16();
            var result = new uint[count];
            for (int i = 0; i < count; ++i)
                result[i] = stream.ReadUInt32();

            return result;
        }

        public static byte[] ReadByteArray(this BinaryReader stream)
        {
            var count = stream.ReadUInt16();
            var result = new byte[count];
            for (int i = 0; i < count; ++i)
                result[i] = stream.ReadByte();

            return result;
        }

        public static void WriteArray(this BinaryWriter stream, byte[] @value)
        {
            var count = (ushort)@value.Length;
            stream.Write(count);
            for (int i = 0; i < count; ++i)
                stream.Write(@value[i]);
        }

        public static void WriteAsHalf(this BinaryWriter stream, float @value)
        {
            stream.Write(Mathf.FloatToHalf(@value));
        }

        public static void WriteAsHalf(this BinaryWriter stream, Vector2 @value)
        {
            stream.Write(Mathf.FloatToHalf(@value.x));
            stream.Write(Mathf.FloatToHalf(@value.y));
        }

        public static void Write(this BinaryWriter stream, Vector2 @value)
        {
            stream.Write(@value.x);
            stream.Write(@value.y);
        }

        public static void Write(this BinaryWriter stream, Vector3 @value)
        {
            stream.Write(@value.x);
            stream.Write(@value.y);
            stream.Write(@value.z);
        }

        public static void Write(this BinaryWriter stream, Vector2Int @value)
        {
            stream.Write(@value.x);
            stream.Write(@value.y);
        }

        public static void Write(this BinaryWriter stream, Vector3Int @value)
        {
            stream.Write(@value.x);
            stream.Write(@value.y);
            stream.Write(@value.z);
        }

        public static float ReadSingleAsHalf(this BinaryReader stream)
        {
            return Mathf.HalfToFloat(stream.ReadUInt16());
        }

        public static Vector2 ReadVector2AsHalf(this BinaryReader stream)
        {
            return new Vector2(Mathf.HalfToFloat(stream.ReadUInt16()), Mathf.HalfToFloat(stream.ReadUInt16()));
        }

        public static Vector2 ReadVector2(this BinaryReader stream)
        {
            return new Vector2(stream.ReadSingle(), stream.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader stream)
        {
            return new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
        }

        public static Vector2Int ReadVector2Int(this BinaryReader stream)
        {
            return new Vector2Int(stream.ReadInt32(), stream.ReadInt32());
        }

        public static Vector3Int ReadVector3Int(this BinaryReader stream)
        {
            return new Vector3Int(stream.ReadInt32(), stream.ReadInt32(), stream.ReadInt32());
        }
    }
}
