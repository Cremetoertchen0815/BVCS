#region License
//   Copyright 2016 Kastellanos Nikolaos
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
#endregion

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nez.ExtendedContent.GeonBit.Graphics;

namespace Nez.ExtendedContent.GeonBit.Serialization
{
    [ContentTypeWriter]
    internal class DynamicIndexBufferWriter : ContentTypeWriter<DynamicIndexBufferContent>
    {
        protected override void Write(ContentWriter output, DynamicIndexBufferContent buffer)
        {
            WriteIndexBuffer(output, buffer);

            output.Write(buffer.IsWriteOnly);

            return;
        }

        private static void WriteIndexBuffer(ContentWriter output, DynamicIndexBufferContent buffer)
        {
            // check if the buffer contains values greater than UInt16.MaxValue
            bool is16Bit = true;
            foreach (int index in buffer)
            {
                if (index > ushort.MaxValue)
                {
                    is16Bit = false;
                    break;
                }
            }

            int stride = (is16Bit) ? 2 : 4;

            output.Write(is16Bit); // Is 16 bit
            output.Write((uint)(buffer.Count * stride)); // Data size
            if (is16Bit)
            {
                foreach (int item in buffer)
                    output.Write((ushort)item);
            }
            else
            {
                foreach (int item in buffer)
                    output.Write(item);
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform) => "GeonBit.Graphics.Content.DynamicIndexBufferReader, Aether.Graphics";

    }
}
