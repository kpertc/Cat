using Adobe.Substance.Input;
using Adobe.Substance.Input.Description;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Adobe.Substance.Runtime
{
    /// <summary>
    /// Class that provides runtime functionality to modify inputs at and render substance graphs, allowing SubstanceGraphSO to generate its assets at runtime.
    /// </summary>
    public class SubstanceRuntimeGraph : MonoBehaviour
    {
        /// <summary>
        /// Target substance instance.
        /// </summary>
        [SerializeField]
        public SubstanceGraphSO GraphSO;

        /// <summary>
        /// Native handler to the substance SDK object.
        /// </summary>
        private SubstanceNativeHandler _runtimeHandler;

        /// <summary>
        /// Main material generated by the substance instance.
        /// </summary>
        public Material DefaulMaterial => GraphSO.OutputMaterial;

        /// <summary>
        /// Concurrent queue that handles results of substance render operations.
        /// </summary>
        /// <typeparam name="AsyncRenderResult"></typeparam>
        private readonly ConcurrentQueue<AsyncRenderResult> _asyncRenderQueue = new ConcurrentQueue<AsyncRenderResult>();

        /// <summary>
        /// Dictionary to cache substance inputs by name.
        /// </summary>
        /// <returns></returns>
        private readonly Dictionary<string, SubstanceInputBase> _inputsTable = new Dictionary<string, SubstanceInputBase>();

        /// <summary>
        /// Dictionary that caches substance outputs by name.
        /// </summary>
        /// <returns></returns>
        private readonly Dictionary<string, SubstanceOutputTexture> _outputTable = new Dictionary<string, SubstanceOutputTexture>();

        /// <summary>
        /// Resoltion input.
        /// </summary>
        private SubstanceInputInt2 _resolutionInput;

        /// <summary>
        /// Cached graph index.
        /// </summary>
        private int graphID = 0;

        /// <summary>
        /// On awake SubstanceRuntime will be used to create a instance for the attached SubstanceGraphSO in the substance SDK.
        /// </summary>
        protected void Awake()
        {
            if (GraphSO == null)
                return;

            if (_runtimeHandler != null)
                return;

            graphID = GraphSO.Index;

            _runtimeHandler = SubstanceRuntime.Instance.InitializeInstance(GraphSO);
            InitializeGraph(GraphSO);
        }

        /// <summary>
        /// Generate assets for the target graph and populates the inputs and outputs dictionaries.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        private void InitializeGraph(SubstanceGraphSO graph)
        {
            graph.RuntimeInitialize(_runtimeHandler, true);

            foreach (var input in graph.Input)
            {
                _inputsTable.Add(input.Description.Identifier, input as SubstanceInputBase);

                if (input.ValueType == SubstanceValueType.Int2 && input.Description.Identifier == "$outputsize")
                    _resolutionInput = input as SubstanceInputInt2;
            }

            foreach (var output in graph.Output)
            {
                if (!_outputTable.ContainsKey(output.Description.Identifier))
                {
                    _outputTable.Add(output.Description.Identifier, output);
                }
            }
        }

        /// <summary>
        /// Check the render ConcurrentQueue for render results.
        /// </summary>
        protected void Update()
        {
            while (_asyncRenderQueue.TryDequeue(out AsyncRenderResult result))
            {
                if (result.Exception != null)
                {
                    result.Tcs.SetException(result.Exception);
                    continue;
                }

                GraphSO.UpdateOutputTextures(result.RenderResult);
                result.Tcs.SetResult(null);
            }
        }

        /// <summary>
        /// Replaces the render target material.
        /// </summary>
        /// <param name="material">Attaches a new Substance Material to be managed by this object.</param>
        public void AttachGraph(SubstanceGraphSO graph)
        {
            GraphSO = graph;
        }

        /// <summary>
        /// Disposes the substance SDK handler.
        /// </summary>
        protected void OnDestroy()
        {
            if (_runtimeHandler != null)
                _runtimeHandler.Dispose();
        }

        #region Input Handle

        /// <summary>
        /// Update Substance Float Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value">Value used to update parameter</param>
        public void SetInputFloat(string inputName, float value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float}.");

            _runtimeHandler.SetInputFloat(value, input.Index, graphID);
        }

        /// <summary>
        /// Get Substance Float Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <returns>Current input value.</returns>
        public float GetInputFloat(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float}.");

            return _runtimeHandler.GetInputFloat(input.Index, graphID);
        }

        /// <summary>
        /// Update Substance Vector2 Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value">Value used to update parameter</param>
        public void SetInputVector2(string inputName, Vector2 value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float2)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float2}.");

            _runtimeHandler.SetInputFloat2(value, input.Index, graphID);
        }

        /// <summary>
        /// Get Substance Vector2 Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <returns>Current input value.</returns>
        public Vector2 GetInputVector2(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float2)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float2}.");

            return _runtimeHandler.GetInputFloat2(input.Index, graphID);
        }

        /// <summary>
        /// Update Substance Vector3 Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value">Value used to update parameter</param>
        public void SetInputVector3(string inputName, Vector3 value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float3)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float3}.");

            _runtimeHandler.SetInputFloat3(value, input.Index, graphID);
        }

        /// <summary>
        /// Get Substance Vector3 Input.
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <returns>Current input value.</returns>
        public Vector3 GetInputVector3(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float3)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float3}.");

            return _runtimeHandler.GetInputFloat3(input.Index, graphID);
        }

        /// <summary>
        /// Update Substance Vector4 Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value">Value used to update parameter</param>
        public void SetInputVector4(string inputName, Vector4 value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float4)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float4}.");

            _runtimeHandler.SetInputFloat4(value, input.Index, graphID);
        }

        /// <summary>
        /// Get Substance Vector4 Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <returns>Current input value.</returns>
        public Vector4 GetInputVector4(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float4)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float4}.");

            return _runtimeHandler.GetInputFloat4(input.Index, graphID);
        }

        /// <summary>
        /// Update Substance Color Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value">Value used to update parameter</param>
        public void SetInputColor(string inputName, Color value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float3 && input.ValueType != SubstanceValueType.Float4)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float4} or {SubstanceValueType.Float3}.");

            if (input.ValueType == SubstanceValueType.Float3)
            {
                var vector = new Vector3
                {
                    x = value.r,
                    y = value.g,
                    z = value.b
                };

                _runtimeHandler.SetInputFloat3(vector, input.Index, graphID);
            }
            else if (input.ValueType == SubstanceValueType.Float4)
            {
                _runtimeHandler.SetInputFloat4(value, input.Index, graphID);
            }
        }

        /// <summary>
        /// Get Substance Color
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <returns>Current input value.</returns>
        public Color GetInputColor(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Float3 && input.ValueType != SubstanceValueType.Float4)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Float4} or {SubstanceValueType.Float3}.");

            if (input.ValueType == SubstanceValueType.Float3)
            {
                Vector3 vector = _runtimeHandler.GetInputFloat3(input.Index, graphID);
                return new Color(vector.x, vector.y, vector.z);
            }
            else if (input.ValueType == SubstanceValueType.Float4)
            {
                Vector4 vector = _runtimeHandler.GetInputFloat4(input.Index, graphID);
                return new Color(vector.x, vector.y, vector.z, vector.w);
            }
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// Update Substance Boolean Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value">Value used to update parameter</param>
        public void SetInputBool(string inputName, bool value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int}.");

            _runtimeHandler.SetInputInt(value ? 1 : 0, input.Index, graphID);
        }

        /// <summary>
        /// Get Substance Boolean Input.
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR.</param>
        /// <returns>Current input value.</returns>
        public bool GetInputBool(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int}.");

            return _runtimeHandler.GetInputInt(input.Index, graphID) == 1;
        }

        /// <summary>
        /// Update Substance Int Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value">Value used to update the parameter</param>
        public void SetInputInt(string inputName, int value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int}.");

            _runtimeHandler.SetInputInt(value, input.Index, graphID);
        }

        /// <summary>
        /// Get Substance Int Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <returns>Current input value.</returns>
        public int GetInputInt(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int}.");

            return _runtimeHandler.GetInputInt(input.Index, graphID);
        }

        /// <summary>
        /// Update Substance Vector2Int Input.
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value">Value used to update the parameter.</param>
        public void SetInputVector2Int(string inputName, Vector2Int value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int2)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int2}.");

            _runtimeHandler.SetInputInt2(value, input.Index, graphID);
        }

        /// <summary>
        /// Get array of 2 int.
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <returns>Current input value.</returns>
        public Vector2Int GetInputVector2Int(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int2)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int2}.");

            return _runtimeHandler.GetInputInt2(input.Index, graphID);
        }

        /// <summary>
        /// Update Substance Vector3Int Input.
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value">Value used to update the parameter.</param>
        public void SetInputVector3Int(string inputName, Vector3Int value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int3)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int3}.");

            _runtimeHandler.SetInputInt3(value, input.Index, graphID);
        }

        /// <summary>
        /// Get array of 3 int (Vector3Int’s x, y & z values)
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <returns>Current input value.</returns>
        public Vector3Int GetInputVector3Int(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int3)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int3}.");

            return _runtimeHandler.GetInputInt3(input.Index, graphID);
        }

        /// <summary>
        /// Update Substance Vector4Int Input
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="x">Value used to update the parameter</param>
        /// <param name="y">Value used to update the parameter</param>
        /// <param name="z">Value used to update the parameter</param>
        /// <param name="w">Value used to update the parameter</param>
        public void SetInputVector4Int(string inputName, int x, int y, int z, int w)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int4)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int4}.");

            _runtimeHandler.SetInputInt4(x, y, z, w, input.Index, graphID);
        }

        /// <summary>
        /// Get array of 4 int (Vector4Int’s x, y, z & w values)
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <returns>Current input value.</returns>
        public int[] GetInputVector4Int(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Int4)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Int4}.");

            return _runtimeHandler.GetInputInt4(input.Index, graphID);
        }

        /// <summary>
        /// Update Substance string Input.
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR.</param>
        /// <param name="value">Value used to update the parameter.</param>
        public void SetInputString(string inputName, string value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.String)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.String}.");

            _runtimeHandler.SetInputString(value, input.Index, graphID);
        }

        /// <summary>
        /// Get Substance string input.
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR.</param>
        /// <returns>Input current value.</returns>
        public string GetInputString(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.String)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.String}.");

            return _runtimeHandler.GetInputString(input.Index, graphID);
        }

        /// <summary>
        /// Returns the complete input description for the target input name.
        /// </summary>
        /// <param name="inputName">Target input name.</param>
        /// <returns>Complete input description for the target input.</returns>
        public SubstanceInputDescription GetInputDescription(string inputName)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            return input.Description;
        }

        /// <summary>
        /// Update Substance Texture2D Input.
        /// </summary>
        /// <param name="inputName">Name of the input in the SBSAR</param>
        /// <param name="value"> value used to update the parameter</param>
        public void SetInputTexture(string inputName, Texture2D value)
        {
            if (!TryGetInput(inputName, out SubstanceInputBase input))
                throw new ArgumentException($"No Substance Input found with name {inputName}");

            if (input.ValueType != SubstanceValueType.Image)
                throw new ArgumentException($"Substance Input {inputName} is of type {input.ValueType} not {SubstanceValueType.Image}.");

            if (value == null)
                throw new ArgumentException("Texture can not be null.");

            if (!value.isReadable)
                throw new ArgumentException("Texture must be readable");

            var pixels = value.GetPixels32();
            _runtimeHandler.SetInputTexture2D(pixels, value.width, value.height, input.Index, graphID);
        }

        /// <summary>
        /// Returns instance texture output resoltion.
        /// </summary>
        /// <returns>Current output resolution</returns>
        public Vector2Int GetTexturesResolution()
        {
            return _runtimeHandler.GetInputInt2(_resolutionInput.Index, _resolutionInput.GraphID);
        }

        /// <summary>
        /// Sets instance texture output resolution.
        /// </summary>
        /// <param name="size"></param>
        public void SetTexturesResolution(Vector2Int size)
        {
            _runtimeHandler.SetInputInt2(size, _resolutionInput.Index, _resolutionInput.GraphID);
        }

        /// <summary>
        /// Returns true if this substance instance has an input with a given name.
        /// </summary>
        /// <param name="inputName">Input name.</param>
        /// <returns>TRUE if substance instance has input with the given name.</returns>
        public bool HasInput(string inputName)
        {
            return _inputsTable.ContainsKey(inputName);
        }

        private bool TryGetInput(string name, out Input.SubstanceInputBase input)
        {
            return _inputsTable.TryGetValue(name, out input);
        }

        #endregion Input Handle

        #region Output Handle

        /// <summary>
        /// Returns a list with all output textures for the substance instance.
        /// </summary>
        /// <returns>Output texture.</returns>
        public List<Texture2D> GetGeneratedTextures()
        {
            return _outputTable.Values.Select(a => a.OutputTexture).ToList();
        }

        /// <summary>
        /// Returns the output texture for a given output name.
        /// </summary>
        /// <param name="outputName">Output name.</param>
        /// <returns>Output texture.</returns>
        public Texture2D GetOutputTexture(string outputName)
        {
            return _outputTable[outputName].OutputTexture;
        }

        #endregion Output Handle

        #region Render

        /// <summary>
        /// Renders the substance instance synchronously.
        /// </summary>
        public void Render()
        {
            var result = _runtimeHandler.Render(GraphSO.Index);
            GraphSO.UpdateOutputTextures(result);
        }

        /// <summary>
        /// Renders the substance instance asynchronously.
        /// </summary>
        /// <returns>Task that will finish once render is done.</returns>
        public Task RenderAsync()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Task.Run(() =>
            {
                try
                {
                    var result = _runtimeHandler.Render(graphID);
                    _asyncRenderQueue.Enqueue(new AsyncRenderResult(result, tcs));
                }
                catch (Exception e)
                {
                    _asyncRenderQueue.Enqueue(new AsyncRenderResult(e));
                }
            });

            return tcs.Task;
        }

        #endregion Render

        #region Preset

        /// <summary>
        /// Uses a preset XML to set graph input parameters.
        /// </summary>
        /// <param name="presetXML">Preset XML data.</param>
        public void LoadPreset(string presetXML)
        {
            _runtimeHandler.ApplyPreset(GraphSO.Index, presetXML);
        }

        /// <summary>
        /// Saves the current graph state into a preset XML.
        /// </summary>
        /// <returns>Preset created using the current state of the graph inputs.</returns>
        public string CreatePresetFromCurrentState()
        {
            return _runtimeHandler.CreatePresetFromCurrentState(GraphSO.Index);
        }

        #endregion Preset

        /// <summary>
        /// Internal class to store substance instance render result.
        /// </summary>
        private class AsyncRenderResult
        {
            public IntPtr RenderResult { get; }
            public TaskCompletionSource<object> Tcs { get; }
            public Exception Exception { get; }

            public AsyncRenderResult(IntPtr renderResult, TaskCompletionSource<object> tcs)
            {
                RenderResult = renderResult;
                Tcs = tcs;
                Exception = null;
            }

            public AsyncRenderResult(Exception exception)
            {
                Exception = exception;
            }
        }
    }
}