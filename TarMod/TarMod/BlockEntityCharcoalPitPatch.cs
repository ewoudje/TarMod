using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace TarMod;

[HarmonyPatch(typeof(BlockEntityCharcoalPit))]
[HarmonyPatch("ConvertPit")]
public class BlockEntityCharcoalPitPatch
{
    
    [HarmonyDebug]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        for (int i = 0; i < list.Count; i++)
        {
            var instruction = list[i];
            if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder { LocalIndex: 5 })
            {
                list.Insert(i + 1, CodeInstruction.LoadArgument(0));
                list.Insert(i + 2, CodeInstruction.LoadLocal(1));
                list.Insert(i + 3, CodeInstruction.Call(() => ConvertWithLocals(default, default)));
                break;
            }
        }

        return list.AsEnumerable();
    }
    
    
    public static void ConvertWithLocals(BlockEntityCharcoalPit instance, HashSet<BlockPos> pos)
    {
        var world = instance.Api.World;
        try
        {
            var cache = new Dictionary<BlockPos, (int, BlockEntityTarPit)>();
            foreach (var p in pos)
            {
                Block b = null;
                var amount = BlockFirepit.GetFireWoodQuanity(world, p);
                for (int i = 0; i < 9; i++)
                {
                    b = world.BlockAccessor.GetBlock(p.Down());
                    if (b is TarPitClayBlock) break;
                }

                if (b is not TarPitClayBlock bake) continue;
                bake.Bake(world, p);

                BlockEntityTarPit tar = null;
                
                try
                {
                    tar = BlockEntityTarPit.MostNearbyPit(world, p, cache);
                }
                catch (Exception e)
                {
                    world.Logger.Warning(e);
                }
                
                tar?.FireWoodBurned(amount);
            }
        }
        catch (Exception e)
        {
            world.Logger.Error(e);
        }
    }
}