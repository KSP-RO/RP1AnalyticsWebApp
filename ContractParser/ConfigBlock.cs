using System.Collections.Generic;

namespace ContractParser
{
    public enum BlockType
    {
        ContractType,
        Requirement,
        Behaviour,
        Parameter,
        DataExpand,
        Unknown
    }
    public class ConfigBlock
    {
        public string name;
        public BlockType type = BlockType.Unknown;
        public List<string[]> content;
        public List<ConfigBlock> childrenBlocks;

        public ConfigBlock(List<string[]> block)
        {
            if (block == null || block.Count == 0)
                return;

            if (block[0][0].Contains("CONTRACT_TYPE"))
                type = BlockType.ContractType;
            else if (block[0][0].Contains("PARAMETER"))
                type = BlockType.Parameter;
            else if (block[0][0].Contains("BEHAVIOUR"))
                type = BlockType.Behaviour;
            else if (block[0][0].Contains("REQUIREMENT"))
                type = BlockType.Requirement;
            else if (block[0][0].Contains("DATA_EXPAND"))
                type = BlockType.DataExpand;
            else
                type = BlockType.Unknown;

            block.RemoveRange(0, 2);            // remove first and second line (node name and opening bracket)
            block.RemoveAt(block.Count - 1);    // remove closing bracket

            content = block;

            PopulateChildrenBlocks();

            foreach (var line in block)
            {
                if (line.Length == 2 && line[0] == "name")
                {
                    name = line[1];
                    break;
                }
            }
        }

        public static ConfigBlock ExtractBlock(List<string[]> input, string blockStart, ref int index, bool remove = false)
        {
            int blockStartIndex = index;
            int length = 0;
            uint bracketCounter = 0;

            for (int i = index; i < input.Count; i++)
            {
                string[] line = input[i];

                if (line[0] == "{")
                {
                    bracketCounter++;
                }
                else if (line[0] == "}" && i > blockStartIndex && --bracketCounter == 0)
                {
                    length = (i - blockStartIndex) + 1;
                    break;
                }
            }

            List<string[]> block = input.GetRange(blockStartIndex, length);

            if (length > 0)
            {
                if (remove)
                    input.RemoveRange(blockStartIndex, length);
                index--;
            }
            return new ConfigBlock(block);
        }

        public string GetFieldValue(string field)
        {
            if (string.IsNullOrEmpty(field) || content == null || content.Count == 0)
                return null;

            foreach (var line in content)
            {
                string s = line[0].RemoveOperator();
                if (line.Length == 2 && s.Contains(field))
                {
                    return line[1];
                }
            }

            return null;
        }

        public string[] GetFieldValues(string field)
        {
            if (string.IsNullOrEmpty(field) || content == null || content.Count == 0)
                return null;

            var list = new List<string>();

            foreach (var line in content)
            {
                string s = line[0].RemoveOperator();
                if (line.Length == 2 && s.Contains(field))
                {
                    list.Add(line[1]);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Recursively finds children blocks from the parent
        /// </summary>
        private void PopulateChildrenBlocks()
        {
            childrenBlocks = new List<ConfigBlock>();

            if (content == null || content.Count <= 3)
                return;

            uint bracketCounter = 0;
            int blockStartIndex = content.Count;
            int length;
            for (int i = 0; i < content.Count; i++)
            {
                string[] line = content[i];

                if (line[0] == "{")
                {
                    bracketCounter++;

                    if (bracketCounter == 1)
                        blockStartIndex = i - 1;
                }
                if (line[0] == "}" && i > blockStartIndex && --bracketCounter == 0)
                {
                    length = (i - blockStartIndex) + 1;

                    if (length > 0)
                    {
                        childrenBlocks.Add(ExtractBlock(content, content[blockStartIndex][0], ref blockStartIndex, true));
                        i -= length;
                    }
                }
            }
        }
    }
}
