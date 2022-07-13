using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bundle.Client.Palettes
{
    public static class PacketPaletteHelper
    {
        /// <summary>
        /// Generate a code snippet of updated IDs from a modified packet palette (usually when you have updated packet item position)
        /// </summary>
        /// <example>
        /// You have inserted a new packet type with ID 0x02 into the copied new packet palette:
        /// { 0x00, PacketTypeIn.SpawnEntity },
        /// { 0x01, PacketTypeIn.SpawnExperienceOrb },
        /// { 0xFF, PacketTypeIn.IamNewPacket }, // use 0xFF because it has conflict with old packet ID, we will correct the IDs now
        /// { 0x02, PacketTypeIn.SpawnWeatherEntity },
        /// ...
        /// 
        /// Call this method with your new packet palette:
        /// UpdatePacketIdByItemPosition(new PacketPaletteXXXX(), "code_snippet.txt");
        /// And it will generate a Dictionary format with the Packet IDs corrected for you to copy and paste:
        /// { 0x00, PacketTypeIn.SpawnEntity },
        /// { 0x01, PacketTypeIn.SpawnExperienceOrb },
        /// { 0x02, PacketTypeIn.IamNewPacket },
        /// { 0x03, PacketTypeIn.SpawnWeatherEntity },
        /// ...
        /// </example>
        /// <param name="palette"></param>
        /// <param name="outputFile"></param>
        public static void UpdatePacketIdByItemPosition(PacketTypePalette palette, string outputFile)
        {
            // I am just too tired to create another full .cs file so... please just copy and paste
            List<string> lines = new List<string>();
            lines.Add("=== Inbound Packets ===");
            int i = 0;
            foreach(var t in palette.GetMappingIn())
            {
                lines.Add(string.Format("{{ 0x{0}, {1} }},", i.ToString("X2"), t.Value));
                i++;
            }
            lines.Add("=== End of Inbound ===");
            lines.Add("");
            lines.Add("=== Outbound Packets ===");
            i = 0;
            foreach (var t in palette.GetMappingOut())
            {
                lines.Add(string.Format("{{ 0x{0}, {1} }},", i.ToString("X2"), t.Value));
                i++;
            }
            lines.Add("=== End of Outbound ===");

            File.WriteAllText(outputFile, string.Join("\r\n", lines));
        }

        /// <summary>
        /// Generate a code snippet of rearranged order of packet types from a modified packet palette (usually when you have updated packet IDs)
        /// </summary>
        /// <example>
        /// You have changed some packet IDs:
        /// { 0x00, PacketTypeIn.SpawnEntity },
        /// { 0x02, PacketTypeIn.SpawnExperienceOrb }, // ID changed from 0x02 -> 0x01
        /// { 0x01, PacketTypeIn.SpawnWeatherEntity }, // ID changed from 0x01 -> 0x02
        /// ...
        /// 
        /// Call this method with your new packet palette:
        /// UpdatePacketPositionToAscending(new PacketPaletteXXXX(), "code_snippet.txt");
        /// And it will generate a Dictionary format with the ascending order of Packet IDs for you to copy and paste:
        /// { 0x00, PacketTypeIn.SpawnEntity },
        /// { 0x01, PacketTypeIn.SpawnWeatherEntity },
        /// { 0x02, PacketTypeIn.SpawnExperienceOrb },
        /// ...
        /// </example>
        /// <param name="palette"></param>
        /// <param name="outputFile"></param>
        public static void UpdatePacketPositionToAscending(PacketTypePalette palette, string outputFile)
        {
            // I am just too tired to create another full .cs file so... please just copy and paste
            List<string> lines = new List<string>();
            lines.Add("=== Inbound Packets ===");
            for (int i = 0; i < palette.GetMappingIn().Count; i++)
            {
                lines.Add(string.Format("{{ 0x{0}, {1} }},", i.ToString("X2"), palette.GetMappingIn()[i]));
            }
            lines.Add("=== End of Inbound ===");
            lines.Add("");
            lines.Add("=== Outbound Packets ===");
            for (int i = 0; i < palette.GetMappingOut().Count; i++)
            {
                lines.Add(string.Format("{{ 0x{0}, {1} }},", i.ToString("X2"), palette.GetMappingOut()[i]));
            }
            lines.Add("=== End of Outbound ===");

            File.WriteAllText(outputFile, string.Join("\r\n", lines));
        }

        /// <summary>
        /// Generate PacketPaletteXXX.cs by feeding PacketTypeIn and PacketTypeOut list
        /// </summary>
        /// <param name="outputFile">The output file name</param>
        /// <param name="namespaceToUse">The namespace to use in the generated class</param>
        /// <param name="className">Class name</param>
        /// <param name="pIn">List of PacketTypeIn</param>
        /// <param name="pOut">List of PacketTypeOut</param>
        /// <example>
        /// You need to make sure the order of each item are all correct for that MC version
        /// Example format:
        /// List<PacketTypeIn> pIn = new List<PacketTypeIn>()
        /// {
        ///     PacketTypeIn.SpawnEntity,        // packet ID of 0x00
        ///     PacketTypeIn.SpawnExperienceOrb, // packet ID of 0x01 and so on
        ///     ...
        /// }
        /// </example>
        public static void GenerateIDsPacketByList(string outputFile, string namespaceToUse, string className, List<PacketTypeIn> pIn, List<PacketTypeOut> pOut)
        {
            const string TAB = "    ";
            const string TAB2 = "        ";
            const string TAB3 = "            ";
            List<string> lines = new List<string>();
            lines.Add("using System;");
            lines.Add("using System.Collections.Generic;");
            lines.Add("using System.Linq;");
            lines.Add("using System.Text;");
            lines.Add("");
            lines.Add("namespace " + namespaceToUse);
            lines.Add("{");
            lines.Add(TAB + "public class " + className + " : PacketTypePalette");
            lines.Add(TAB + "{");
            lines.Add(TAB2 + "private Dictionary<int, PacketTypeIn> typeIn = new Dictionary<int, PacketTypeIn>()");
            lines.Add(TAB2 + "{");
            for (int i = 0; i < pIn.Count; i++)
            {
                lines.Add(TAB3 + string.Format("{{ 0x{0}, PacketTypeIn.{1} }},", i.ToString("X2"), pIn[i]));
            }
            lines.Add(TAB2 + "};");
            lines.Add("");
            lines.Add(TAB2 + "private Dictionary<int, PacketTypeOut> typeOut = new Dictionary<int, PacketTypeOut>()");
            lines.Add(TAB2 + "{");
            for (int i = 0; i < pOut.Count; i++)
            {
                lines.Add(TAB3 + string.Format("{{ 0x{0}, PacketTypeOut.{1} }},", i.ToString("X2"), pOut[i]));
            }
            lines.Add(TAB2 + "};");
            lines.Add("");
            lines.Add(TAB2 + "protected override Dictionary<int, PacketTypeIn> GetListIn()");
            lines.Add(TAB2 + "{");
            lines.Add(TAB3 + "return typeIn;");
            lines.Add(TAB2 + "}");
            lines.Add("");
            lines.Add(TAB2 + "protected override Dictionary<int, PacketTypeOut> GetListOut()");
            lines.Add(TAB2 + "{");
            lines.Add(TAB3 + "return typeOut;");
            lines.Add(TAB2 + "}");
            lines.Add(TAB + "}");
            lines.Add("}");
            lines.Add("");

            File.WriteAllText(outputFile, string.Join("\r\n", lines));
        }
    }
}
