using System;
using Newtonsoft.Json;
using UnityEngine;

public class ColorConverter : JsonConverter
{
	public override bool CanRead
	{
		get
		{
			return false;
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		Color color = (Color)value;
		writer.WriteStartObject();
		writer.WritePropertyName("a");
		writer.WriteValue(color.a);
		writer.WritePropertyName("r");
		writer.WriteValue(color.r);
		writer.WritePropertyName("g");
		writer.WriteValue(color.g);
		writer.WritePropertyName("b");
		writer.WriteValue(color.b);
		writer.WriteEndObject();
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Color);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
	}
}
