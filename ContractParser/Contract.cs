using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ContractParser
{
    public class Contract
    {
        public string Name { get; set; }
        public string Title { get; set; }

        private bool genericTitle = false;

        public Contract()
        {
        }

        public Contract(string name, string title)
        {
            this.Name = name;
            this.Title = title;
        }

        public static Contract TryParseContract(ConfigBlock block)
        {
            if (block.content == null || block.content.Count == 0)
                return null;

            var c = new Contract();
            if (!c.FetchContract(block)) return null;
            Console.WriteLine($"Parsed contract {c.Name}");
            return c;
        }

        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            return JsonConvert.SerializeObject(this, settings);
        }

        private bool FetchContract(ConfigBlock block)
        {
            List<string[]> cfg = block.content;
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
                            if(!genericTitle)
                                Title = value;
                            break;
                        case "genericTitle":
                            Title = value;
                            genericTitle = true;
                            break;
                        case "name":
                            Name = value;
                            break;
                    }
                }
            }

            foreach (ConfigBlock innerBlock in block.childrenBlocks)
            {
                GetDataFromBlock(innerBlock);
                if (innerBlock.type == BlockType.DataExpand)
                    return false;   // cannot parse expanded contracts yet
            }

            return !string.IsNullOrWhiteSpace(Name);
        }

        private void GetDataFromBlock(ConfigBlock block)
        {
            if (block.type == BlockType.DataExpand && block.content[0][0] == "type" && block.content[0][1] == "CelestialBody")
            {
                string paramName = block.content[1][0];
                string bodiesKey = block.content[1][1];
                // TODO: try to properly expand scansat contracts:
                //  1) fetch the body names from Groups.cfg using bodiesKey
                //  2) return a separate Contract for every body
                //  3) in every Contract, replace all occurrences of paramName with body name
                //  4) append $".{body name}" to the name of every Contract
            }
            else if (block.type == BlockType.DataExpand && block.content[0][0] == "type" && block.content[0][1] == "string")
            {
                // probably expanding based on scansat experiment type?
            }
        }
    }
}
