using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bundle.Runtime;

namespace Bundle.Client.Palettes
{
    public static class PaletteHelper
    {
        public static PacketTypePalette GetTypeHandler()
        {
            var protocol = GlobalProtocolVersion.Value;
            PacketTypePalette p;
            if (protocol > MinecraftVersion.MC119Version)
                throw new NotImplementedException("NotImplementedException");

            if (protocol <= MinecraftVersion.MC18Version)
                p = new PacketPalette17();
            else if (protocol <= MinecraftVersion.MC1112Version)
                p = new PacketPalette110();
            else if (protocol <= MinecraftVersion.MC112Version)
                p = new PacketPalette112();
            else if (protocol <= MinecraftVersion.MC1122Version)
                p = new PacketPalette1122();
            else if (protocol <= MinecraftVersion.MC114Version)
                p = new PacketPalette113();
            else if (protocol <= MinecraftVersion.MC115Version)
                p = new PacketPalette114();
            else if (protocol <= MinecraftVersion.MC1152Version)
                p = new PacketPalette115();
            else if (protocol <= MinecraftVersion.MC1161Version)
                p = new PacketPalette116();
            else if (protocol <= MinecraftVersion.MC1165Version)
                p = new PacketPalette1162();
            else if (protocol <= MinecraftVersion.MC1171Version)
                p = new PacketPalette117();
            else
                p = new PacketPalette118();

            p.SetForgeEnabled(false);
            return p;
        }
    }
}
