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
public class BlockEntityCharcoalPitPatch
{
    
    [HarmonyTranspiler]
    [HarmonyPatch("ConvertPit")]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var hit = false;
        for (int i = 0; i < list.Count; i++)
        {
            var instruction = list[i];
            if (instruction.opcode == OpCodes.Stloc_3)
            {
                // + 3 is after loading the wierd field
                list.Insert(i + 3, new CodeInstruction(OpCodes.Dup));
                list.Insert(i + 4, CodeInstruction.LoadArgument(0));
                list.Insert(i + 5, CodeInstruction.Call(() => ConvertWithLocals(null, null)));
                hit = true;
                break;
            }
        }

        if (!hit)
            throw new Exception("BlockEntityCharcoalPitPatch Transpiler failed, if you are using this with the correct *matching* vintage story version, please report this to the developer.");

        return list.AsEnumerable();
    }
    
    
    public static void ConvertWithLocals(Dictionary<BlockPos, Vec3i> pos, BlockEntityCharcoalPit instance)
    {
        var world = instance.Api.World;
        try
        {
            var cache = new Dictionary<BlockPos, (int, BlockEntityTarPit)>();
            foreach (var keyPair in pos)
            {
                Block b = null;
                BlockPos p = new(keyPair.Key.X, keyPair.Value.Y, keyPair.Key.Z);
                var amount = keyPair.Value.X;
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