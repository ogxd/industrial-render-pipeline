using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class IndustrialRenderPipeline : RenderPipeline {

    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras) {
        base.Render(renderContext, cameras);

        foreach (var camera in cameras) {
            Render(renderContext, camera);
        }
    }

    private void Render(ScriptableRenderContext context, Camera camera) {
        context.SetupCameraProperties(camera);

        var buffer = new CommandBuffer {
            name = camera.name
        };
        CameraClearFlags clearFlags = camera.clearFlags;
        buffer.ClearRenderTarget(
            (clearFlags & CameraClearFlags.Depth) != 0,
            (clearFlags & CameraClearFlags.Color) != 0,
            camera.backgroundColor
        );
        context.ExecuteCommandBuffer(buffer);
        buffer.Release();

        // ? 
        ScriptableCullingParameters cullingParameters;
        if (!CullResults.GetCullingParameters(camera, out cullingParameters)) {
            return;
        }
        CullResults cull = CullResults.Cull(ref cullingParameters, context);
        var drawSettings = new DrawRendererSettings(
            camera, new ShaderPassName("SRPDefaultUnlit")
        );
        var filterSettings = new FilterRenderersSettings(true);
        context.DrawRenderers(
            cull.visibleRenderers, ref drawSettings, filterSettings
        );

        context.DrawSkybox(camera);
        context.Submit();
    }
}