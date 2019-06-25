using SlimDX.Direct3D11;
using System;

namespace Virtual_World
{
    /// <summary>
    /// A compute shader for drawing the gobe into a UAV
    /// </summary>
    public class GlobeShader : AbstractComputeShader
    {
        UnorderedAccessView _uav;
        public GlobeShader(Device device, UnorderedAccessView uav)
            : base ("Globe.hlsl", "DrawGlobe", device)
        {
            _uav = uav ??
                throw new ArgumentNullException(nameof(uav));

            _threadsPerGroupX = 8;
            _threadGroupsX = (1080 / _threadsPerGroupX);

            _threadsPerGroupY = 8;
            _threadGroupsY = (1080 / _threadsPerGroupY);

            _threadsPerGroupZ = 1;
            _threadGroupsZ = 1;

        }

        protected override void PreviewDispatch(Device device)
        {
            device.ImmediateContext.ComputeShader.SetUnorderedAccessView(_uav, 0);
            device.ImmediateContext.ClearUnorderedAccessView(_uav, new float[] { 1.0f, 1.0f, 0.0f, 1.0f });
        }

        protected override void PostDispatch(Device device)
        {
            device.ImmediateContext.ComputeShader.SetUnorderedAccessView(null, 0);
        }
    }
}
