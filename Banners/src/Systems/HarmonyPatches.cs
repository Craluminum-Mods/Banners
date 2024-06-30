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
        HarmonyInstance.CreateReversePatcher(original: GuiHandbookItemStackPage_PageCodeForStack_Patch.TargetMethod(), standin: GuiHandbookItemStackPage_PageCodeForStack_Patch.GetBase()).Patch(HarmonyReversePatchType.Original);

        HarmonyInstance.Patch(original: BlockBed_OnBlockInteractStart_Patch.TargetMethod(), prefix: BlockBed_OnBlockInteractStart_Patch.GetPrefix());
        HarmonyInstance.Patch(original: ServerSystemBlockSimulation_HandleBlockPlaceOrBreak_Patch.TargetMethod(), prefix: ServerSystemBlockSimulation_HandleBlockPlaceOrBreak_Patch.GetPrefix());
        HarmonyInstance.Patch(original: BlockEntityBed_DidUnmount_Patch.TargetMethod(), transpiler: BlockEntityBed_DidUnmount_Patch.GetTranspiler());
        HarmonyInstance.Patch(original: ColorBlend_ColorBurn_Patch.TargetMethod(), prefix: ColorBlend_ColorBurn_Patch.GetPrefix());
        HarmonyInstance.Patch(original: GuiHandbookItemStackPage_PageCodeForStack_Patch.TargetMethod(), postfix: GuiHandbookItemStackPage_PageCodeForStack_Patch.GetPostfix());

        HarmonyInstance.Patch(original: MechNetworkRenderer_AddDevice_Patch.TargetMethod(), prefix: MechNetworkRenderer_AddDevice_Patch.GetPrefix());
        HarmonyInstance.Patch(original: BlockWindmillRotor_OnBlockInteractStart_Patch.TargetMethod(), prefix: BlockWindmillRotor_OnBlockInteractStart_Patch.GetPrefix());
        if (api.Side.IsClient())
        {
            HarmonyInstance.Patch(original: BlockWindmillRotor_GetPlacedBlockInteractionHelp_Patch.TargetMethod(), postfix: BlockWindmillRotor_GetPlacedBlockInteractionHelp_Patch.GetPostfix());
        }
    }

    public override void Dispose()
    {
        HarmonyInstance.Unpatch(original: BlockBed_OnBlockInteractStart_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        HarmonyInstance.Unpatch(original: ServerSystemBlockSimulation_HandleBlockPlaceOrBreak_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        HarmonyInstance.Unpatch(original: BlockEntityBed_DidUnmount_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        HarmonyInstance.Unpatch(original: GuiHandbookItemStackPage_PageCodeForStack_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        BlockEntityBed_DidUnmount_Patch.Applied = false;

        HarmonyInstance.Unpatch(original: MechNetworkRenderer_AddDevice_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        HarmonyInstance.Unpatch(original: BlockWindmillRotor_OnBlockInteractStart_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        if (api.Side.IsClient())
        {
            HarmonyInstance.Unpatch(original: BlockWindmillRotor_GetPlacedBlockInteractionHelp_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        }
    }
}