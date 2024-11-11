﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flags;

/// <summary>
/// Patch because bed drops banner upon waking up
/// </summary>
public static class BlockEntityBed_DidUnmount_Patch
{
    [ThreadStatic]
    public static bool Applied;

    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(BlockEntityBed), nameof(BlockEntityBed.DidUnmount), new[] { typeof(EntityAgent) });
    }

    public static MethodInfo GetTranspiler() => typeof(BlockEntityBed_DidUnmount_Patch).GetMethod(nameof(Transpiler));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        if (Applied)
        {
            return instructions;
        }
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        int baseCallIndex = codes.FindIndex(c => c.opcode == OpCodes.Call && (c.operand?.ToString()?.Contains("OnBlockRemoved") ?? false));

        baseCallIndex--;

        codes.RemoveRange(baseCallIndex, 2);
        Label endLabel = il.DefineLabel();
        codes[^1].labels.Add(endLabel);

        LocalBuilder currentItemVar = il.DeclareLocal(typeof(long));
        LocalBuilder countVar = il.DeclareLocal(typeof(int));
        LocalBuilder indexVar = il.DeclareLocal(typeof(int));
        Label loopStartLabel = il.DefineLabel();
        codes.InsertRange(
            baseCallIndex, new CodeInstruction[] {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, typeof(BlockEntity).GetField("TickHandlers", BindingFlags.Instance | BindingFlags.NonPublic)),
                new(OpCodes.Brfalse_S, endLabel),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, typeof(BlockEntity).GetField("TickHandlers", BindingFlags.Instance | BindingFlags.NonPublic)),
                new(OpCodes.Callvirt, typeof(List<long>).GetProperty("Count")!.GetMethod),
                new(OpCodes.Stloc_S, countVar),

                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Stloc_S, indexVar),

                new(OpCodes.Ldloc_S, indexVar) { labels = new List<Label>() { loopStartLabel } },
                new(OpCodes.Ldloc_S, countVar),
                new(OpCodes.Bge, endLabel),

                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, typeof(BlockEntity).GetField("TickHandlers", BindingFlags.Instance | BindingFlags.NonPublic)),
                new(OpCodes.Ldloc_S, indexVar),
                new(OpCodes.Callvirt, typeof(List<long>).GetMethod("get_Item")),
                new(OpCodes.Stloc_S, currentItemVar),

                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, typeof(BlockEntity).GetField("Api", BindingFlags.Instance | BindingFlags.Public)),
                new(OpCodes.Callvirt, typeof(ICoreAPI).GetProperty("Event")!.GetMethod),
                new(OpCodes.Ldloc_S, currentItemVar),
                new(OpCodes.Callvirt, typeof(IEventAPI).GetMethod("UnregisterGameTickListener")),

                new(OpCodes.Ldloc_S, indexVar),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Add),
                new(OpCodes.Stloc_S, indexVar),

                new(OpCodes.Br, loopStartLabel)

            });

        Applied = true;
        return codes.AsEnumerable();
    }
}