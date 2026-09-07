using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace RAHashChecker
{
	public static class RetroAchievementsTools
	{
		/// <summary>
		/// Grab a list of hashes for a given game.
		/// </summary>
		/// <param name="gameId"></param>
		/// <param name="httpClient"></param>
		/// <returns></returns>
		private static async Task<List<RaHashEntry>> FetchGameHashesAsync(int gameId, HttpClient httpClient)
		{
			string apiKey = Properties.Settings.Default.RAAPIKey;

			string url = $"https://retroachievements.org/API/API_GetGameHashes.php" +
						 $"?y={Uri.EscapeDataString(apiKey)}" +
						 $"&i={gameId}";

			string json = await httpClient.GetStringAsync(url);

			RaGameHashResult? result =
				System.Text.Json.JsonSerializer.Deserialize<RaGameHashResult>(json);

			return result?.Results ?? new List<RaHashEntry>();
		}

		/// <summary>
		/// Grab a list of games for a given console.
		/// </summary>
		/// <param name="consoleId"></param>
		/// <param name="httpClient"></param>
		/// <returns></returns>
		public static async Task<List<RaGame>> FetchGamesWithHashesAsync(int consoleId, HttpClient httpClient)
		{
			string username = Properties.Settings.Default.RAUsername;
			string apiKey = Properties.Settings.Default.RAAPIKey;

			string url = $"https://retroachievements.org/API/API_GetGameList.php" +
						 $"?z={Uri.EscapeDataString(username)}" +
						 $"&y={Uri.EscapeDataString(apiKey)}" +
						 $"&i={consoleId}" +
						 $"&h=1";

			string json = await httpClient.GetStringAsync(url);

			List<RaGame>? games = System.Text.Json.JsonSerializer.Deserialize<List<RaGame>>(json);

			return games ?? new List<RaGame>();
		}

		public static async Task<List<RaHashEntry>> FetchGameHashesWithRetryAsync(int gameId, HttpClient http)
		{
			for (int attempt = 0; attempt < 4; attempt++)
			{
				try
				{
					return await FetchGameHashesAsync(gameId, http);
				}
				catch (HttpRequestException ex) when (ex.StatusCode == (HttpStatusCode)429)
				{
					await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // 1s, 2s, 4s, 8s
				}
			}
			throw new Exception($"Gave up on game {gameId} after repeated 429s.");
		}
	}
}