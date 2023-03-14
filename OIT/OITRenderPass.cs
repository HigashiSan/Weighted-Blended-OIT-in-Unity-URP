using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OITRenderPass : ScriptableRenderPass
{
    string m_profilerTag;
    LayerMask m_layerMask;
    FilteringSettings m_filteringSettings;
    RenderStateBlock m_renderStateBlock;
    Material m_blendMat = null;

    /// <summary>
    /// RenderTarget Handle
    /// </summary>
    RenderTargetIdentifier m_cameraColor;
    RenderTargetIdentifier m_cameraDepth;
    RenderTargetIdentifier m_accumulate;
    RenderTargetIdentifier m_revealage;
    RenderTargetIdentifier m_destination;

    RenderTargetIdentifier[] m_oitBuffers = new RenderTargetIdentifier[2];

    int m_destinationID;
    static readonly int m_accumTexID = Shader.PropertyToID("_AccumTex");
    static readonly int m_revealageTexID = Shader.PropertyToID("_RevealageTex");

    public OITRenderPass(string profilerTag, RenderPassEvent renderPassEvent, RenderQueueType renderQueueType, LayerMask layerMask)
    {
        this.renderPassEvent = renderPassEvent;
        m_profilerTag = profilerTag;

        m_layerMask = layerMask;
        RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
            ? RenderQueueRange.transparent
            : RenderQueueRange.opaque;
        m_filteringSettings = new FilteringSettings(renderQueueRange, m_layerMask);
        m_renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

        m_blendMat = new Material(CoreUtils.CreateEngineMaterial("cdc/FinalBlend"));
        if (m_blendMat == null)
        {
            Debug.Log("No shader");
        }
        m_blendMat.hideFlags = HideFlags.DontSave;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cmd.GetTemporaryRT(m_accumTexID, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        m_accumulate = new RenderTargetIdentifier(m_accumTexID);
        cmd.SetRenderTarget(m_accumulate);
        cmd.ClearRenderTarget(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));

        cmd.GetTemporaryRT(m_revealageTexID, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        m_revealage = new RenderTargetIdentifier(m_revealageTexID);
        cmd.SetRenderTarget(m_revealage);
        cmd.ClearRenderTarget(false, true, new Color(1.0f, 1.0f, 1.0f, 1.0f));

        cmd.GetTemporaryRT(m_destinationID, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        m_destination = new RenderTargetIdentifier(m_destinationID);

        m_oitBuffers[0] = m_accumulate;
        m_oitBuffers[1] = m_revealage;
    }

    public void Setup(RenderTargetIdentifier color, RenderTargetIdentifier depth)
    {
        m_cameraColor = color;
        m_cameraDepth = depth;
    }

    void DoAccumulate(CommandBuffer cmd, ScriptableRenderContext context, ref RenderingData renderingData)
    {
        SortingCriteria sortingCriteria = (m_filteringSettings.renderQueueRange == RenderQueueRange.transparent)
            ? SortingCriteria.CommonTransparent
            : renderingData.cameraData.defaultOpaqueSortFlags;
        DrawingSettings drawSettings = CreateDrawingSettings(new ShaderTagId("DoOIT"), ref renderingData, sortingCriteria);

        cmd.Clear();
        cmd.SetRenderTarget(m_oitBuffers, m_cameraDepth);
        context.ExecuteCommandBuffer(cmd);
        context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_filteringSettings, ref m_renderStateBlock);
    }

    void DoBlend(CommandBuffer cmd, ScriptableRenderContext context)
    {
        cmd.Clear();
        cmd.SetGlobalTexture("_AccumTex", m_accumulate);
        cmd.SetGlobalTexture("_RevealageTex", m_revealage);
        Blit(cmd, m_cameraColor, m_destination, m_blendMat);
        Blit(cmd, m_destination, m_cameraColor);
        context.ExecuteCommandBuffer(cmd);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(m_profilerTag);
        DoAccumulate(cmd, context, ref renderingData);
        DoBlend(cmd, context);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(m_accumTexID);
        cmd.ReleaseTemporaryRT(m_revealageTexID);
        cmd.ReleaseTemporaryRT(m_destinationID);
    }


}
