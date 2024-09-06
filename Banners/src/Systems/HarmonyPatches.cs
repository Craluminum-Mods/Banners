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
        HarmonyInstance.Patch(original: EntityShapeRenderer_onMeshReady_Patch.TargetMethod(), prefix: EntityShapeRenderer_onMeshReady_Patch.GetPrefix());
        HarmonyInstance.Patch(original: Entity_OnInteract_Patch.TargetMethod(), prefix: Entity_OnInteract_Patch.GetPrefix());
        HarmonyInstance.Patch(original: EntityBoat_OnInteract_Patch.TargetMethod(), prefix: EntityBoat_OnInteract_Patch.GetPrefix());
    }

    public override void Dispose()
    {
        HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
        BlockEntityBed_DidUnmount_Patch.Applied = false;
    }
}