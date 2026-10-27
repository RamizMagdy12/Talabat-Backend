using System.Globalization;
using System.Text.Json;

namespace Talabat.Admin.Localization
{
	public class SharedLocalizer
	{
		private readonly Dictionary<string, Dictionary<string, string>> _dicts;
		private readonly ILogger<SharedLocalizer> _logger;

		public SharedLocalizer(IWebHostEnvironment env, ILogger<SharedLocalizer> logger)
		{
			_logger = logger;
			_dicts = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

			foreach (var lang in new[] { "en", "ar" })
			{
				var path = Path.Combine(env.WebRootPath, "resources", $"{lang}.json");
				_dicts[lang] = LoadFile(path);
			}
		}

		public string this[string key] => Get(key);

		public string Get(string key, params (string Name, object? Value)[] args)
		{
			var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			if (!_dicts.TryGetValue(lang, out var dict))
				dict = _dicts["en"];

			if (!dict.TryGetValue(key, out var value) && _dicts["en"].TryGetValue(key, out var enValue))
				value = enValue;

			value ??= key;

			foreach (var (name, raw) in args)
				value = value.Replace($"{{{{{name}}}}}", raw?.ToString() ?? string.Empty, StringComparison.Ordinal);

			return value;
		}

		public bool IsRtl => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase);

		private Dictionary<string, string> LoadFile(string path)
		{
			try
			{
				if (!File.Exists(path))
				{
					_logger.LogWarning("Shared resource file missing: {Path}", path);
					return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				}

				var json = File.ReadAllText(path);
				var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
					?? new Dictionary<string, string>();
				return new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed loading shared resources from {Path}", path);
				return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}
		}
	}
}
