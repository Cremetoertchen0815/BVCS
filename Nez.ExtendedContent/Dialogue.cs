using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nez.ExtendedContent
{

    public struct Message
    {
        public const string cVersion = "v1.0";
        public string Text;
        public (bool Bold, float Speed, Color Color)[] SectionFormatting;
        public (string, string) Speaker;
        public int DisplayFlags;
        public (string AnswerText, string callbackReceiver, string callbackHead, string callbackBody)[] Answers;
        public (string Receiver, string Head, string Body)? ConfirmReaction;
        public float AutoContinueDelay;
    }

    [ContentImporter(".msgdb", CacheImportedData = false, DefaultProcessor = "MessageDBProcessor", DisplayName = "MessageDB Importer")]
	public class MessageDBImporter : ContentImporter<List<(List<Message>, string)>>
	{
		public override List<(List<Message>, string)> Import(string filename, ContentImporterContext context)
		{
            //Open file stream and binary reader
            var ret = new List<(List<Message>, string)>();
            using (var st = File.OpenRead(filename))
            {
                using (var br = new BinaryReader(st))
                {
                    //Read header
                    if (br.ReadString() != "BeaconMsgDB") throw new Exception("Invalid database file!");
                    if (br.ReadString() != Message.cVersion) throw new Exception("Incorrect version! (Importer version:" + Message.cVersion + ")");
                    //List sets
                    var cntA = br.ReadInt32();
                    for (int i = 0; i < cntA; i++)
                    {
                        //Load basic data/list actors
                        var elA = new List<Message>();
                        var name = br.ReadString();
                        var cntB = br.ReadInt32();
                        for (int j = 0; j < cntB; j++) br.ReadString(); //Read actor data, tho it's irrelevant to the game

                        //List messages
                        var cntC = br.ReadInt32();
                        for (int j = 0; j < cntC; j++)
                        {
                            var elC = new Message();
                            elC.Text = br.ReadString(); //Read text
                            elC.Speaker = br.ReadBoolean() ? (br.ReadString(), br.ReadString()) : ("missingno", "default"); //Read speaker

                            //Read sections formats
                            var cntD = br.ReadInt32();
                            var elD = new List<(bool Bold, float Speed, Color Color)>();
                            for (int k = 0; k < cntD; k++) elD.Add((br.ReadBoolean(), br.ReadSingle(), new Color(br.ReadByte(), br.ReadByte(), br.ReadByte())));
                            elC.SectionFormatting = elD.ToArray();
                            //Read answers
                            var cntE = br.ReadInt32();
                            var elE = new List<(string, string, string, string)>();
                            for (int k = 0; k < cntE; k++) elE.Add((br.ReadString(), br.ReadString(), br.ReadString(), br.ReadString()));
                            elC.Answers = elE.ToArray();
                            //Read confirm rection
                            elC.ConfirmReaction = br.ReadBoolean() ? ((string, string, string)?)(br.ReadString(), br.ReadString(), br.ReadString()) : null;
                            elC.DisplayFlags = br.ReadInt32();
                            elC.AutoContinueDelay = br.ReadSingle();

                            //Add to list
                            elA.Add(elC);
                        }

                        //Add set to DB
                        ret.Add((elA, name));
                    }
                    br.Close();
                }
            }

            return ret;
        }
    }

    [ContentProcessor(DisplayName = "MessageDB Processor")]
    public class MessageDBProcessor : ContentProcessor<List<(List<Message> SetData, string Name)>, List<(List<Message> SetData, string Name)>>
    {
        public override List<(List<Message> SetData, string Name)> Process(List<(List<Message> SetData, string Name)> input, ContentProcessorContext context) => input;
    }

    [ContentTypeWriter]
    public class MessageDBWriter : ContentTypeWriter<List<(List<Message> SetData, string Name)>>
    {
        public override string GetRuntimeReader(TargetPlatform targetPlatform) => "Nez.MessageDBReader, Nez";

        protected override void Write(ContentWriter output, List<(List<Message> SetData, string Name)> value)
        {
            output.Write(Message.cVersion);
            output.Write(value.Count);
            foreach (var element in value)
            {
                output.Write(element.Name);
                output.Write(element.SetData.Count);
                foreach (var elementB in element.SetData)
                {
                    output.Write(elementB.Text);
                    output.Write(elementB.SectionFormatting.Length);
                    foreach (var elementC in elementB.SectionFormatting)
                    {
                        output.Write(elementC.Bold);
                        output.Write(elementC.Speed);
                        output.Write(elementC.Color);
                    }
                    output.Write(elementB.Speaker.Item1);
                    output.Write(elementB.Speaker.Item2);
                    output.Write(elementB.Answers.Length);
                    foreach (var elementC in elementB.Answers)
                    {
                        output.Write(elementC.AnswerText);
                        output.Write(elementC.callbackReceiver);
                        output.Write(elementC.callbackHead);
                        output.Write(elementC.callbackBody);
                    }
                    output.Write(elementB.ConfirmReaction.HasValue);
                    if (elementB.ConfirmReaction != null)
                    {
                        output.Write(elementB.ConfirmReaction.Value.Receiver);
                        output.Write(elementB.ConfirmReaction.Value.Head);
                        output.Write(elementB.ConfirmReaction.Value.Body);
                    }
                    output.Write(elementB.AutoContinueDelay);
                    output.Write(elementB.DisplayFlags);
                }
            }
        }
    }
}
