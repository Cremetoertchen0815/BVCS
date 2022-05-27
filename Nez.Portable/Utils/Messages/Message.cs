using System;

namespace Nez
{
    public class Message
    {
        //ctor
        public Message(string text, MessageSectionFormat[] formats, (string text, Telegram callback)[] answers)
        {
            Text = text;
            SectionFormatting = formats;
            Answers = answers;
        }

        public const string cVersion = "v1.0";

        /// <summary>
        /// The entirety of the to be displayed text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Describes the style and scroll speed of the different sections of text
        /// </summary>
        public MessageSectionFormat[] SectionFormatting { get; set; }

        /// <summary>
        /// Describes the speaking entity and it's emote. Default is a standard placeholder.
        /// </summary>
        public (string, string)? Speaker { get; set; } = ("missingno", "idle");

        /// <summary>
        /// Defines centain attributes of a message. Default is a non-blocking, fast-forwardable message, that requires a confirmation to continue.
        /// </summary>
        public MessageFlags DisplayFlags { get; set; } = MessageFlags.Default;

        /// <summary>
        /// Describes a list of selectable answers. If answer count is > 0, the AutoContinue flag and ConfirmReaction are being ignored.
        /// </summary>
        public (string text, Telegram callback)[] Answers { get; set; }

        /// <summary>
        /// Is being sent, if the message was auto-skipped, confirmed, the receiver is != string.Empty and there are no selectable answers.
        /// </summary>
        public Telegram? ConfirmReaction { get; set; } = null;

        /// <summary>
        /// Determines how long the dialman waits with automatically switching to the next message.
        /// </summary>
        public float AutoContinueDelay = 0.5F;
    }

    [Flags]
    public enum MessageFlags
    {
        Default = 0,
        BlockGameInput = 1,
        NoFastForward = 2,
        AutoContinue = 4
    }
}
