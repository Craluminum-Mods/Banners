using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.Server;

namespace Flags;

public class HarmonyPatches : ModSystem
{
    public static ICoreAPI api;

    private Harmony HarmonyInstance => new Harmony(Mod.Info.ModID);

    public override void Start(ICoreAPI _api)
    {
        api = _api;
        HarmonyInstance.Patch(original: typeof(BlockBed).GetMethod(nameof(BlockBed.OnBlockInteractStart)), prefix: typeof(BlockBed_OnBlockInteractStart_Patch).GetMethod(nameof(BlockBed_OnBlockInteractStart_Patch.Prefix)));
        HarmonyInstance.Patch(original: AccessTools.Method(typeof(ServerSystemBlockSimulation), "HandleBlockPlaceOrBreak"), prefix: typeof(ServerSystemBlockSimulation_HandleBlockPlaceOrBreak_Patch).GetMethod(nameof(ServerSystemBlockSimulation_HandleBlockPlaceOrBreak_Patch.Prefix)));
        HarmonyInstance.Patch(original: BlockEntityBed_DidUnmount_Patch.TargetMethod(), transpiler: typeof(BlockEntityBed_DidUnmount_Patch).GetMethod(nameof(BlockEntityBed_DidUnmount_Patch.Transpiler)));
    }

    public override void Dispose()
    {
        HarmonyInstance.Unpatch(original: typeof(BlockBed).GetMethod(nameof(BlockBed.OnBlockInteractStart)), HarmonyPatchType.All, HarmonyInstance.Id);
        HarmonyInstance.Unpatch(original: AccessTools.Method(typeof(ServerSystemBlockSimulation), "HandleBlockPlaceOrBreak"), HarmonyPatchType.All, HarmonyInstance.Id);
        HarmonyInstance.Unpatch(original: BlockEntityBed_DidUnmount_Patch.TargetMethod(), HarmonyPatchType.All, HarmonyInstance.Id);
        BlockEntityBed_DidUnmount_Patch.Applied = false;
    }
}