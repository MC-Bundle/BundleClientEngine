using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bundle.Client.Palettes
{
    public class PacketPalette17 : PacketTypePalette
    {
        private Dictionary<int, PacketTypeIn> typeIn = new Dictionary<int, PacketTypeIn>()
        {
            { 0x00, PacketTypeIn.KeepAlive },
            { 0x01, PacketTypeIn.JoinGame },
            { 0x02, PacketTypeIn.ChatMessage },
            { 0x03, PacketTypeIn.TimeUpdate },
            { 0x04, PacketTypeIn.EntityEquipment },
            { 0x05, PacketTypeIn.SpawnPosition },
            { 0x06, PacketTypeIn.UpdateHealth },
            { 0x07, PacketTypeIn.Respawn },
            { 0x08, PacketTypeIn.PlayerPositionAndLook },
            { 0x09, PacketTypeIn.HeldItemChange },
            { 0x0A, PacketTypeIn.UseBed },
            { 0x0B, PacketTypeIn.EntityAnimation },
            { 0x0C, PacketTypeIn.SpawnPlayer },
            { 0x0D, PacketTypeIn.CollectItem },
            { 0x0E, PacketTypeIn.SpawnEntity },
            { 0x0F, PacketTypeIn.SpawnLivingEntity },
            { 0x10, PacketTypeIn.SpawnPainting },
            { 0x11, PacketTypeIn.SpawnExperienceOrb },
            { 0x12, PacketTypeIn.EntityVelocity },
            { 0x13, PacketTypeIn.DestroyEntities },
            { 0x14, PacketTypeIn.EntityMovement },
            { 0x15, PacketTypeIn.EntityPosition },
            { 0x16, PacketTypeIn.EntityRotation },
            { 0x17, PacketTypeIn.EntityPositionAndRotation },
            { 0x18, PacketTypeIn.EntityTeleport },
            { 0x19, PacketTypeIn.EntityHeadLook },
            { 0x1A, PacketTypeIn.EntityStatus },
            { 0x1B, PacketTypeIn.AttachEntity },
            { 0x1C, PacketTypeIn.EntityMetadata },
            { 0x1D, PacketTypeIn.EntityEffect },
            { 0x1E, PacketTypeIn.RemoveEntityEffect },
            { 0x1F, PacketTypeIn.SetExperience },
            { 0x20, PacketTypeIn.EntityProperties },
            { 0x21, PacketTypeIn.ChunkData },
            { 0x22, PacketTypeIn.MultiBlockChange },
            { 0x23, PacketTypeIn.BlockChange },
            { 0x24, PacketTypeIn.BlockAction },
            { 0x25, PacketTypeIn.BlockBreakAnimation },
            { 0x26, PacketTypeIn.MapChunkBulk },
            { 0x27, PacketTypeIn.Explosion },
            { 0x28, PacketTypeIn.Effect },
            { 0x29, PacketTypeIn.SoundEffect },
            { 0x2A, PacketTypeIn.Particle },
            { 0x2B, PacketTypeIn.ChangeGameState },
            { 0x2C, PacketTypeIn.SpawnWeatherEntity },
            { 0x2D, PacketTypeIn.OpenWindow },
            { 0x2E, PacketTypeIn.CloseWindow },
            { 0x2F, PacketTypeIn.SetSlot },
            { 0x30, PacketTypeIn.WindowItems },
            { 0x31, PacketTypeIn.WindowProperty },
            { 0x32, PacketTypeIn.WindowConfirmation },
            { 0x33, PacketTypeIn.UpdateSign },
            { 0x34, PacketTypeIn.MapData },
            { 0x35, PacketTypeIn.BlockEntityData },
            { 0x36, PacketTypeIn.OpenSignEditor },
            { 0x37, PacketTypeIn.Statistics },
            { 0x38, PacketTypeIn.PlayerInfo },
            { 0x39, PacketTypeIn.PlayerAbilities },
            { 0x3A, PacketTypeIn.TabComplete },
            { 0x3B, PacketTypeIn.ScoreboardObjective },
            { 0x3C, PacketTypeIn.UpdateScore },
            { 0x3D, PacketTypeIn.DisplayScoreboard },
            { 0x3E, PacketTypeIn.Teams },
            { 0x3F, PacketTypeIn.PluginMessage },
            { 0x40, PacketTypeIn.Disconnect },
            { 0x41, PacketTypeIn.ServerDifficulty },
            { 0x42, PacketTypeIn.CombatEvent },
            { 0x43, PacketTypeIn.Camera },
            { 0x44, PacketTypeIn.WorldBorder },
            { 0x45, PacketTypeIn.Title },
            { 0x46, PacketTypeIn.SetCompression },
            { 0x47, PacketTypeIn.PlayerListHeaderAndFooter },
            { 0x48, PacketTypeIn.ResourcePackSend },
            { 0x49, PacketTypeIn.UpdateEntityNBT },
        };

        private Dictionary<int, PacketTypeOut> typeOut = new Dictionary<int, PacketTypeOut>()
        {
            { 0x00, PacketTypeOut.KeepAlive },
            { 0x01, PacketTypeOut.ChatMessage },
            { 0x02, PacketTypeOut.InteractEntity },
            { 0x03, PacketTypeOut.PlayerMovement },
            { 0x04, PacketTypeOut.PlayerPosition },
            { 0x05, PacketTypeOut.PlayerRotation },
            { 0x06, PacketTypeOut.PlayerPositionAndRotation },
            { 0x07, PacketTypeOut.PlayerDigging },
            { 0x08, PacketTypeOut.PlayerBlockPlacement },
            { 0x09, PacketTypeOut.HeldItemChange },
            { 0x0A, PacketTypeOut.Animation },
            { 0x0B, PacketTypeOut.EntityAction },
            { 0x0C, PacketTypeOut.SteerVehicle },
            { 0x0D, PacketTypeOut.CloseWindow },
            { 0x0E, PacketTypeOut.ClickWindow },
            { 0x0F, PacketTypeOut.WindowConfirmation },
            { 0x10, PacketTypeOut.CreativeInventoryAction },
            { 0x11, PacketTypeOut.EnchantItem },
            { 0x12, PacketTypeOut.UpdateSign },
            { 0x13, PacketTypeOut.PlayerAbilities },
            { 0x14, PacketTypeOut.TabComplete },
            { 0x15, PacketTypeOut.ClientSettings },
            { 0x16, PacketTypeOut.ClientStatus },
            { 0x17, PacketTypeOut.PluginMessage },
            { 0x18, PacketTypeOut.Spectate },
            { 0x19, PacketTypeOut.ResourcePackStatus },
        };

        protected override Dictionary<int, PacketTypeIn> GetListIn()
        {
            return typeIn;
        }

        protected override Dictionary<int, PacketTypeOut> GetListOut()
        {
            return typeOut;
        }
    }
}
