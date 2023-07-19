using System.Configuration;

namespace qckdev.Net.Http.Test.Common.Configuration
{
    public sealed class Settings 
    {

        public string JiraUrl { get; set; }
        public string JiraToken { get; set; }
        public string PokemonUrl { get; set; }
        public string GorestUrl { get; set; }
        public string GorestToken { get; set; }
        public string MockbinUrl { get; set; }

    }
}
