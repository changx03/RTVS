﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.R.Debugger.Engine.PortSupplier {
    partial class RDebugPortSupplier {
        internal class DebugPort : IDebugPort2 {
            private readonly RDebugPortSupplier _supplier;
            private readonly IDebugPortRequest2 _request;
            private readonly Guid _guid = Guid.NewGuid();

            [Import]
            private IRSessionProvider RSessionProvider { get; set; }

            public DebugPort(RDebugPortSupplier supplier, IDebugPortRequest2 request) {
                _supplier = supplier;
                _request = request;

                var compModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                compModel.DefaultCompositionService.SatisfyImportsOnce(this);
            }

            public int EnumProcesses(out IEnumDebugProcesses2 ppEnum) {
                var processes = RSessionProvider.GetSessions().Select(kv => new DebugProcess(this, kv.Key, kv.Value));
                ppEnum = new AD7ProcessEnum(processes.Cast<IDebugProcess2>().ToArray());
                return VSConstants.S_OK;
            }

            public int GetPortId(out Guid pguidPort) {
                pguidPort = _guid;
                return VSConstants.S_OK;
            }

            public int GetPortName(out string pbstrName) {
                return _request.GetPortName(out pbstrName);
            }

            public int GetPortRequest(out IDebugPortRequest2 ppRequest) {
                ppRequest = _request;
                return VSConstants.S_OK;
            }

            public int GetPortSupplier(out IDebugPortSupplier2 ppSupplier) {
                ppSupplier = _supplier;
                return VSConstants.S_OK;
            }

            public int GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess) {
                ppProcess = null;
                return VSConstants.E_NOTIMPL;
            }
        }
    }
}