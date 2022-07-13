using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bundle.Client.Palettes
{
    /// <summary>
    /// Packet type palette
    /// </summary>
    /// <remarks>
    /// Steps for implementing palette for new Minecraft version:
    /// 1. Check out https://wiki.vg/Pre-release_protocol to see if there is any packet got added/removed
    /// 2. Add new packet type to PacketTypeIn.cs and PacketTypeOut.cs (if any)
    /// 3. Create a new PacketPaletteXXX.cs by copying the latest version of existing PacketPaletteXXX.cs (could reduce massive works on writing a brand new one)
    /// 4. Apply change to the copied PacketPaletteXXX.cs by either:
    ///     - Inserting new packet type to the correct position
    ///     - Removing packet type that got deleted
    ///    OR
    ///     - Changing the packet IDs manually
    /// 5. Use PacketPaletteHelper to generate a code snippet and copy the generated code snippet back to PacketPaletteXXX.cs
    ///     - Use UpdatePacketPositionToAscending() if you changed the packet IDs manually
    ///     - Use UpdatePacketIdByItemPosition() if you inserted some packet type into the dictionary
    ///    Simply add the method call in Program.cs and run the program once. The code snippet will be generated
    /// 
    /// 
    /// The way how Mojang change the packet ID is simple: 
    ///  * Either adding/removing a packet from middle and cause packet ID below it get shifted
    ///  * Append a new packet at the end (but this is rare)
    /// </remarks>
    public abstract class PacketTypePalette
    {
        protected abstract Dictionary<int, PacketTypeIn> GetListIn();
        protected abstract Dictionary<int, PacketTypeOut> GetListOut();

        private Dictionary<PacketTypeIn, int> reverseMappingIn = new Dictionary<PacketTypeIn, int>();

        private Dictionary<PacketTypeOut, int> reverseMappingOut = new Dictionary<PacketTypeOut, int>();

        private bool forgeEnabled = false;

        public PacketTypePalette()
        {
            foreach (var p in GetListIn())
            {
                reverseMappingIn.Add(p.Value, p.Key);
            }
            foreach (var p in GetListOut())
            {
                reverseMappingOut.Add(p.Value, p.Key);
            }
        }

        /// <summary>
        /// Get incomming packet type by packet ID
        /// </summary>
        /// <param name="packetId">packet ID</param>
        /// <returns>Packet type</returns>
        public PacketTypeIn GetIncommingTypeById(int packetId)
        {
            PacketTypeIn p;
            if (GetListIn().TryGetValue(packetId, out p))
            {
                return p;
            }
            else if (forgeEnabled)
            {
                return PacketTypeIn.Unknown;
            }
            else
                throw new KeyNotFoundException("Packet ID of 0x" + packetId.ToString("X2") + " doesn't exist!");
        }

        /// <summary>
        /// Get incomming packet ID by packet type
        /// </summary>
        /// <param name="packetType">Packet type</param>
        /// <returns>packet ID</returns>
        public int GetIncommingIdByType(PacketTypeIn packetType)
        {
            return reverseMappingIn[packetType];
        }

        /// <summary>
        /// Get outgoing packet type by packet ID
        /// </summary>
        /// <param name="packetId">Packet ID</param>
        /// <returns>Packet type</returns>
        public PacketTypeOut GetOutgoingTypeById(int packetId)
        {
            PacketTypeOut p;
            if (GetListOut().TryGetValue(packetId, out p))
            {
                return p;
            }
            else if (forgeEnabled)
            {
                return PacketTypeOut.Unknown;
            }
            else
                throw new KeyNotFoundException("Packet ID of 0x" + packetId.ToString("X2") + " doesn't exist!");
        }

        /// <summary>
        /// Get outgoing packet ID by packet type
        /// </summary>
        /// <param name="packetType">Packet type</param>
        /// <returns>Packet ID</returns>
        public int GetOutgoingIdByType(PacketTypeOut packetType)
        {
            return reverseMappingOut[packetType];
        }


        /// <summary>
        /// Public method for getting the type mapping
        /// </summary>
        /// <returns>PacketTypeIn with packet ID as index</returns>
        public Dictionary<int, PacketTypeIn> GetMappingIn()
        {
            return GetListIn();
        }

        /// <summary>
        /// Public method for getting the type mapping
        /// </summary>
        /// <returns>PacketTypeOut with packet ID as index</returns>
        public Dictionary<int ,PacketTypeOut> GetMappingOut()
        {
            return GetListOut();
        }

        /// <summary>
        /// Enable forge or disable forge
        /// </summary>
        /// <remarks>
        /// Have a rare chance that forge mod may modify packet ID.
        /// Ignore packet type not found when forge enabled to
        /// prevent program crash.
        /// </remarks>
        /// <param name="enabled"></param>
        public void SetForgeEnabled(bool enabled)
        {
            this.forgeEnabled = enabled;
        }
    }
}
