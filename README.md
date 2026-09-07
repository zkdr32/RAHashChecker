# RAHashChecker

Release download [here](https://github.com/zkdr32/RAHashChecker/releases)

A Windows desktop tool for checking whether your ROMs are compatible with [RetroAchievements](https://retroachievements.org) (RA), and for tidying up the ones that aren't. Uses RAHasher to hash files, so this is basically a fancy UI wrapper for that with some added features.

Point it at a folder of ROMs, pick the console, and it hashes every file the way RA expects, then tells you which games are supported, which have achievements, and which don't match anything in RA.

## What it does

- **Hashes your ROMs using RA's rules.** It shells out to `RAHasher.exe` to compute the canonical hash RA would use for each file. This is per-console (disc systems like PS1 aren't a plain file hash), which is why the external hasher is required.
- **Matches against RA's database.** It pulls the full game + hash list for the selected console and checks each ROM's hash against it. Each file is reported as having achievements, being found but with no achievements, not being in RA, or failing to hash.
- **Guesses the game for non-matches.** When a hash doesn't match, it tries to resolve the ROM to a RA game by title. If it recognizes the game, that tells you the game is supported and your particular dump isn't the accepted one.
- **Lists the accepted versions.** For recognized-but-unmatched games, it can fetch every ROM version RA accepts for that game (names, MD5s, labels, patch URLs) so you can see which dump you actually need.
- **Moves rejects out of the way.** Right-click a result to move it (and its associated files — a `.cue` and its `.bin` tracks, or a single `.chd`) into an `Extras` folder, keeping your main library to just the ROMs that matter.

## Requirements

- Windows with .NET (WPF app)
- [`RAHasher.exe`](https://github.com/RetroAchievements/RALibretro) — set its location on first run
- A RetroAchievements account (username + web API key), entered in the app

## Getting started

1. Launch the app and set the path to `RAHasher.exe`.
2. Enter your RetroAchievements username and API key. (Find your API key on your RA account settings page.)
3. Select the console for the ROMs you're checking.
4. Drag a folder or files onto the drop zone, or use the folder picker.
5. Click **Hash Files + Search RetroAchievements** to hash and match against RA.
6. For non-matches, click **Find Expected Filename** to see which versions RA accepts.
7. Right-click any result to move it and its files into `Extras`.

## How matching works

The hash match is **exact** — your file's RA hash either is or isn't in RA's set for that console. There's no fuzzy matching on the hash itself.

The "game found, wrong version" hint is a **title-based guess**. It strips region/revision tags (e.g. `(USA)`, `(Rev 1)`) and looks for a RA game with that title. So a ROM whose hash doesn't match but whose title resolves means: RA supports this game, but not the specific dump you have. The **Find Expected Files** list shows the versions RA does accept.

Note: the title guess only fires when the name resolves cleanly against RA's list. If the title doesn't line up (subtitle punctuation, word order, a game RA lists under a different name), you'll get no hint even though the game may be supported — use the search button to look it up on the RA site.

## Notes

- The RA API is rate-limited. Checking a library makes one request; looking up expected files makes one request per distinct unmatched game, so a large pile of non-matches issues more requests.
- `Extras` is a local staging folder this tool creates. Nothing reads from it afterward — it's a one-way "get this out of my main set" move.

## License

_TODO_
