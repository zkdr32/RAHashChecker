using System;
using System.Collections.Generic;
using System.Text;

namespace RAHashChecker
{
	public class RaSystem
	{
		public string Name { get; set; }
		public int ConsoleId { get; set; }
		public string[] ValidExtensions { get; set; }

		// Archive formats work for every system, so they're shared.
		private static readonly string[] ArchiveExtensions = { ".zip", ".7z", ".rar" };

		public RaSystem(string name, int consoleId, string[] validExtensions)
		{
			Name = name;
			ConsoleId = consoleId;

			// Add the archive extensions to the list, as they're valid for all systems
			ValidExtensions = validExtensions
				.Concat(ArchiveExtensions)
				.ToArray();
		}

		public override string ToString() => Name;

		public string ValidExtensionsSingleLine
		{
			get
			{
				return "(" + string.Join(" | ", ValidExtensions) + ")";
			}
		}

		public static List<RaSystem> Systems { get; } = new List<RaSystem>();

		static RaSystem()
		{
			Systems.Add(new RaSystem("Atari - 2600", 25, new[] { ".a26", ".bin" }));
			Systems.Add(new RaSystem("Atari - 7800", 51, new[] { ".a78" }));
			Systems.Add(new RaSystem("Atari - Jaguar", 17, new[] { ".j64", ".jag" }));
			Systems.Add(new RaSystem("Atari - Jaguar CD", 77, new[] { ".chd", ".cue" }));
			Systems.Add(new RaSystem("Atari - Lynx", 13, new[] { ".lnx" }));
			Systems.Add(new RaSystem("Atari - 5200", 50, new[] { ".a52", ".bin" }));
			Systems.Add(new RaSystem("Atari - ST", 36, new[] { ".st", ".stx", ".msa", ".dim" }));

			Systems.Add(new RaSystem("NEC - PC-8000/8800", 47, new[] { ".d88", ".cmt", ".t88" }));
			Systems.Add(new RaSystem("NEC - PC Engine/TurboGrafx-16", 8, new[] { ".pce" }));
			Systems.Add(new RaSystem("NEC - PC Engine CD/TurboGrafx-CD", 76, new[] { ".chd", ".cue" }));
			Systems.Add(new RaSystem("NEC - PC-FX", 49, new[] { ".chd", ".cue" }));
			Systems.Add(new RaSystem("NEC - PC-6000", 67, new[] { ".p6", ".cas" }));
			Systems.Add(new RaSystem("NEC - PC-9800", 48, new[] { ".d88", ".dup", ".hdi" }));

			Systems.Add(new RaSystem("Nintendo - Nintendo Entertainment System", 7, new[] { ".nes" }));
			Systems.Add(new RaSystem("Nintendo - Famicom Disk System", 81, new[] { ".fds" }));
			Systems.Add(new RaSystem("Nintendo - Super Nintendo Entertainment System", 3, new[] { ".sfc", ".smc", ".swc", ".fig" }));
			Systems.Add(new RaSystem("Nintendo - Nintendo 64", 2, new[] { ".n64", ".z64", ".v64" }));
			Systems.Add(new RaSystem("Nintendo - GameCube", 16, new[] { ".iso", ".gcm", ".rvz", ".ciso" }));
			Systems.Add(new RaSystem("Nintendo - Wii", 19, new[] { ".iso", ".wbfs", ".rvz", ".wad" }));
			Systems.Add(new RaSystem("Nintendo - Game Boy", 4, new[] { ".gb", ".gic" }));
			Systems.Add(new RaSystem("Nintendo - Game Boy Color", 6, new[] { ".gbc" }));
			Systems.Add(new RaSystem("Nintendo - Game Boy Advance", 5, new[] { ".gba" }));
			Systems.Add(new RaSystem("Nintendo - Nintendo DS", 18, new[] { ".nds" }));
			Systems.Add(new RaSystem("Nintendo - Nintendo DSi", 78, new[] { ".nds" }));
			Systems.Add(new RaSystem("Nintendo - Pokemon Mini", 24, new[] { ".min" }));
			Systems.Add(new RaSystem("Nintendo - Nintendo 3DS", 62, new[] { ".3ds", ".cia", ".cci", ".cxi" }));
			Systems.Add(new RaSystem("Nintendo - Virtual Boy", 28, new[] { ".vb", ".vboy" }));
			Systems.Add(new RaSystem("Nintendo - Game & Watch", 60, new[] { ".mgw" }));
			Systems.Add(new RaSystem("Nintendo - Wii U", 20, new[] { ".wua", ".rpx", ".wud", ".wux" }));

			Systems.Add(new RaSystem("Sega - SG-1000", 33, new[] { ".sg" }));
			Systems.Add(new RaSystem("Sega - Master System", 11, new[] { ".sms" }));
			Systems.Add(new RaSystem("Sega - Genesis/Mega Drive", 1, new[] { ".md", ".gen", ".bin", ".smd" }));
			Systems.Add(new RaSystem("Sega - Sega CD", 9, new[] { ".chd", ".cue" }));
			Systems.Add(new RaSystem("Sega - 32X", 10, new[] { ".32x", ".bin" }));
			Systems.Add(new RaSystem("Sega - Saturn", 39, new[] { ".chd", ".cue" }));
			Systems.Add(new RaSystem("Sega - Dreamcast", 40, new[] { ".chd", ".gdi", ".cdi" }));
			Systems.Add(new RaSystem("Sega - Game Gear", 15, new[] { ".gg" }));
			Systems.Add(new RaSystem("Sega - Pico", 68, new[] { ".md", ".bin" }));

			Systems.Add(new RaSystem("SNK - Neo Geo CD", 56, new[] { ".chd", ".cue" }));
			Systems.Add(new RaSystem("SNK - Neo Geo Pocket", 14, new[] { ".ngp", ".ngc" }));

			Systems.Add(new RaSystem("Sony - PlayStation", 12, new[] { ".chd", ".cue" }));
			Systems.Add(new RaSystem("Sony - PlayStation 2", 21, new[] { ".chd", ".iso" }));
			Systems.Add(new RaSystem("Sony - PlayStation Portable", 41, new[] { ".chd", ".iso", ".cso", ".pbp" }));

			Systems.Add(new RaSystem("Others - 3DO Interactive Multiplayer", 43, new[] { ".chd", ".cue", ".iso" }));
			Systems.Add(new RaSystem("Others - Amstrad CPC", 37, new[] { ".dsk", ".sna", ".cdt", ".cpr" }));
			Systems.Add(new RaSystem("Others - Apple II", 38, new[] { ".dsk", ".nib", ".do", ".po" }));
			Systems.Add(new RaSystem("Others - Arcade", 27, new[] { ".zip", ".chd" }));
			Systems.Add(new RaSystem("Others - Arcadia 2001", 73, new[] { ".bin" }));
			Systems.Add(new RaSystem("Others - Arduboy", 71, new[] { ".hex", ".arduboy" }));
			Systems.Add(new RaSystem("Others - ColecoVision", 44, new[] { ".col" }));
			Systems.Add(new RaSystem("Others - Elektor TV Games Computer", 75, new[] { ".bin", ".pgm" }));
			Systems.Add(new RaSystem("Others - Fairchild Channel F", 57, new[] { ".chf", ".bin" }));
			Systems.Add(new RaSystem("Others - Intellivision", 45, new[] { ".int", ".bin" }));
			Systems.Add(new RaSystem("Others - Interton VC 4000", 74, new[] { ".bin" }));
			Systems.Add(new RaSystem("Others - Magnavox Odyssey 2", 23, new[] { ".bin" }));
			Systems.Add(new RaSystem("Others - Mega Duck", 69, new[] { ".bin" }));
			Systems.Add(new RaSystem("Others - MSX", 29, new[] { ".rom", ".dsk", ".mx1", ".mx2" }));
			Systems.Add(new RaSystem("Others - Uzebox", 80, new[] { ".uze" }));
			Systems.Add(new RaSystem("Others - Vectrex", 46, new[] { ".vec", ".bin" }));
			Systems.Add(new RaSystem("Others - WASM-4", 72, new[] { ".wasm" }));
			Systems.Add(new RaSystem("Others - Watara Supervision", 63, new[] { ".sv", ".bin" }));
			Systems.Add(new RaSystem("Others - WonderSwan", 53, new[] { ".ws", ".wsc" }));
		}
	}
}