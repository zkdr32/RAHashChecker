using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text;

namespace RAHashChecker
{
	[AddINotifyPropertyChangedInterface]
	public class HashedRom
	{
		public string FilePath { get; set; }
		public string FileName { get; set; }
		public string RAHash { get; set; }
		public string MatchStatus { get; set; } = "";
		public string MatchedGame { get; set; } = "";
		public string ClosestMatch { get; set; } = "";
		public int ClosestMatchId { get; set; } = 0;

		public bool NeedsAttention
		{
			get
			{
				return MatchStatus == "Not in RA" ||
					MatchStatus == "Failed to hash";
			}
		}

		public HashedRom(string filePath, string raHash)
		{
			FilePath = filePath;
			FileName = Path.GetFileName(filePath);
			RAHash = raHash;
		}
	}
}