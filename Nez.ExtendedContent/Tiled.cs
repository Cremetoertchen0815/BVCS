using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using System.Text;

namespace Nez.ExtendedContent
{
    [ContentImporter(".tmx", DisplayName = "Tiled Level Importer", DefaultProcessor = "BinaryGZipProcessor")]
    public class TiledImporter : ContentImporter<byte[]>
    {
        public override byte[] Import(string filename, ContentImporterContext context) => Encoding.UTF8.GetBytes(File.ReadAllText(filename).Replace(".png", "").Replace(".tsx", ""));
    }


}
