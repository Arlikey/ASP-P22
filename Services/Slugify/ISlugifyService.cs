using System.Text.RegularExpressions;

namespace ASP_P22.Services.Slugify
{
	public interface ISlugifyService
	{
		 string GenerateSlug(string phrase);

		 string Transliterate(string txt);
	}
}
