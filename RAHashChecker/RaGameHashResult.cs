using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RAHashChecker
{
	public class RaGameHashResult
	{
		[JsonPropertyName("Results")]
		public List<RaHashEntry> Results { get; set; } = new List<RaHashEntry>();
	}

	public class RaHashEntry
	{
		[JsonPropertyName("MD5")]
		public string Md5 { get; set; } = "";

		[JsonPropertyName("Name")]
		public string Name { get; set; } = "";

		[JsonPropertyName("Labels")]
		public List<string> Labels { get; set; } = new List<string>();

		[JsonPropertyName("PatchUrl")]
		public string? PatchUrl { get; set; }
	}
}