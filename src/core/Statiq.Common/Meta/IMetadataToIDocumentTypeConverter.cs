using System;
using System.ComponentModel;
using System.Globalization;

namespace Statiq.Common
{
    internal class IMetadataToIDocumentTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(IDocument);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            value is null || !(value is IMetadata metadata) || destinationType != typeof(IDocument) ? default : new Document(metadata);
    }
}
