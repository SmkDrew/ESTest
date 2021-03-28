﻿using Newtonsoft.Json;
using System;
using System.Globalization;

namespace ESTest.Model.Models
{
    public class JsonExponentialConverter : JsonConverter
    {
        public override bool CanRead { get { return true; } }
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            decimal amount = 0;
            if (decimal.TryParse(reader.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
            {
                return amount;
            }
            return amount;
        }
    }
}