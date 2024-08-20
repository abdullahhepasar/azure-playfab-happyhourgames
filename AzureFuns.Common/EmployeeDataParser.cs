namespace PlayFab.AzureFunctions
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization; 
    using System.IO; 

    public class EmployeeDataParser : IParser<RawPlayer>
    {
        private readonly JsonSerializerSettings _settings;

        public EmployeeDataParser()
        {
            var resolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            _settings = new JsonSerializerSettings
            {
                ContractResolver = resolver, 
                Formatting = Formatting.Indented
            };
        }

        public RawPlayer Parse(string input)
        {
            return JsonConvert.DeserializeObject<RawPlayer>(input, _settings);
        }

        public RawPlayer ReadAndParse(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var input = reader.ReadToEnd();
                return Parse(input);
            }
        }
    }
}
