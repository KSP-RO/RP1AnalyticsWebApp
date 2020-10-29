using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ContractParser
{
    public class Contract
    {
        public string Name { get; set; }
        public string Title { get; set; }

        public Contract(List<string[]> cfg)
        {
            if (cfg == null || cfg.Count == 0)
                return;

            FetchContract(cfg);
            Console.WriteLine($"Added contract {this.Name}");
        }

        public Contract(string name, string title)
        {
            this.Name = name;
            this.Title = title;
        }

        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            return JsonConvert.SerializeObject(this, settings);
        }

        private void FetchContract(List<string[]> cfg)
        {
            for (int i = 0; i < cfg.Count; i++)
            {
                string[] line = cfg[i];
                if (line.Length == 2)
                {
                    string field = line[0];
                    string value = line[1];

                    field = field.RemoveOperator();

                    switch (field)
                    {
                        case "title":
                            Title = value;
                            break;
                        case "name":
                            Name = value;
                            break;
                    }
                }
                else if (line[0].Contains("{"))
                {
                    var block = ConfigBlock.ExtractBlock(cfg, cfg[i - 1][0], ref i, true);

                    GetDataFromBlock(block);
                }
            }
        }

        private void GetDataFromBlock(ConfigBlock block)
        {

        }
    }
}
