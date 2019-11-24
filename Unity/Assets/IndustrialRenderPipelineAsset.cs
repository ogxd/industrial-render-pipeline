using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "Rendering/Industrial Render Pipeline Asset")]
public class IndustrialRenderPipelineAsset : RenderPipelineAsset {

    protected override IRenderPipeline InternalCreatePipeline() {
        return new IndustrialRenderPipeline();
    }
}