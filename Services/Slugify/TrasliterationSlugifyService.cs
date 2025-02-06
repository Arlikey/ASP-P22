using System.Text;
using System.Text.RegularExpressions;

namespace ASP_P22.Services.Slugify
{
	public class TrasliterationSlugifyService : ISlugifyService
	{
		private readonly Dictionary<char, string> transliterationMap = new()
	{
		{'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"}, {'е', "e"},
		{'ё', "yo"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"}, {'й', "y"}, {'к', "k"},
		{'л', "l"}, {'м', "m"}, {'н', "n"}, {'о', "o"}, {'п', "p"}, {'р', "r"},
		{'с', "s"}, {'т', "t"}, {'у', "u"}, {'ф', "f"}, {'х', "kh"}, {'ц', "ts"},
		{'ч', "ch"}, {'ш', "sh"}, {'щ', "shch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
		{'э', "e"}, {'ю', "yu"}, {'я', "ya"}
	};

		public string GenerateSlug(string phrase)
		{
			string str = Transliterate(phrase.ToLower());

			str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
			str = Regex.Replace(str, @"\s+", " ").Trim();
			str = str.Length > 45 ? str[..45] : str;
			str = Regex.Replace(str, @"\s", "-");

			return str;
		}

		public string Transliterate(string text)
		{
			var sb = new StringBuilder();
			foreach (char c in text)
			{
				sb.Append(transliterationMap.TryGetValue(c, out string? value) ? value : c.ToString());
			}
			return sb.ToString();
		}
	}
}
