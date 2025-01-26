using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace TarMod;

public class ItemTarPit : Item
{
    private Dictionary<string, int> cache = new();
    
    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        handling = EnumHandHandling.NotHandled;
        
        if (blockSel.Face == BlockFacing.DOWN) return;
        var block = blockSel.Block;
        var world = byEntity.World;
        block ??= world.BlockAccessor.GetBlock(blockSel.Position);
        
        if (block == null) return;
        if (!block.Code.PathStartsWith("soil-")) return;
        
        var fertility = block.Variant["fertility"];
        var newBlock = 0;
        if (!cache.TryGetValue(fertility, out newBlock))
        {
            newBlock = world.GetBlock(AssetLocation.Create("tarpit-raw-" + Variant["type"] + "-" + fertility, "tarmod"))
                .Id;
            
            cache.Add(fertility, newBlock);
        }
        
        world.BlockAccessor.SetBlock(newBlock, blockSel.Position);
        handling = EnumHandHandling.Handled;
        
        slot.TakeOut(1);
    }
}