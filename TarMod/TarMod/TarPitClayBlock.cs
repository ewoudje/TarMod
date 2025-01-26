using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace TarMod;

public interface TarPitClayBlock
{

    void Bake(IWorldAccessor world, BlockPos pos);

}