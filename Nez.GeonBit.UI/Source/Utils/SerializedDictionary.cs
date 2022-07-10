#region File Description
//-----------------------------------------------------------------------------
// A simple serializeable dictionary.
// From: https://weblogs.asp.net/pwelter34/444961
//
// Author: Ronen Ness.
// Since: 2018.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Nez.GeonBit.UI.Utils
{
    /// <summary>
    /// A plain dictionary that support XML serialization.
    /// </summary>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Dictionary value type.</typeparam>
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {

        #region IXmlSerializable Members
        /// <summary>
        /// Get schema.
        /// </summary>
        /// <returns>Always null.</returns>
        public System.Xml.Schema.XmlSchema GetSchema() => null;

        /// <summary>
        /// Read XML.
        /// </summary>
        /// <param name="reader">XML reader.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                var key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");
                var value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
                Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        /// <summary>
        /// Write to xml.
        /// </summary>
        /// <param name="writer">XML writer.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (var key in Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                var value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
