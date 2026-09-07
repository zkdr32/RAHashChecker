using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Text;

namespace RAHashChecker
{
	[AddINotifyPropertyChangedInterface]
	public class ExpectedVersion
	{
		public string SourceFileName { get; set; } = "";   // which of YOUR files this is for
		public string GameTitle { get; set; } = "";        // "Advance Wars"
		public int GameId { get; set; }
		public string ExpectedFileName { get; set; } = ""; // "Advance Wars (USA) (Rev 1).gba"
		public string Md5 { get; set; } = "";
		public string Labels { get; set; } = "";           // "nointro" etc.
		public string PatchUrl { get; set; } = "";         // empty if none
	}
}
