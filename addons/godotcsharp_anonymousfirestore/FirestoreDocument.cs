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
			Dictionary<string, object> Fields,
			[property:JsonPropertyName("name")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string Name = null,
			[property:JsonPropertyName("createTime")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string CreateTime = null,
			[property:JsonPropertyName("updateTime")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string UpdateTime = null);

	public record class StringFirestoreValue([property:JsonPropertyName("stringValue")]
			string value);
	public record class IntegerFirestoreValue([property:JsonPropertyName("integerValue")]
			string value);
	public record class DoubleFirestoreValue([property:JsonPropertyName("doubleValue")]
			double value);
	public record class BooleanFirestoreValue([property:JsonPropertyName("booleanValue")]
			bool value);
	public record class ArrayFirestoreValue([property:JsonPropertyName("arrayValue")]
			ArrayValuesFirestoreValue value);
	public record class ArrayValuesFirestoreValue([property:JsonPropertyName("values")]
			List<object> values);
	public record class MapFirestoreValue([property:JsonPropertyName("mapValue")]
			MapFieldsFirestoreValue value);
	public record class MapFieldsFirestoreValue(
		[property:JsonPropertyName("fields")]
			Dictionary<string, object> Fields);
}
