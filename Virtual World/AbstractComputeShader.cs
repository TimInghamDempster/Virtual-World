using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;

namespace Virtual_World
{
    /// <summary>
    /// Inheriting from this will take care of the boilerplate
    /// for running a compute shader, along with some code gen
    /// </summary>
    public abstract class AbstractComputeShader
    {
        // Simple text replacement markup which wil
        // be applied to compute shaders before compilation
        protected struct MarkupTag
        {
            public string Name { get; }
            public string Value { get; }

            public MarkupTag(string name, string value)
            {
                Name = name ??
                    throw new ArgumentNullException(nameof(name));

                Value = value ??
                    throw new ArgumentNullException(nameof(value));
            }
        }

        protected int _threadsPerGroupX;
        protected int _threadsPerGroupY;
        protected int _threadsPerGroupZ;

        protected int _threadGroupsX;
        protected int _threadGroupsY;
        protected int _threadGroupsZ;

        private ComputeShader _computeShader;
        private readonly string _filename;
        private readonly string _shaderName;
        private bool _initialised = false;

        protected List<MarkupTag> _markupTags = new List<MarkupTag>();

        private readonly Device _device;

        public AbstractComputeShader(string filename, string shaderName, Device device)
        {
            _device = device ??
                throw new ArgumentNullException(nameof(device));

            _filename = filename ??
                throw new ArgumentNullException(nameof(filename));

            _shaderName = shaderName ??
                throw new ArgumentNullException(nameof(shaderName));
        }

        /// <summary>
        /// We need to give derived classes a chance to
        /// set things up before doing the code gen, so
        /// instead of doing this at construction we do
        /// it lazily before first use of the shader
        /// </summary>
        private void InitialiseShader()
        {
            _markupTags.Add(
                new MarkupTag(
                    "threadCountX",
                    _threadsPerGroupX.ToString()));

            _markupTags.Add(
                new MarkupTag(
                    "threadCountY",
                    _threadsPerGroupY.ToString()));

            _markupTags.Add(
                new MarkupTag(
                    "threadCountZ",
                    _threadsPerGroupZ.ToString()));

            var tempFilename = GenerateTempFile(_filename);
            var csBytecode = ShaderBytecode.CompileFromFile(tempFilename, _shaderName, "cs_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization, EffectFlags.None);
            _computeShader = new ComputeShader(_device, csBytecode);

            csBytecode.Dispose();

            _initialised = true;
        }

        /// <summary>
        /// Override this to set up anything specific your compute
        /// shader needs, such as binding resource views
        /// </summary>
        protected virtual void PreviewDispatch(Device device) { }

        /// <summary>
        /// Dispatches the shader
        /// </summary>
        public void Dispatch()
        {
            if(!_initialised)
            {
                InitialiseShader();
            }

            _device.ImmediateContext.ComputeShader.Set(_computeShader);

            PreviewDispatch(_device);

            _device.ImmediateContext.Dispatch(_threadGroupsX, _threadGroupsY, _threadGroupsZ);

            PostDispatch(_device);
        }

        // <summary>
        /// Override this to clean up anything specific your compute
        /// shader needs, such as nulling resource views
        /// </summary>
        protected virtual void PostDispatch(Device device) { }

        /// <summary>
        /// Copy the shader into a new file and replace all
        /// of the tags with the specified values.  Means we
        /// can programatically set things like threads per
        /// group.  Returns the filename of the generated file
        /// </summary>
        private string GenerateTempFile(string filename)
        {
            var outputFilename = Path.GetFileNameWithoutExtension(filename);
            outputFilename += "_Generated";
            outputFilename += Path.GetExtension(filename);

            using(StreamReader reader = new StreamReader(filename))
            {
                using(StreamWriter writer = new StreamWriter(outputFilename))
                {
                    while(!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        foreach(var tag in _markupTags)
                        {
                            var fixedUpTag = "#" + tag.Name + "#";
                            line = line.Replace(fixedUpTag, tag.Value);
                        }

                        writer.WriteLine(line);
                    }
                }
            }

            return outputFilename;
        }

        public void Dispose()
        {
            _computeShader.Dispose();
        }
    }
}
