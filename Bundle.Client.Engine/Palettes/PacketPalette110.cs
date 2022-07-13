using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bundle.Client.Palettes
{
    public class PacketPalette110 : PacketTypePalette
    {
        private Dictionary<int, PacketTypeIn> typeIn = new Dictionary<int, PacketTypeIn>()
        {
            { 0x00, PacketTypeIn.SpawnEntity },
            { 0x01, PacketTypeIn.SpawnExperienceOrb },
            { 0x02, PacketTypeIn.SpawnWeatherEntity },
            { 0x03, PacketTypeIn.SpawnLivingEntity },
            { 0x04, PacketTypeIn.SpawnPainting },
            { 0x05, PacketTypeIn.SpawnPlayer },
            { 0x06, PacketTypeIn.EntityAnimation },
            { 0x07, PacketTypeIn.Statistics },
            { 0x08, PacketTypeIn.BlockBreakAnimation },
            { 0x09, PacketTypeIn.BlockEntityData },
            { 0x0A, PacketTypeIn.BlockAction },
            { 0x0B, PacketTypeIn.BlockChange },
            { 0x0C, PacketTypeIn.BossBar },
            { 0x0D, PacketTypeIn.ServerDifficulty },
            { 0x0E, PacketTypeIn.TabComplete },
            { 0x0F, PacketTypeIn.ChatMessage },
            { 0x10, PacketTypeIn.MultiBlockChange },
            { 0x11, PacketTypeIn.WindowConfirmation },
            { 0x12, PacketTypeIn.CloseWindow },
            { 0x13, PacketTypeIn.OpenWindow },
            { 0x14, PacketTypeIn.WindowItems },
            { 0x15, PacketTypeIn.WindowProperty },
            { 0x16, PacketTypeIn.SetSlot },
            { 0x17, PacketTypeIn.SetCooldown },
            { 0x18, PacketTypeIn.PluginMessage },
            { 0x19, PacketTypeIn.NamedSoundEffect },
            { 0x1A, PacketTypeIn.Disconnect },
            { 0x1B, PacketTypeIn.EntityStatus },
            { 0x1C, PacketTypeIn.Explosion },
            { 0x1D, PacketTypeIn.UnloadChunk },
            { 0x1E, PacketTypeIn.ChangeGameState },
            { 0x1F, PacketTypeIn.KeepAlive },
            { 0x20, PacketTypeIn.ChunkData },
            { 0x21, PacketTypeIn.Effect },
            { 0x22, PacketTypeIn.Particle },
            { 0x23, PacketTypeIn.JoinGame },
            { 0x24, PacketTypeIn.MapData },
            { 0x25, PacketTypeIn.EntityPosition },
            { 0x26, PacketTypeIn.EntityPositionAndRotation },
            { 0x27, PacketTypeIn.EntityRotation },
            { 0x28, PacketTypeIn.EntityMovement },
            { 0x29, PacketTypeIn.VehicleMove },
            { 0x2A, PacketTypeIn.OpenSignEditor },
            { 0x2B, PacketTypeIn.PlayerAbilities },
            { 0x2C, PacketTypeIn.CombatEvent },
            { 0x2D, PacketTypeIn.PlayerInfo },
            { 0x2E, PacketTypeIn.PlayerPositionAndLook },
            { 0x2F, PacketTypeIn.UseBed },
            { 0x30, PacketTypeIn.DestroyEntities },
            { 0x31, PacketTypeIn.RemoveEntityEffect },
            { 0x32, PacketTypeIn.ResourcePackSend },
            { 0x33, PacketTypeIn.Respawn },
            { 0x34, PacketTypeIn.EntityHeadLook },
            { 0x35, PacketTypeIn.WorldBorder },
            { 0x36, PacketTypeIn.Camera },
            { 0x37, PacketTypeIn.HeldItemChange },
            { 0x38, PacketTypeIn.DisplayScoreboard },
            { 0x39, PacketTypeIn.EntityMetadata },
            { 0x3A, PacketTypeIn.AttachEntity },
            { 0x3B, PacketTypeIn.EntityVelocity },
            { 0x3C, PacketTypeIn.EntityEquipment },
            { 0x3D, PacketTypeIn.SetExperience },
            { 0x3E, PacketTypeIn.UpdateHealth },
            { 0x3F, PacketTypeIn.ScoreboardObjective },
            { 0x40, PacketTypeIn.SetPassengers },
            { 0x41, PacketTypeIn.Teams },
            { 0x42, PacketTypeIn.UpdateScore },
            { 0x43, PacketTypeIn.SpawnPosition },
            { 0x44, PacketTypeIn.TimeUpdate },
            { 0x45, PacketTypeIn.Title },
            { 0x46, PacketTypeIn.SoundEffect },
            { 0x47, PacketTypeIn.PlayerListHeaderAndFooter },
            { 0x48, PacketTypeIn.CollectItem },
            { 0x49, PacketTypeIn.EntityTeleport },
            { 0x4A, PacketTypeIn.EntityProperties },
            { 0x4B, PacketTypeIn.EntityEffect },
        };

        private Dictionary<int, PacketTypeOut> typeOut = new Dictionary<int, PacketTypeOut>()
        {
            { 0x00, PacketTypeOut.TeleportConfirm },
            { 0x01, PacketTypeOut.TabComplete },
            { 0x02, PacketTypeOut.ChatMessage },
            { 0x03, PacketTypeOut.ClientStatus },
            { 0x04, PacketTypeOut.ClientSettings },
            { 0x05, PacketTypeOut.WindowConfirmation },
            { 0x06, PacketTypeOut.EnchantItem },
            { 0x07, PacketTypeOut.ClickWindow },
            { 0x08, PacketTypeOut.CloseWindow },
            { 0x09, PacketTypeOut.PluginMessage },
            { 0x0A, PacketTypeOut.InteractEntity },
            { 0x0B, PacketTypeOut.KeepAlive },
            { 0x0C, PacketTypeOut.PlayerPosition },
            { 0x0D, PacketTypeOut.PlayerPositionAndRotation },
            { 0x0E, PacketTypeOut.PlayerRotation },
            { 0x0F, PacketTypeOut.PlayerMovement },
            { 0x10, PacketTypeOut.VehicleMove },
            { 0x11, PacketTypeOut.SteerBoat },
            { 0x12, PacketTypeOut.PlayerAbilities },
            { 0x13, PacketTypeOut.PlayerDigging },
            { 0x14, PacketTypeOut.EntityAction },
            { 0x15, PacketTypeOut.SteerVehicle },
            { 0x16, PacketTypeOut.ResourcePackStatus },
            { 0x17, PacketTypeOut.HeldItemChange },
            { 0x18, PacketTypeOut.CreativeInventoryAction },
            { 0x19, PacketTypeOut.UpdateSign },
            { 0x1A, PacketTypeOut.Animation },
            { 0x1B, PacketTypeOut.Spectate },
            { 0x1C, PacketTypeOut.PlayerBlockPlacement },
            { 0x1D, PacketTypeOut.UseItem },
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
