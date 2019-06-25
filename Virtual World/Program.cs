using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;

namespace Virtual_World
{
    class Program
    {
        private static SlimDX.Direct3D11.Device device;
        private static SwapChain swapChain;
        private static FSQ finalRender;
        private static RenderTargetView renderView;
        private static GlobeShader globeShader;

        static void Main(string[] args)
        {
            // Setup
            var form = SlimDXHelper.InitD3D(out device, out swapChain, out renderView);

            finalRender = new FSQ(device, renderView, "SimpleFSQ.fx");
            globeShader = new GlobeShader(device, finalRender.UAV);

            // Main loop
            MessagePump.Run(form, SimMain);

            // Tear down
            finalRender.Dispose();
            globeShader.Dispose();

            swapChain.Dispose();
            renderView.Dispose();
            device.Dispose();
        }

        private static void SimMain()
        {
            // Draws the globe into the render target
            globeShader.Dispatch();

            // Draws the render target to the screen
            finalRender.Draw();

            swapChain.Present(0, PresentFlags.None);
        }
    }
}
