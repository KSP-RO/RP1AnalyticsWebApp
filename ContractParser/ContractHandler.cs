using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ContractParser
{
    public static class ContractHandler
    {
        public static Dictionary<string, Contract> Contracts { get; private set; } = new Dictionary<string, Contract>();
        public static int Count { get => Contracts.Count; }

        public static bool Add(Contract c, bool overwrite = true)
        {
            if (c is Contract && c.Name != null)
            {
                if (Contracts.ContainsKey(c.Name))
                {
                    if (!overwrite)
                    {
                        Console.WriteLine($"Contract {c.Name}/{c.Title} was already present, couldn't save {c.Title}");
                        return false;
                    }
                    else
                        Console.WriteLine($"Contract {c.Name}/{c.Title} was already present, overwriting with {c.Title}");
                }

                Contracts[c.Name] = c;
                return true;
            }

            return false;
        }

        public static bool Add(string name, string title)
        {
            return Add(new Contract(name, title));
        }

        public static string SerializeContracts()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            return JsonConvert.SerializeObject(Contracts.Values, settings);
        }

        internal static Contract[] FindContractsInCfg(List<string[]> list)
        {
            if (list == null || list.Count == 0)
                return null;

            var contractsFound = new List<Contract>();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i][0].Contains("CONTRACT_TYPE"))
                {
                    var block = ConfigBlock.ExtractBlock(list, list[i][0], ref i, true);
                    if (block.type != BlockType.ContractType)
                        continue;

                    var c = new Contract(block.content);

                    if (c != null)
                        contractsFound.Add(c);
                }
            }

            return contractsFound.ToArray();
        }
    }
}
