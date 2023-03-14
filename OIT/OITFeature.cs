using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;

public class OITFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OITSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public FilterSettings filterSettings = new FilterSettings();
    }

    [System.Serializable]
    public class FilterSettings
    {
        public RenderQueueType renderQueueType;
        public LayerMask layerMask;

        public FilterSettings()
        {
            renderQueueType = RenderQueueType.Transparent;
            layerMask = 0;
        }
    }

    public OITSettings settings = new OITSettings();
    public OITRenderPass m_OITRenderPass;


    public override void Create()
    {
        FilterSettings filter = settings.filterSettings;
        m_OITRenderPass = new OITRenderPass("OIT Pass", settings.renderPassEvent, filter.renderQueueType, filter.layerMask);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_OITRenderPass.Setup(renderer.cameraColorTarget, renderer.cameraDepthTarget);
        renderer.EnqueuePass(m_OITRenderPass);
    }
}
