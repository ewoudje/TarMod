using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace TarMod;

public class ItemClayLayer : Item
{
    private string fertilityOfCache = String.Empty;
    private Dictionary<BlockFacing, int> cache = new();
    
    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        handling = EnumHandHandling.NotHandled;
        
        if (blockSel.Face == BlockFacing.DOWN) return;
        //TODO
        if (blockSel.Face != BlockFacing.UP) return;
        
        var block = blockSel.Block;
        var world = byEntity.World;
        block ??= world.BlockAccessor.GetBlock(blockSel.Position);
        
        if (block == null) return;
        if (!block.Code.PathStartsWith("soil-")) return;
        
        var fertility = block.Variant["fertility"];
        if (!fertilityOfCache.Equals(fertility))
        {
            cache.Clear();
            fertilityOfCache = fertility;
        }
        
        world.BlockAccessor.SetBlock(GetCachedBlock(world, blockSel.Face), blockSel.Position);
        handling = EnumHandHandling.Handled;
        
        slot.TakeOut(1);
    }

    private int GetCachedBlock(IWorldAccessor world, BlockFacing face)
    {
        if (cache.TryGetValue(face, out int value)) return value;
        var start = new StringBuilder();
        if (face == BlockFacing.UP)
        {
            start.Append("claycoveredbottom-n-n-n-n");
        }
        else
        {
            start.Append("claycoveredside-");
            start.Append(face);
        }

        start.Append("-raw-");
        start.Append(Variant["type"]);
        start.Append('-');
        start.Append(fertilityOfCache);
        
        return cache[face] = world.GetBlock(AssetLocation.Create(start.ToString(), "tarmod")).Id;
    }
}