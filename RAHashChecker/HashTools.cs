using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static RAHashChecker.MainWindow;

namespace RAHashChecker
{
	public static class HashTools
	{
		public record HashProgress(int Current, int Total, string CurrentFile);

		public static async Task<List<HashedRom>> HashFilesAsync(
			string raHasherPath,
			IReadOnlyList<string> filePaths,
			RaSystem system,
			IProgress<HashProgress>? progress = null)
		{
			var results = new List<HashedRom>();

			for (int i = 0; i < filePaths.Count; i++)
			{
				string filePath = filePaths[i];
				string fileName = System.IO.Path.GetFileName(filePath);

				// Tell the UI what we're about to work on
				if (progress != null)
				{
					progress.Report(new HashProgress(i + 1, filePaths.Count, fileName));
				}

				string? hash = await RunRaHasherAsync(raHasherPath, filePath, system);

				// Store the result even on failure, so the user sees which files didn't hash
				results.Add(new HashedRom(filePath, hash ?? ""));
			}

			return results;
		}

		private static async Task<string?> RunRaHasherAsync(string raHasherPath, string filePath, RaSystem system)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = raHasherPath,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			startInfo.ArgumentList.Add(system.ConsoleId.ToString());
			startInfo.ArgumentList.Add(filePath);

			using (var process = new Process())
			{
				process.StartInfo = startInfo;
				process.Start();

				string output = await process.StandardOutput.ReadToEndAsync();
				string errorOutput = await process.StandardError.ReadToEndAsync();
				await process.WaitForExitAsync();

				if (process.ExitCode != 0)
				{
					// Surface the reason instead of silently returning null
					Logger.Log.Write($"RAHasher failed on {filePath}: {errorOutput.Trim()}");
					return null;
				}

				return ParseHash(output);
			}
		}

		private static string? ParseHash(string rawOutput)
		{
			if (string.IsNullOrWhiteSpace(rawOutput))
			{
				return null;
			}

			// Take the first token of the first non-empty line.
			// RAHasher prints the hash first; in some modes a filename follows it.
			string firstLine = rawOutput
				.Split('\n')
				.Select(line => line.Trim())
				.FirstOrDefault(line => line.Length > 0) ?? "";

			string firstToken = firstLine.Split(' ')[0].Trim();

			// Sanity check: a valid RA hash is 32 hex characters
			if (IsValidHash(firstToken))
			{
				return firstToken;
			}

			return null;
		}

		private static bool IsValidHash(string candidate)
		{
			if (candidate.Length != 32)
			{
				return false;
			}

			foreach (char c in candidate)
			{
				bool isHex = (c >= '0' && c <= '9')
						  || (c >= 'a' && c <= 'f')
						  || (c >= 'A' && c <= 'F');
				if (!isHex)
				{
					return false;
				}
			}

			return true;
		}
	}
}