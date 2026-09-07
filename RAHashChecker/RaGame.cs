using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RAHashChecker
{
	public class RaGame
	{
		[JsonPropertyName("Title")]
		public string Title { get; set; } = "";

		[JsonPropertyName("ID")]
		public int Id { get; set; }

		[JsonPropertyName("Hashes")]
		public List<string> Hashes { get; set; } = new List<string>();

		[JsonPropertyName("NumAchievements")]
		public int NumAchievements { get; set; }
	}

}
