using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace TarMod;

public class BlockEntityTarPit : BlockEntityLiquidContainer
{
    private const int burnModifier = 4;
    
    public override string InventoryClassName => "tarpit";

    private MeshData currentMesh;
    
    public BlockEntityTarPit()
    {
      inventory = new InventoryGeneric(1, (string)null, (ICoreAPI) null, (idx, i) => new ItemSlotLiquidOnly(i, 25f));
    }
    
    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        
        if (Api.Side != EnumAppSide.Client)
            return;
        currentMesh = this.GenMesh();
        MarkDirty(true);
    }
    
    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
      ItemSlot itemSlot = inventory[0];
      if (itemSlot.Empty)
        dsc.AppendLine(Lang.Get("Empty"));
      else
        dsc.AppendLine(Lang.Get("Contents: {0}x{1}", itemSlot.Itemstack.StackSize, itemSlot.Itemstack.GetName()));
    }
    
    private MeshData GenMesh()
    {
        return (Block as BlockTarPit).GenMesh(this.Api as ICoreClientAPI, this.GetContent(), this.Pos);
    }

    public override void OnBlockPlaced(ItemStack byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);
        if (Api.Side != EnumAppSide.Client)
            return;
        currentMesh = GenMesh();
        MarkDirty(true);
    }
    
    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        if (currentMesh != null)
            mesher.AddMeshData(currentMesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0.0f, 0.0f, 0.0f));
        return true;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        ICoreAPI api = Api;
        if ((api != null ? (api.Side == EnumAppSide.Client ? 1 : 0) : 0) == 0)
            return;
        currentMesh = GenMesh();
        MarkDirty(true);
    }
  
    
    private static BlockEntityTarPit Traverse(IBlockAccessor accessor, BlockPos startPos, Dictionary<BlockPos, (int, BlockEntityTarPit)> cache)
    {
        var queue = new Queue<(BlockPos, int)>();
        var bestDistance = int.MaxValue;
        BlockEntityTarPit bestTarPit = null;
        var visited = new HashSet<BlockPos>();
        
        queue.Enqueue((startPos, 0));

        BlockEntityTarPit result = null;

        while (queue.Count > 0)
        {
            var (pos, distance) = queue.Dequeue();
            visited.Add(pos);

            if (cache.TryGetValue(pos, out var v))
            {
                if (v.Item2 != null && v.Item1 < bestDistance)
                {
                    bestDistance = distance;
                    bestTarPit = v.Item2;
                }
                
                continue;
            }
            
            var block = accessor.GetBlock(pos);
            if (block is not BlockClayCover cover)
            {
                if (block is not BlockTarPit) continue;
            
                var be = accessor.GetBlockEntity(pos);
                if (be is not BlockEntityTarPit tarPit) continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarPit = tarPit;
                }
                
                continue;
            }

            BlockPos copy;

            if (cover.Variant["n"].EqualsFast("y") && !visited.Contains(copy = pos.NorthCopy()))
                queue.Enqueue((copy, distance + 1));

            if (cover.Variant["e"].EqualsFast("y") && !visited.Contains(copy = pos.EastCopy()))
                queue.Enqueue((copy, distance + 1));

            if (cover.Variant["s"].EqualsFast("y") && !visited.Contains(copy = pos.SouthCopy()))
                queue.Enqueue((copy, distance + 1));

            if (cover.Variant["w"].EqualsFast("y") && !visited.Contains(copy = pos.WestCopy()))
                queue.Enqueue((copy, distance + 1));
        }
        
        cache.Add(startPos, (bestDistance, bestTarPit));
        return bestTarPit;
    }
    
    public void FireWoodBurned(int quantity)
    {
        if (inventory[0].Empty)
            inventory[0].Itemstack = new ItemStack(Api.World.GetItem(new AssetLocation("game", "tarportion")), quantity * burnModifier);
        else
            inventory[0].TryPutInto(Api.World, inventory[0], quantity * burnModifier);
        
        MarkDirty();
    }
    
    public static BlockEntityTarPit MostNearbyPit(IWorldAccessor world, BlockPos pos, Dictionary<BlockPos, (int, BlockEntityTarPit)> cache = null)
    {
        cache ??= new Dictionary<BlockPos, (int, BlockEntityTarPit)>();
        return Traverse(world.BlockAccessor, pos, cache);
    }
    
}