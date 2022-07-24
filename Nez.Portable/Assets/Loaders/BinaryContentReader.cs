using Microsoft.Xna.Framework.Content;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Nez
{
    internal class BinaryContentReader : ContentTypeReader<byte[]>
    {
        protected override byte[] Read(ContentReader input, byte[] existingInstance)
        {
            int length = input.ReadInt32();
            bool compressed = length < 0;
            byte[] data = input.ReadBytes(length);

            if (!compressed) return data;
            length *= -1;

            using (var msi = new MemoryStream(data))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return mso.ToArray();
            }
        }
    }

    internal class StringContentReader : ContentTypeReader<string>
    {
        protected override string Read(ContentReader input, string existingInstance)
        {
            bool compressed = input.ReadBoolean();
            int length = input.ReadInt32();
            byte[] data = input.ReadBytes(length);

            if (!compressed) return Encoding.UTF8.GetString(data);

            using (var msi = new MemoryStream(data))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}
