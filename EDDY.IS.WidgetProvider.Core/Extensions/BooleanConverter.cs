using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core.Extensions
{
    public class BooleanConverter : JsonConverter<bool>
    {
        public override bool Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            bool.Parse(reader.GetString());

        public override void Write(
            Utf8JsonWriter writer,
            bool b,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(b.ToString());
    }
}
