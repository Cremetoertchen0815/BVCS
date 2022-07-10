#region License
/// -------------------------------------------------------------------------------------
/// Notice: This file had been edited to integrate as core inside GeonBit.
/// Original license and attributes below. The license and copyright notice below affect
/// this file and this file only. https://github.com/tainicom/Aether.Extras
/// -------------------------------------------------------------------------------------
//   Copyright 2011-2016 Kastellanos Nikolaos
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
using Nez.ExtendedContent.GeonBit.Animation;
using System.Collections.Generic;

namespace Nez.ExtendedContent.GeonBit.Serialization
{
    [ContentTypeWriter]
    internal class AnimationsDataWriter : ContentTypeWriter<AnimationsContent>
    {
        protected override void Write(ContentWriter output, AnimationsContent value)
        {
            WriteClips(output, value.Clips);
            WriteBindPose(output, value.BindPose);
            WriteInvBindPose(output, value.InvBindPose);
            WriteSkeletonHierarchy(output, value.SkeletonHierarchy);
            WriteBoneNames(output, value.BoneNames);
        }

        private void WriteClips(ContentWriter output, Dictionary<string, ClipContent> clips)
        {
            int count = clips.Count;
            output.Write(count);

            foreach (var clip in clips)
            {
                output.Write(clip.Key);
                output.WriteObject<ClipContent>(clip.Value);
            }

            return;
        }

        private void WriteBindPose(ContentWriter output, List<Microsoft.Xna.Framework.Matrix> bindPoses)
        {
            int count = bindPoses.Count;
            output.Write(count);

            for (int i = 0; i < count; i++)
                output.Write(bindPoses[i]);

            return;
        }

        private void WriteInvBindPose(ContentWriter output, List<Microsoft.Xna.Framework.Matrix> invBindPoses)
        {
            int count = invBindPoses.Count;
            output.Write(count);

            for (int i = 0; i < count; i++)
                output.Write(invBindPoses[i]);

            return;
        }

        private void WriteSkeletonHierarchy(ContentWriter output, List<int> skeletonHierarchy)
        {
            int count = skeletonHierarchy.Count;
            output.Write(count);

            for (int i = 0; i < count; i++)
                output.Write(skeletonHierarchy[i]);

            return;
        }

        private void WriteBoneNames(ContentWriter output, List<string> boneNames)
        {
            int count = boneNames.Count;
            output.Write(count);

            for (int boneIndex = 0; boneIndex < count; boneIndex++)
            {
                string boneName = boneNames[boneIndex];
                output.Write(boneName);
            }

            return;
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform) => "GeonBit.Extend.Animation.Animations, " +
                typeof(Nez.ExtendedContent.GeonBit.Animation.Animations).Assembly.FullName;

        public override string GetRuntimeReader(TargetPlatform targetPlatform) => "GeonBit.Extend.Animation.Content.AnimationsReader, " +
                typeof(Nez.ExtendedContent.GeonBit.Content.AnimationsReader).Assembly.FullName;
    }

}
