using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace TarMod;

public class CoatingRecipe : IByteSerializable, IRecipeBase<CoatingRecipe>
{
    public int RecipeId;
    private CraftingRecipeIngredient[] ingredients;

    public CraftingRecipeIngredient LiquidIngredient
    {
        get => ingredients[0];
        set => ingredients[0] = value;
    }

    public CraftingRecipeIngredient AffectBlockIngredient
    {
        get => ingredients[1];
        set => ingredients[1] = value;
    }

    public CoatingOutput Output;
    public string Code;
    
    public void ToBytes(BinaryWriter writer)
    {
        writer.Write(Code);
        writer.Write(ingredients.Length);
        for (int index = 0; index < this.ingredients.Length; ++index)
            ingredients[index].ToBytes(writer);
        Output.ToBytes(writer);
    }

    public void FromBytes(BinaryReader reader, IWorldAccessor resolver)
    {
        Code = reader.ReadString();
        ingredients = new CraftingRecipeIngredient[reader.ReadInt32()];
        for (int index = 0; index < ingredients.Length; ++index)
        {
            ingredients[index] = new CraftingRecipeIngredient();
            ingredients[index].FromBytes(reader, resolver);
            ingredients[index].Resolve(resolver, "Coating Recipe (FromBytes)");
        }
        Output = new CoatingOutput();
        Output.FromBytes(reader, resolver);
    }

    public Dictionary<string, string[]> GetNameToCodeMapping(IWorldAccessor world)
    {
        //TODO map wildcards
        throw new System.NotImplementedException();
    }

    public bool Resolve(IWorldAccessor world, string sourceForErrorLogging)
    {
        if (!LiquidIngredient.Resolve(world, sourceForErrorLogging)) return false;
        if (!AffectBlockIngredient.Resolve(world, sourceForErrorLogging)) return false;
        
        return true;
    }

    public CoatingRecipe Clone()
    {
        var clone = new CoatingRecipe();
        clone.Code = Code;
        clone.ingredients = new CraftingRecipeIngredient[ingredients.Length];
        for (int index = 0; index < ingredients.Length; ++index)
        {
            clone.ingredients[index] = ingredients[index].Clone();
        }

        clone.Output = Output.Clone();
        clone.Enabled = Enabled;
        clone.Name = Name;
        clone.RecipeId = RecipeId;
        return clone;
    }

    public AssetLocation Name { get; set; }
    public bool Enabled { get; set; } = true;
    public IRecipeIngredient[] Ingredients => ingredients;
    IRecipeOutput IRecipeBase<CoatingRecipe>.Output => Output;

    public class CoatingOutput : IRecipeOutput, IByteSerializable
    {
        public AssetLocation Code {get; private set;}
        public int BlockId {get; private set;}
        
        
        public void FillPlaceHolder(string key, string value)
        {
            Code = Code.CopyWithPath(Code.Path.Replace("{" + key + "}", value));
        }

        public void ToBytes(BinaryWriter writer)
        {
            writer.Write(Code.Path);
        }

        public void FromBytes(BinaryReader reader, IWorldAccessor resolver)
        {
            Code = AssetLocation.Create(reader.ReadString());
            BlockId = resolver.GetBlock(Code).BlockId;
        }

        public CoatingOutput Clone()
        {
            var clone = new CoatingOutput();
            clone.Code = Code.CopyWithPath(Code.Path);
            clone.BlockId = BlockId;
            return clone;
        }
    }
}