using Microsoft.Win32;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static RAHashChecker.HashTools;

namespace RAHashChecker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class MainWindow : Window
	{
		private string _raHasherLocation = Properties.Settings.Default.RAHasherLocation;
		public string RAHasherPath
		{
			get
			{
				return _raHasherLocation;
			}

			set
			{
				Properties.Settings.Default.RAHasherLocation = value;
				_raHasherLocation = value;
				Properties.Settings.Default.Save();
			}
		}

		private string _raUsername = Properties.Settings.Default.RAUsername;
		public string RAUsername
		{
			get
			{
				return _raUsername;
			}

			set
			{
				Properties.Settings.Default.RAUsername = value;
				_raUsername = value;
				Properties.Settings.Default.Save();
			}
		}

		private string _apiKey = Properties.Settings.Default.RAAPIKey;
		public string APIKey
		{
			get
			{
				return _apiKey;
			}

			set
			{
				Properties.Settings.Default.RAAPIKey = value;
				_apiKey = value;
				Properties.Settings.Default.Save();
			}
		}

		private static readonly HttpClient _httpClient = new HttpClient();

		private List<RaGame> _lastFetchedGames = new List<RaGame>();

		public ObservableCollection<ExpectedVersion> ExpectedVersions { get; set; } = new ObservableCollection<ExpectedVersion>();

		public RaSystem SelectedSystem { get; set; }

		/// <summary>
		/// Log that shows up in the UI
		/// </summary>
		public string Log { get; set; } = "";

		/// <summary>
		/// List of files that show up on the left side after selecting the files to hash
		/// </summary>
		public ObservableCollection<string> FilesToBeHashed { get; set; } = new ObservableCollection<string>();

		/// <summary>
		/// Hashed roms that show up on the right side after hashing
		/// </summary>
		public ObservableCollection<HashedRom> HashedRoms { get; set; } = new ObservableCollection<HashedRom>();

		public MainWindow()
		{
			InitializeComponent();
			Logger.Log.Logged += OnLogged;
			_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RAHashChecker/1.0 (contact: zakdwyer)");
		}

		private void OnLogged(string line)
		{
			Dispatcher.BeginInvoke(() => Log += line + Environment.NewLine);
		}

		// Find RA Hasher program button clicked
		private void RAHasherLocationButton_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
			{
				Title = "Select RAHasher.exe",
				Filter = "Executable (*.exe)|*.exe|All Files (*.*)|*.*"
			};

			if (dialog.ShowDialog() == true)
			{
				RAHasherPath = dialog.FileName;
			}
		}

		// Show an effect when dragging over the add files button
		private void FileDropZone_DragOver(object sender, DragEventArgs e)
		{
			// Only allow file drops; show the "copy" cursor
			e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
				? DragDropEffects.Copy
				: DragDropEffects.None;
			e.Handled = true;
		}

		private void FileDropZone_Drop(object sender, DragEventArgs e)
		{
			// No data? Return
			if (!e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				return;
			}

			// Get string array of all dropped file paths
			// e.Data.GetData() returns an object, which is then casted to a string[] which contains file paths
			string[] droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);

			// Add standalone files and files within folders
			var filesToCheck = new List<string>();
			foreach (string path in droppedPaths)
			{
				if (Directory.Exists(path))
				{
					filesToCheck.AddRange(
						Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories));
				}
				else if (File.Exists(path))
				{
					filesToCheck.Add(path);
				}
			}

			List<string> relevantFiles = GetOnlyRelevantFiles(filesToCheck);

			// Add the relevant files to the file list
			foreach (string file in relevantFiles)
			{
				FilesToBeHashed.Add(file);
			}
		}

		private void DragHereButton_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedSystem == null)
			{
				MessageBox.Show("Select a system first.");
				return;
			}

			OpenFolderDialog dialog = new OpenFolderDialog();

			List<string> filesToCheck = new List<string>();
			if (dialog.ShowDialog() == true)
			{
				var path = dialog.FolderName;
				if (Directory.Exists(path))
				{
					filesToCheck.AddRange(
						Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories));
				}
			}

			// Trim to only relevant files
			var relevantFiles = GetOnlyRelevantFiles(filesToCheck);

			// Add the files
			relevantFiles.ForEach(f => FilesToBeHashed.Add(f));
		}

		private void ClearListButton_Click(object sender, RoutedEventArgs e)
		{
			var response = MessageBox.Show("Are you sure you want to clear all files?",
				"Clear files", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (response == MessageBoxResult.Yes)
			{
				FilesToBeHashed.Clear();
			}
		}

		private async void HashButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(RAHasherPath))
			{
				MessageBox.Show("Please select RAHasher.exe first.");
				return;
			}

			if (FilesToBeHashed.Count == 0)
			{
				MessageBox.Show("Please add files to hash first.");
				return;
			}

			await RunCheckAsync(FilesToBeHashed.ToList());
		}

		/// <summary>
		/// Returns files that match the selected system's expected filenames
		/// </summary>
		/// <param name="filesToCheck"></param>
		/// <returns></returns>
		private List<string> GetOnlyRelevantFiles(List<string> filesToCheck)
		{
			// Keep only valid file types from the drop
			return filesToCheck
				.Where(f => SelectedSystem.ValidExtensions
					.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		private async void FindExpectedFilesButton_Click(object sender, RoutedEventArgs e)
		{
			ExpectedVersions.Clear();

			// Only the ones that didn't match but DID resolve to a game
			var unmatched = HashedRoms
				.Where(r => r.MatchStatus == "Not in RA" && r.ClosestMatchId > 0)
				.ToList();

			// Avoid fetching the same game twice if several files point to it
			var fetchedGames = new Dictionary<int, List<RaHashEntry>>();

			StatusText.Text = "Looking up expected files...";

			foreach (HashedRom rom in unmatched)
			{
				int gameId = rom.ClosestMatchId;

				// Fetch once per game, reuse from cache
				if (!fetchedGames.TryGetValue(gameId, out List<RaHashEntry>? entries))
				{
					try
					{
						entries = await RetroAchievementsTools.FetchGameHashesWithRetryAsync(gameId, _httpClient);
						fetchedGames[gameId] = entries;
						await Task.Delay(200);  // So we don't hit the limit
					}
					catch (Exception ex)
					{
						Log += $"Failed to fetch hashes for game {gameId}: {ex.Message}\n";
						continue;
					}
				}

				foreach (RaHashEntry entry in entries)
				{
					ExpectedVersions.Add(new ExpectedVersion
					{
						SourceFileName = rom.FileName,
						GameTitle = rom.ClosestMatch,
						GameId = gameId,
						ExpectedFileName = entry.Name,
						Md5 = entry.Md5,
						Labels = string.Join(", ", entry.Labels),
						PatchUrl = entry.PatchUrl ?? ""
					});
				}
			}

			new ExpectedVersionsWindow(ExpectedVersions).ShowDialog();

			StatusText.Text = $"Found {ExpectedVersions.Count} expected versions.";
		}

		private void MatchAgainstRa(List<RaGame> games)
		{
			// hash -> the game it belongs to (so we have title AND achievement count)
			var hashToGame = new Dictionary<string, RaGame>(StringComparer.OrdinalIgnoreCase);
			foreach (RaGame game in games)
			{
				foreach (string hash in game.Hashes)
				{
					hashToGame[hash] = game;
				}
			}

			foreach (HashedRom rom in HashedRoms)
			{
				if (string.IsNullOrEmpty(rom.RAHash))
				{
					rom.MatchStatus = "Failed to hash";
					rom.MatchedGame = "";
				}
				else if (hashToGame.TryGetValue(rom.RAHash, out RaGame? game))
				{
					rom.MatchedGame = game.Title;

					if (game.NumAchievements > 0)
					{
						rom.MatchStatus = "✓ Has achievements";
					}
					else
					{
						rom.MatchStatus = "Found, but no achievements";
					}
				}
				else
				{
					rom.MatchStatus = "Not in RA";
					rom.MatchedGame = "";

					RaGame? closest = FindClosestGame(rom.FileName);
					if (closest != null)
					{
						rom.ClosestMatch = closest.Title;
						rom.ClosestMatchId = closest.Id;
					}
				}
			}
		}

		private void OpenRaSearch(string fileName)
		{
			string query = BuildSearchQuery(fileName);

			string url = $"https://retroachievements.org/search" +
						 $"?query={Uri.EscapeDataString(query)}" +
						 $"&scope=games";

			Process.Start(new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			});
		}

		private static string BuildSearchQuery(string fileName)
		{
			// Drop the extension first
			string name = System.IO.Path.GetFileNameWithoutExtension(fileName);

			// Remove anything in (parentheses) or [brackets], including the brackets
			name = Regex.Replace(name, @"\(.*?\)", "");
			name = Regex.Replace(name, @"\[.*?\]", "");

			// Collapse any doubled-up whitespace left behind, and trim
			name = Regex.Replace(name, @"\s+", " ").Trim();

			return name;
		}

		private async Task RunCheckAsync(List<string> relevant)
		{
			var progress = new Progress<HashProgress>(p =>
			{
				HashProgressBar.Maximum = p.Total;
				HashProgressBar.Value = p.Current;
				StatusText.Text = $"Hashing {p.Current}/{p.Total}: {p.CurrentFile}";
			});

			// Step 1: hash the files
			List<HashedRom> hashed = await HashFilesAsync(RAHasherPath, relevant, SelectedSystem, progress);

			HashedRoms.Clear();
			foreach (HashedRom hashedRom in hashed)
			{
				HashedRoms.Add(hashedRom);
			}

			// Step 2: fetch RA's game + hash list for the chosen system
			try
			{
				StatusText.Text = "Fetching RetroAchievements data...";
				var raResponse = await RetroAchievementsTools.FetchGamesWithHashesAsync(SelectedSystem.ConsoleId, _httpClient);
				_lastFetchedGames = raResponse;
				MatchAgainstRa(raResponse);
				StatusText.Text = "Done.";
			}
			catch (Exception ex)
			{
				StatusText.Text = "RA fetch failed.";
				Logger.Log.Write($"RA API error: {ex.Message}");
			}
		}

		private RaGame? FindClosestGame(string fileName)
		{
			string query = Normalize(BuildSearchQuery(fileName));

			if (string.IsNullOrWhiteSpace(query))
			{
				return null;
			}

			RaGame? hit = _lastFetchedGames.FirstOrDefault(g =>
				Normalize(g.Title).Equals(query, StringComparison.OrdinalIgnoreCase));

			if (hit == null)
			{
				hit = _lastFetchedGames.FirstOrDefault(g =>
					Normalize(g.Title).Contains(query, StringComparison.OrdinalIgnoreCase));
			}

			return hit;
		}

		private void SearchForGameButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.DataContext is HashedRom rom)
			{
				OpenRaSearch(rom.FileName);
			}
		}

		private void RemoveFromClick_ContextMenu_Click(object sender, RoutedEventArgs e)
		{
			// Copy to a list first — moving modifies HashedRoms, and you can't
			// modify a collection while iterating the live SelectedItems over it.
			var selectedRoms = ResultsGrid.SelectedItems
				.Cast<HashedRom>()
				.ToList();

			foreach (HashedRom rom in selectedRoms)
			{
				try
				{
					MoveRomToExtras(rom);
					HashedRoms.Remove(rom);
				}
				catch (Exception ex)
				{
					Log += $"Failed to move {rom.FileName}: {ex.Message}\n\n";
				}
			}
		}

		private void MoveRomToExtras(HashedRom rom)
		{
			string sourcePath = rom.FilePath;
			string? sourceDir = Path.GetDirectoryName(sourcePath);

			if (sourceDir == null)
			{
				return;
			}

			string baseName = Path.GetFileNameWithoutExtension(sourcePath);

			// All files in this folder that belong to THIS game (cue + its bins, or the single chd/zip)
			var gameFiles = Directory
				.EnumerateFiles(sourceDir)
				.Where(f => Path.GetFileNameWithoutExtension(f)
					.StartsWith(baseName, StringComparison.OrdinalIgnoreCase))
				.ToList();

			// Everything else in the folder that does NOT belong to this game
			var otherFiles = Directory
				.EnumerateFiles(sourceDir)
				.Where(f => !gameFiles.Contains(f))
				.ToList();

			// Are there subfolders? If so, the folder isn't a clean single-game folder.
			bool hasSubfolders = Directory.EnumerateDirectories(sourceDir).Any();

			// "Dedicated folder" = this game's files are the ONLY files here, no other games, no subfolders.
			bool folderIsDedicatedToThisGame = otherFiles.Count == 0 && !hasSubfolders;

			if (folderIsDedicatedToThisGame)
			{
				// Move the whole folder up into an Extras next to the PARENT
				MoveWholeFolderToExtras(sourceDir);
			}
			else
			{
				// Shared directory (loose files among other games) — move only this game's files
				string extrasDir = Path.Combine(sourceDir, "Extras");
				Directory.CreateDirectory(extrasDir);

				foreach (string file in gameFiles)
				{
					MoveOneFile(file, extrasDir);
				}
			}

			HashedRoms.Remove(rom);
			Log += $"Moved {rom.FileName} to Extras\\\n\n";
		}

		private void MoveWholeFolderToExtras(string gameFolder)
		{
			// The dedicated folder, e.g. X:\...\PS1\Resident Evil
			string? parentDir = Path.GetDirectoryName(gameFolder);   // X:\...\PS1
			if (parentDir == null)
			{
				return;
			}

			// Extras sits next to the game folder, inside PS1
			string extrasDir = Path.Combine(parentDir, "Extras");
			Directory.CreateDirectory(extrasDir);

			string folderName = Path.GetFileName(gameFolder);         // "Resident Evil"
			string destPath = Path.Combine(extrasDir, folderName);    // X:\...\PS1\Extras\Resident Evil

			if (Directory.Exists(destPath))
			{
				throw new IOException($"{folderName} already exists in Extras.");
			}

			Directory.Move(gameFolder, destPath);
		}

		private static void MoveOneFile(string sourceFile, string destDir)
		{
			string destPath = Path.Combine(destDir, Path.GetFileName(sourceFile));

			// Don't clobber an existing file
			if (File.Exists(destPath))
			{
				throw new IOException($"{Path.GetFileName(destPath)} already exists in Extras.");
			}

			File.Move(sourceFile, destPath);
		}

		/// <summary>
		/// Strips puncutation that differs between filenames and RA titles, such as dashes, colons, etc.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private static string Normalize(string s)
		{
			// Strip punctuation that differs between filenames and RA titles
			// (dash vs colon, apostrophes, etc.) so "X - Y" and "X: Y" compare equal.
			s = Regex.Replace(s, @"[^a-zA-Z0-9 ]", "");   // keep letters, digits, spaces
			s = Regex.Replace(s, @"\s+", " ").Trim();
			return s;
		}

		// Might be useful to keep around in case I add a way to add files via open file dialog
		private string BuildFilterFromSelectedSystem()
		{
			string filter = "";

			foreach (var ext in SelectedSystem.ValidExtensions)
			{
				filter += ext + "|" + ext;
			}

			return filter;
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
		}

		public HashedRom SelectedHashedRom { get; set; }

		private async void FindExpectedFilename_Click(object sender, RoutedEventArgs e)
		{
			ExpectedVersions.Clear();

			if (SelectedHashedRom == null)
			{
				MessageBox.Show("Please select a ROM first.");
				return;
			}

			if (SelectedHashedRom.MatchStatus != "Not in RA")
			{
				MessageBox.Show("Please selected a ROM that wasn't found in Retro Achievements.");
				return;
			}

			if (SelectedHashedRom.ClosestMatchId == 0)
			{
				MessageBox.Show("Please select a rom that has a closest match.");
				return;
			}

			// Avoid fetching the same game twice if several files point to it
			var fetchedGames = new Dictionary<int, List<RaHashEntry>>();

			StatusText.Text = "Looking up expected filename...";

			int gameId = SelectedHashedRom.ClosestMatchId;

			// Fetch once per game, reuse from cache
			if (!fetchedGames.TryGetValue(gameId, out List<RaHashEntry>? entries))
			{
				try
				{
					// Grab entries for this game ID
					entries = await RetroAchievementsTools.FetchGameHashesWithRetryAsync(gameId, _httpClient);
					fetchedGames[gameId] = entries;
				}
				catch (Exception ex)
				{
					Log += $"Failed to fetch hashes for game {gameId}: {ex.Message}\n";
					return;
				}

				foreach (RaHashEntry entry in entries)
				{
					ExpectedVersions.Add(new ExpectedVersion
					{
						SourceFileName = SelectedHashedRom.FileName,
						GameTitle = SelectedHashedRom.ClosestMatch,
						GameId = gameId,
						ExpectedFileName = entry.Name,
						Md5 = entry.Md5,
						Labels = string.Join(", ", entry.Labels),
						PatchUrl = entry.PatchUrl ?? ""
					});
				}
			}

			new ExpectedVersionsWindow(ExpectedVersions).ShowDialog();

			StatusText.Text = $"Found {ExpectedVersions.Count} expected versions.";
		}

		private void HelpButton_Click(object sender, RoutedEventArgs e)
		{
			HelpWindow win = new HelpWindow();
			win.Show();
		}
	}
}