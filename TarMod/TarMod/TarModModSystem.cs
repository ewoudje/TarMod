using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace TarMod;

[HarmonyPatch]
public class TarModModSystem : ModSystem {
    public Harmony Harmony;
    private List<CoatingRecipe> coatingRecipes;
    private Dictionary<int, Dictionary<int, CoatingRecipe>> coatingRecipesDictionary;

    public override void AssetsLoaded(ICoreAPI api)
    {
        if (api is not ICoreServerAPI sapi)
            return;

        var recipeLoader = sapi.ModLoader.GetModSystem<RecipeLoader>();
        //recipeLoader.LoadRecipes<CoatingRecipe>("coating", "recipes/coating", AddCoatingRecipe);
    }

    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockClass("BlockClayCover", typeof(BlockClayCover));
        api.RegisterBlockClass("BlockTarPit", typeof(BlockTarPit));
        api.RegisterBlockEntityClass("BlockEntityTarPit", typeof(BlockEntityTarPit));
        
        api.RegisterItemClass("ItemClayLayer", typeof(ItemClayLayer));
        api.RegisterItemClass("ItemTarPit", typeof(ItemTarPit));
        api.RegisterItemClass("ItemBrush", typeof(ItemBrush));

        //coatingRecipes = api.RegisterRecipeRegistry<RecipeRegistryGeneric<CoatingRecipe>>("coating").Recipes;
        
        if (!Harmony.HasAnyPatches(Mod.Info.ModID)) {
            Harmony = new Harmony(Mod.Info.ModID);
            Harmony.PatchAll(); // Applies all harmony patches
        }
        
    }
    
    public override void Dispose() {
        Harmony?.UnpatchAll(Mod.Info.ModID);
    }

    public void AddCoatingRecipe(CoatingRecipe recipe)
    {
        coatingRecipes.Add(recipe);
        var liquid = recipe.LiquidIngredient.ResolvedItemstack.Id;
        var block = recipe.AffectBlockIngredient.ResolvedItemstack.Id;

        Dictionary<int, CoatingRecipe> blockDictonary;
        if (!coatingRecipesDictionary.TryGetValue(liquid, out blockDictonary))
        {
            blockDictonary = new Dictionary<int, CoatingRecipe>();
            coatingRecipesDictionary.Add(liquid, blockDictonary);
        }
        
        blockDictonary[block] = recipe;
    }
    
    
}