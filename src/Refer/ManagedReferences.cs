﻿using PipServices3.Commons.Refer;
using PipServices3.Commons.Run;
using System.Threading.Tasks;

namespace PipServices3.Container.Refer
{
    /// <summary>
    /// Managed references that in addition to keeping and locating references can also
    /// manage their lifecycle:
    /// - Auto-creation of missing component using available factories
    /// - Auto-linking newly added components
    /// - Auto-opening newly added components
    /// - Auto-closing removed components
    /// </summary>
    /// See <see cref="RunReferencesDecorator"/>, <see cref="LinkReferencesDecorator"/>, 
    /// <see cref="BuildReferencesDecorator"/>, 
    /// <a href="https://pip-services3-dotnet.github.io/pip-services3-commons-dotnet/class_pip_services3_1_1_commons_1_1_refer_1_1_references.html">References</a>
    public class ManagedReferences: ReferencesDecorator, IOpenable
    {
        protected References _references;
        protected BuildReferencesDecorator _builder;
        protected LinkReferencesDecorator _linker;
        protected RunReferencesDecorator _runner;

        /// <summary>
        /// Creates a new instance of the references
        /// </summary>
        /// <param name="tuples">tuples where odd values are component locators (descriptors)
        /// and even values are component references</param>
        public ManagedReferences(object[] tuples = null)
            : base(null, null)
        {
            _references = new References(tuples);
            _builder = new BuildReferencesDecorator(_references, this);
            _linker = new LinkReferencesDecorator(_builder, this);
            _runner = new RunReferencesDecorator(_linker, this);

            BaseReferences = _runner;
        }

        /// <summary>
        /// Checks if the component is opened.
        /// </summary>
        /// <returns>true if the component has been opened and false otherwise.</returns>
        public bool IsOpen()
        {
            return _linker.IsOpen() && _runner.IsOpen();
        }

        /// <summary>
        /// Opens the component.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public async Task OpenAsync(string correlationId)
        {
            await _linker.OpenAsync(correlationId);
            await _runner.OpenAsync(correlationId);
        }

        /// <summary>
        /// Closes component and frees used resources.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public async Task CloseAsync(string correlationId)
        {
            await _runner.CloseAsync(correlationId);
            await _linker.CloseAsync(correlationId);
        }

        /// <summary>
        /// Creates a new ManagedReferences object filled with provided key-value pairs
        /// called tuples.Tuples parameters contain a sequence of locator1, component1,
        /// locator2, component2, ... pairs.
        /// </summary>
        /// <param name="tuples">the tuples to fill a new ManagedReferences object.</param>
        /// <returns>a new ManagedReferences object.</returns>
        public static ManagedReferences FromTyples(params object[] tuples)
        {
            return new ManagedReferences(tuples);
        }
    }
}
