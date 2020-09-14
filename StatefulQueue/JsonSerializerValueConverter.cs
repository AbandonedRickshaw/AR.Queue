using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace StatefulQueue
{
    public class JsonSerializerValueConverter<T>
    {
        public ValueConverter<T, string> ValueConverter => new ValueConverter<T, string>(
            obj => JsonSerializer.Serialize(obj, null),
            json => JsonSerializer.Deserialize<T>(json, null));
    }
}
