using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace TarMod;

public class BlockClayCover : Block, TarPitClayBlock
{
    private const double Margin = 5.0 / 16.0;
    private const double ChannelWidth = 6.0 / 16.0;
    private const double OuterMargin = Margin + ChannelWidth;
    private static string[] nesw = { "n", "e", "s", "w" };
    private Dictionary<(bool, bool, bool, bool), int> companionBlocks = new();
    private int bakedVersion = -1;
    
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (!byPlayer.InventoryManager.ActiveHotbarSlot.Empty) return false;
        return InteractChannel(world, blockSel, Variant["n"].Equals("y"), Variant["e"].Equals("y"), Variant["s"].Equals("y"), Variant["w"].Equals("y"));
    }

    private bool InteractChannel(IWorldAccessor world, BlockSelection blockSel, bool n, bool e, bool s, bool w)
    {
        var x = blockSel.HitPosition.X;
        var z = blockSel.HitPosition.Z;
        if (x is > Margin and < OuterMargin)
        {
            if (z > OuterMargin)
            {
                n = !n;
            } 
            else if (z < Margin)
            {
                s = !s;
            }
            else return false;
        } 
        else if (z is > Margin and < OuterMargin)
        {
            if (x > OuterMargin)
            {
                e = !e;
            } 
            else if (x < Margin)
            {
                w = !w;
            }
            else return false;
        }
        else return false;
        
        world.BlockAccessor.SetBlock(GetCoverBlockFor(world, n, e, s, w), blockSel.Position);
        
        return true;
    }

    private int GetCoverBlockFor(IWorldAccessor world, bool n, bool e, bool s, bool w)
    {
        if (companionBlocks.TryGetValue((n, e, s, w), out var block)) return block;
        
        var path = CodeWithVariants(nesw, new []
        {
            n ? "y" : "n", 
            e ? "y" : "n",  
            s ? "y" : "n", 
            w ? "y" : "n"
        });
        
        return companionBlocks[(n, e, s, w)] = world.GetBlock(path).Id;
    }

    public void Bake(IWorldAccessor world, BlockPos pos)
    {
        if (Variant["clay_state"].Equals("burned")) return;

        if (bakedVersion == -1)
        {
            bakedVersion = world.GetBlock(CodeWithVariant("clay_state", "burned")).Id;
        }
        
        world.BlockAccessor.SetBlock(bakedVersion, pos);
    }
}