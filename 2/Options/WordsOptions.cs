public class WordsOptions
{
    public const string SectionName = "ForbiddenSettings";
    public List<String> RestrictedWords {get; set;} = new List<String>();
}