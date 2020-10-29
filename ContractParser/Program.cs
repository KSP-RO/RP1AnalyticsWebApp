using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ContractParser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string inputPath = null;
            string outputPath = null;
            string outputFileName = "ContractData";

            if (args.Length == 1)
                inputPath = args[0];
            else if (args.Length > 1)
            {
                inputPath = args[0];
                outputPath = args[1];
            }

            if (string.IsNullOrEmpty(inputPath))
            {
                Console.Write("Enter contracts folder path: ");
                inputPath = Console.ReadLine();
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                Console.Write("Enter json output folder path: ");
                outputPath = Console.ReadLine();
            }

            while (ContractHandler.Count == 0 && !LoadAllFiles(inputPath))
            {
                Console.WriteLine("ERROR: No configs found in folder");
                Console.Write("Enter the path of the folder containing your contracts: ");
                inputPath = Console.ReadLine();
            }

            string fullOutPath = Path.GetFullPath(outputPath);

            if (fullOutPath != null)
            {
                Directory.CreateDirectory(fullOutPath);
                string FileName = $"{fullOutPath}/{outputFileName}.json";
                try
                {
                    File.WriteAllText(FileName, ContractHandler.SerializeContracts());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }
            }

            Console.WriteLine($"Successfully created json in {fullOutPath} from {ContractHandler.Count} parsed contracts");
            return;
        }

        private static bool LoadAllFiles(string folderPath)
        {
            Console.WriteLine($"Parsing contracts from {folderPath}...");
            IEnumerable<string> filePaths = Directory.EnumerateFiles(folderPath, "*.cfg", SearchOption.AllDirectories);

            if (filePaths.Count() > 0)
            {
                foreach (string file in filePaths)
                {
                    foreach (Contract c in LoadContracts(file) ?? Enumerable.Empty<Contract>())
                    {
                        if (!string.IsNullOrWhiteSpace(c.Name))
                            ContractHandler.Add(c);
                    }
                }
                return true;
            }
            else
                return false;
        }

        private static Contract[] LoadContracts(string fileFullName)
        {
            if (!File.Exists(fileFullName))
            {
                Console.WriteLine("File '" + fileFullName + "' does not exist");
                return null;
            }
            return LoadFromStringArray(File.ReadAllLines(fileFullName));
        }

        private static Contract[] LoadFromStringArray(string[] cfgData)
        {
            if (cfgData == null)
            {
                return null;
            }
            List<string[]> list = PreFormatConfig(cfgData);
            if (list != null && list.Count != 0)
            {
                Contract[] cc = ContractHandler.FindContractsInCfg(list);
                return cc;
            }
            return null;
        }

        private static List<string[]> PreFormatConfig(string[] cfgData)
        {
            if (cfgData != null && cfgData.Length >= 1)
            {
                List<string> list = new List<string>(cfgData);
                int num = list.Count;
                while (--num >= 0)
                {
                    // Remove comments
                    int num2;
                    if ((num2 = list[num].IndexOf("//")) != -1)
                    {
                        if (num2 == 0)
                        {
                            list.RemoveAt(num);
                            continue;
                        }
                        list[num] = list[num].Remove(num2);
                    }

                    // Trim line and remove line if empty
                    list[num] = list[num].Trim();
                    if (list[num].Length == 0)
                    {
                        list.RemoveAt(num);
                    }
                    else if (
                        (num2 = list[num].IndexOf("}", 0)) != -1 &&
                        (num2 != 0 || list[num].Length != 1)
                    )
                    {
                        if (num2 > 0)
                        {
                            list.Insert(num, list[num].Substring(0, num2));
                            num++;
                            list[num] = list[num].Substring(num2);
                            num2 = 0;
                        }
                        if (num2 < list[num].Length - 1)
                        {
                            list.Insert(num + 1, list[num].Substring(num2 + 1));
                            list[num] = "}";
                            num += 2;
                        }
                    }
                    else if (
                        (num2 = list[num].IndexOf("{", 0)) != -1 &&
                        (num2 != 0 || list[num].Length != 1)
                    )
                    {
                        if (num2 > 0)
                        {
                            list.Insert(num, list[num].Substring(0, num2));
                            num++;
                            list[num] = list[num].Substring(num2);
                            num2 = 0;
                        }
                        if (num2 < list[num].Length - 1)
                        {
                            list.Insert(num + 1, list[num].Substring(num2 + 1));
                            list[num] = "{";
                            num += 2;
                        }
                    }
                }

                List<string[]> list2 = new List<string[]>(list.Count);
                int i = 0;
                for (int count = list.Count; i < count; i++)
                {
                    string[] array = list[i].Split(new char[1] { '=' }, 2);
                    if (array != null && array.Length != 0)
                    {
                        int j = 0;
                        for (int num3 = array.Length; j < num3; j++)
                        {
                            array[j] = array[j].Trim();
                        }
                        list2.Add(array);
                    }
                }
                return list2;
            }
            Console.WriteLine("Error: Empty engine config file");
            return null;
        }
    }
}
