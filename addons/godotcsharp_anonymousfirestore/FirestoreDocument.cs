using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Firebase
{
	public record class FirestoreDocument(
			[property:JsonPropertyName("fields")]
			Dictionary<string, FirestoreValue> Fields,
			[property:JsonPropertyName("name")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string Name = null,
			[property:JsonPropertyName("createTime")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string CreateTime = null,
			[property:JsonPropertyName("updateTime")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string UpdateTime = null);

	public record class FirestoreValue(
		[property:JsonPropertyName("stringValue")]
		[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		string StringValue,
		[property:JsonPropertyName("integerValue")]
		[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		string IntegerValue,
		[property:JsonPropertyName("doubleValue")]
		[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		double? DoubleValue,
		[property:JsonPropertyName("booleanValue")]
		[property : JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
		bool? BooleanValue,
		[property:JsonPropertyName("arrayValue")]
		[property : JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		ArrayValuesFirestoreValue ArrayValue,
		[property:JsonPropertyName("mapValue")]
		[property : JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		MapFieldsFirestoreValue MapValue
		)
	{
		public FirestoreValue() : this(null, null, null, null, null, null) { }
		public FirestoreValue(string value) : this(value, null, null, null, null, null) { }
		public FirestoreValue(int value) : this(null, value.ToString(), null, null, null, null) { }
		public FirestoreValue(double value) : this(null, null, value, null, null, null) { }
		public FirestoreValue(bool value) : this(null, null, null, value, null, null) { }
		public FirestoreValue(ArrayValuesFirestoreValue value) : this(null, null, null, null, value, null) { }
		public FirestoreValue(MapFieldsFirestoreValue value) : this(null, null, null, null, null, value) { }
	}

	public record class ArrayValuesFirestoreValue([property:JsonPropertyName("values")]
			List<FirestoreValue> values);
	public record class MapFieldsFirestoreValue(
		[property:JsonPropertyName("fields")]
			Dictionary<string, FirestoreValue> Fields);
}
