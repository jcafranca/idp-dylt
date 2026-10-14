using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.DataTypes
{
    public class LookupInfo
    {
        public int id { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public string long_description { get; set; }
        private string _relatedCodeRaw;
        public string related_code
        {
            get => _relatedCodeRaw;
            set
            {
                _relatedCodeRaw = value;
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        RelatedCodeObject = JsonConvert.DeserializeObject<RelativeCode>(value);
                    }
                    catch (JsonException)
                    {
                        RelatedCodeObject = null; // Handle invalid JSON gracefully
                    }
                }
            }
        }

        [JsonIgnore]  // Prevents serialization to JSON
        public RelativeCode RelatedCodeObject { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(description) ? long_description : description;
        }
    }
    public class RelativeCode
    {
        public string action { get; set; }
        public List<Entries> entries { get; set; }
    }
    public class Entries
    {
        public string type { get; set; }
        public string code { get; set; }
    }

}
