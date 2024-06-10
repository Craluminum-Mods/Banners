using HarmonyLib;
using Vintagestory.API.Common;

namespace Flags;

public class HarmonyPatches : ModSystem
{
    public static ICoreAPI api;

    private Harmony HarmonyInstance => new Harmony(Mod.Info.ModID);

    public override void Start(ICoreAPI _api)
    {
        api = _api;
        HarmonyInstance.Patch(original: BlockBed_OnBlockInteractStart_Patch.TargetMethod(), prefix: typeof(BlockBed_OnBlockInteractStart_Patch).GetMethod("Prefix"));
        HarmonyInstance.Patch(original: ServerSystemBlockSimulation_HandleBlockPlaceOrBreak_Patch.TargetMethod(), prefix: typeof(ServerSystemBlockSimulation_HandleBlockPlaceOrBreak_Patch).GetMethod("Prefix"));
        HarmonyInstance.Patch(original: BlockEntityBed_DidUnmount_Patch.TargetMethod(), transpiler: typeof(BlockEntityBed_DidUnmount_Patch).GetMethod("Transpiler"));
        HarmonyInstance.Patch(original: ColorBlend_ColorBurn_Patch.TargetMethod(), prefix: typeof(ColorBlend_ColorBurn_Patch).GetMethod("Prefix"));
    }

    public override void Dispose()
    {
        HarmonyInstance.Unpatch(original: BlockBed_OnBlockInteractStart_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        HarmonyInstance.Unpatch(original: ServerSystemBlockSimulation_HandleBlockPlaceOrBreak_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        HarmonyInstance.Unpatch(original: BlockEntityBed_DidUnmount_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        HarmonyInstance.Unpatch(original: ColorBlend_ColorBurn_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        BlockEntityBed_DidUnmount_Patch.Applied = false;
    }
}