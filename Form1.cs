using DiscordRPC;
using DiscordRPC.Logging;
using NPSMLib;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TidalDiscord
{
	public partial class Form1 : Form
	{

		public DiscordRpcClient? client = null;
		public RichPresence presence;
		public Timestamps started = Timestamps.Now;
		public NowPlayingSessionManager npsm = new();
		public string last = "";
		public System.Timers.Timer? timer = null;
		public TimeSpan currentpos = new(0);
		public TimeSpan lastping = new(0);
		public TidalAPI api = new();
		public string prevQuery = "";
		public Dictionary<string, string> env = new();

		public Form1()
		{
			presence = new RichPresence()
			{
				Details = "details",
				Timestamps = started,
				State = "state",
				Assets = new Assets()
				{
					LargeImageKey = "untitled",
					LargeImageText = "TIDAL",
				},
				Buttons = [
                    //new DiscordRPC.Button() {
                    //    Label = "button1",
                    //    Url = "https://tpyn.uk/#button1"
                    //},
                    //new DiscordRPC.Button() {
                    //    Label = "button2",
                    //    Url = "https://tpyn.uk/#button2"
                    //}
                ]
			};
			InitializeComponent();

			string EnvFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "env.json");
			if (!File.Exists(EnvFile))
			{
				MessageBox.Show("env.json not found - use env.example.json to create one");
				return;
			}
			string json = File.ReadAllText(EnvFile);
			var _env = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
			if (_env == null || !_env.ContainsKey("client_id") || !_env.ContainsKey("client_secret") || !_env.ContainsKey("discord_id") || _env["client_id"] == "" || _env["client_secret"] == "" || _env["discord_id"] == "")
			{
				MessageBox.Show("env.json is missing client_id, client_secret or discord_id");
				Application.Exit();
				Environment.Exit(1);
				return;
			}
			env = _env;

			timer = new System.Timers.Timer();
			client = new DiscordRpcClient(env["discord_id"]);

			// {
			// 	Logger = new Logger(this, LogLevel.Info)
			// };

			npsm.SessionListChanged += onChange;

			client.Initialize();

			client.SetPresence(presence);

			// Every second, run the onChange function
			timer.Interval = 1000;
			timer.Elapsed += (sender, e) => tick();
			timer.Start();

			FormBorderStyle = FormBorderStyle.None;
			SizeGripStyle = SizeGripStyle.Hide;

			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			SetStyle(ControlStyles.Opaque, true);
			// SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			BackColor = Color.FromArgb(0, 0, 0);
			ForeColor = Color.White;

			// int taskbarHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
			// Location = new Point(0, Screen.PrimaryScreen.Bounds.Height - 75 - taskbarHeight);
			TopMost = true;
		}
		// protected override void OnPaintBackground(PaintEventArgs e)
		// {
		// 	// e.Graphics.FillRectangle(Brushes., e.ClipRectangle);
		// }

		private void onChange(object? sender, EventArgs? e)
		{
			tick(true);
		}

		private void tick(bool recheckpos = false)
		{
			if (npsm.CurrentSession == null) return;
			if (client == null) return;
			MediaPlaybackDataSource source = npsm.CurrentSession.ActivateMediaPlaybackDataSource();
			MediaObjectInfo info = source.GetMediaObjectInfo();
			MediaTimelineProperties timeline = source.GetMediaTimelineProperties();
			bool isPaused = source.GetMediaPlaybackInfo().PlaybackState != MediaPlaybackState.Playing;

			// Runs every second, so increment the current position
			if (!isPaused) currentpos = currentpos.Add(new TimeSpan(0, 0, 1));

			if (recheckpos || lastping != timeline.Position) currentpos = timeline.Position;

			presence.Details = string.Format("Listening to {0} by {1}", info.Title, info.Artist);
			presence.State = string.Format("{0}/{1}", format(currentpos), format(timeline.EndTime)) + (isPaused ? " (Paused)" : "");

			Console.WriteLine(string.Format("Timer tick, {0}, {1}", presence.Details, presence.State));


			if (!api.tokenValid())
			{
				// read from env.json
				api.login(env["client_id"], env["client_secret"]);
				if (!api.tokenValid())
				{
					MessageBox.Show(this, "Failed to login to Tidal API, check your env.json file");
					Application.Exit();
					Environment.Exit(1);
					return;
				}
			}

			string query = info.Artist + " " + info.Title;
			if (query != prevQuery && query != " ")
			{
				(bool success, string id) = api.search(query);
				if (!success)
				{
					// MessageBox.Show(this,"Failed to search for " + query + ": " + id);
				}
				else
				{
					(bool success2, string url) = api.getImage(id);
					if (!success2)
					{
						// MessageBox.Show(this,"Failed to get image for " + id + ": " + url);
					}
					else
					{
						presence.Assets.LargeImageKey = url;
						presence.Assets.LargeImageText = info.AlbumTitle;
					}
				}
			}
			presence.Assets.SmallImageKey = "untitled";
			presence.Assets.SmallImageText = "TidalDiscord - https://github.com/tpguy825/TidalDiscord";
			Invoke(new Action(() =>
			{
				if (last != (info.Title + " - " + info.Artist + (isPaused ? " (Paused)" : "")))
				{
					// Hide controls to prevent flickering
					// ResumeLayout(false);
					// PerformLayout();
					SuspendLayout();

					AutoSize = false;
					Width = 75;
					Refresh();
					AutoSize = true;

					label3.Refresh();
					label4.Refresh();
					pictureBox1.Refresh();

				}
				else
				{
					SuspendLayout();
				}
				pictureBox1.Visible = true;
				if (Screen.PrimaryScreen != null)
				{
					int taskbarHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
					Location = new Point(0, Screen.PrimaryScreen.Bounds.Height - 75 - taskbarHeight);
				}

				label3.Refresh();
				label4.Refresh();
				pictureBox1.Refresh();

				progressBar1.Value = (int)Math.Round(currentpos.TotalSeconds / timeline.EndTime.TotalSeconds * 100);
				label2.Text = format(currentpos) + " / " + format(timeline.EndTime);
				label3.Text = info.Title + (isPaused ? " (Paused)" : "");
				label4.Text = info.Artist;
				pictureBox1.Image = ResizeImage(Image.FromStream(source.GetThumbnailStream()), 75, 75);
				last = info.Title + " - " + info.Artist + (isPaused ? " (Paused)" : "");
				ResumeLayout(false);
				PerformLayout();
			}));

			lastping = timeline.Position;
			prevQuery = query;

			client.SetPresence(presence);

			// check();
		}

		public static Bitmap ResizeImage(Image image, int width, int height)
		{
			Rectangle destRect = new(0, 0, width, height);
			Bitmap destImage = new(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using var wrapMode = new ImageAttributes();
				wrapMode.SetWrapMode(WrapMode.TileFlipXY);
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
			}

			return destImage;
		}

		private static string format(TimeSpan time) => time.ToString(@"mm\:ss");
	}

	public class Logger(Form1 parent, LogLevel level = LogLevel.Error) : ILogger
	{
		public LogLevel Level { get; set; } = level;
		public Form1 parent { get; set; } = parent;

		public void Trace(string message, params object[] args)
		{
			if (Level > LogLevel.Trace) return;
			// parent.listBox1.Items.Add(string.Format(message, args));
		}

		public void Info(string message, params object[] args)
		{
			if (Level > LogLevel.Info) return;
			// parent.check();
		}

		public void Warning(string message, params object[] args)
		{
			if (Level > LogLevel.Warning) return;
			// parent.listBox1.Items.Add(string.Format(message, args));
			MessageBox.Show(string.Format(message, args));
		}

		public void Error(string message, params object[] args)
		{
			if (Level > LogLevel.Error) return;
			if (string.Format(message, args).Contains("to discord-ipc-")) return;
			// parent.listBox1.Items.Add(string.Format(message, args));
			MessageBox.Show(string.Format(message, args));
		}
	}

	public class TidalToken
	{
		public required string access_token { get; set; }
		public required string token_type { get; set; }
		public required DateTime expires { get; set; }
	}

	public class TidalAPI
	{
		private class TidalTokenResponse
		{
			public required string access_token { get; set; }
			public required string token_type { get; set; }
			public required int expires_in { get; set; }
		}

		private class SearchResponse
		{
			public required IdType[] data { get; set; }
		}

		private class IdType
		{
			public required string id { get; set; }
			public required string type { get; set; }
		}

		private class IdResponse
		{
			public required Included[] included { get; set; }
		}

		private class Included
		{
			public required Attributes attributes { get; set; }
		}

		private class Attributes
		{
			public required ImageLinks[] imageLinks { get; set; }
		}

		private class ImageLinks
		{
			public required string href { get; set; }
			public required ImageLinksMeta meta { get; set; }
		}

		private class ImageLinksMeta
		{
			public required int width { get; set; }
			public required int height { get; set; }
		}

		public TidalAPI() { }

		private TidalToken? token = null;

		public bool tokenValid()
		{
			return token != null && token.expires > DateTime.Now;
		}

		public bool login(string client_id, string client_secret)
		{
			// B64CREDS=$(echo -n "<CLIENT_ID>:<CLIENT_SECRET>" | base64)
			//curl - X POST \
			//-H "Authorization: Basic $B64CREDS" \
			//-d "grant_type=client_credentials" \
			//"https://auth.tidal.com/v1/oauth2/token"

			string b64creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{client_id}:{client_secret}"));
			string url = "https://auth.tidal.com/v1/oauth2/token";
			string data = "grant_type=client_credentials";

			try
			{
				HttpClient client = new();
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", b64creds);
				var content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
				var response = client.PostAsync(url, content).Result;
				var responseString = response.Content.ReadAsStringAsync().Result;
				if (responseString == "" || response.StatusCode != System.Net.HttpStatusCode.OK || responseString == null) return false;
				TidalTokenResponse? res = JsonSerializer.Deserialize<TidalTokenResponse>(responseString);
				if (res == null) return false;
				token = new()
				{
					access_token = res.access_token,
					token_type = res.token_type,
					expires = DateTime.Now.AddSeconds(res.expires_in)
				};
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public (bool, string) search(string query)
		{
			if (token == null) return (false, "Not logged in");
			if (query == "") return (false, "Query is empty");
			if (token.expires < DateTime.Now) return (false, "Token expired");

			//          curl - X 'GET' \
			//'https://openapi.tidal.com/search?query=Sentinel%20Noisestorm&offset=0&limit=1&countryCode=GB&popularity=WORLDWIDE' \
			//-H 'accept: application/vnd.tidal.v1+json' \
			//-H 'Authorization: Bearer ey...' \
			//-H 'Content-Type: application/vnd.tidal.v1+json'

			try
			{
				// https://developer.tidal.com/apiref?spec=search&ref=search

				string url = $"https://openapi.tidal.com/v2/searchResults/{Uri.EscapeDataString(query)}/relationships/tracks?countryCode=GB&include=tracks";
				// MessageBox.Show(url);
				HttpClient client = new()
				{
					BaseAddress = new Uri(url),
					
				};
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token?.access_token);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));
				HttpRequestMessage request = new(HttpMethod.Get, url)
				{
					// Content = new StringContent("", Encoding.UTF8, "application/vnd.tidal.v1+json")
				};
				var response = client.SendAsync(request).Result;
				var responseString = response.Content.ReadAsStringAsync().Result;
				if (response.StatusCode != System.Net.HttpStatusCode.OK) return (false, "Failed to get response, status code " + response.StatusCode);
				if (responseString == "" || responseString == null) return (false, "Empty response");

				// get tracks[0].resource.album.imageCover from responseString
				var res = JsonSerializer.Deserialize<SearchResponse>(responseString);
				if (res == null) return (false, "Failed to parse response");
				var track = res.data[0];

				return (true, track.id);
			}
			catch (Exception e)
			{
				return (false, e.Message);
			}
		}

		public (bool, string) getImage(string id)
		{
			if (token == null) return (false, "Not logged in");
			if (id == "") return (false, "Query is empty");
			if (token.expires < DateTime.Now) return (false, "Token expired");

			//          curl - X 'GET' \
			//'https://openapi.tidal.com/search?query=Sentinel%20Noisestorm&offset=0&limit=1&countryCode=GB&popularity=WORLDWIDE' \
			//-H 'accept: application/vnd.tidal.v1+json' \
			//-H 'Authorization: Bearer eyJ...' \
			//-H 'Content-Type: application/vnd.tidal.v1+json'

			try
			{
				// https://developer.tidal.com/apiref?spec=search&ref=search

				string url = $"https://openapi.tidal.com/v2/tracks/{id}?countryCode=GB&include=albums";
				HttpClient client = new()
				{
					BaseAddress = new Uri(url)
				};
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token?.access_token);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));
				HttpRequestMessage request = new(HttpMethod.Get, url)
				{
					// Content = new StringContent("", Encoding.UTF8, "application/vnd.tidal.v1+json")
				};
				var response = client.SendAsync(request).Result;
				var responseString = response.Content.ReadAsStringAsync().Result;
				if (response.StatusCode != System.Net.HttpStatusCode.OK) return (false, "Failed to get response, status code " + response.StatusCode);
				if (responseString == "" || responseString == null) return (false, "Empty response");

				// get tracks[0].resource.album.imageCover from responseString
				var res = JsonSerializer.Deserialize<IdResponse>(responseString);
				if (res == null) return (false, "Failed to parse response");
				var covers = res.included[0].attributes.imageLinks;

				if (covers == null || covers.Length == 0) return (false, "No covers found");

				ImageLinks highestquality = covers[0];
				foreach (var cover in covers)
				{
					if (cover.meta.width > highestquality.meta.width) highestquality = cover;
				}

				return (true, highestquality.href);
			}
			catch (Exception e)
			{
				return (false, e.Message);
			}
		}
	}

}
