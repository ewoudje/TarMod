using System.Text.Json.Nodes;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace TarMod;

public class BlockTarPit : BlockLiquidContainerTopOpened, TarPitClayBlock
{
    private int bakedVersion = -1;
    
    protected override string meshRefsCacheKey => "tarpitMeshRefs" + (string) this.Code;
    
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