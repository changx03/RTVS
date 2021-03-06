﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Services;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Protocol;
using static System.FormattableString;

namespace Microsoft.R.Host.Broker.About {
    [Authorize(Policy = Policies.RUser)]
    [Route("/info/about")]
    public class AboutController : Controller {
        private readonly InterpreterManager _interpManager;
        private readonly SessionManager _sessionManager;
        private readonly ISystemInfoService _systemInfo;

        public AboutController(InterpreterManager interpManager, SessionManager sessionManager, ISystemInfoService systemInfo) {
            _interpManager = interpManager;
            _sessionManager = sessionManager;
            _systemInfo = systemInfo;
        }

        [AllowAnonymous]
        [HttpGet]
        public AboutHost Get() {
            var a = new AboutHost {
                Version = typeof(AboutHost).GetTypeInfo().Assembly.GetName().Version,
                OSDescription = RuntimeInformation.OSDescription,
                Is64BitOperatingSystem = RuntimeInformation.OSArchitecture == Architecture.X64,
                Is64BitProcess = RuntimeInformation.ProcessArchitecture == Architecture.X64,
                ProcessorCount = Environment.ProcessorCount,
#if !NETSTANDARD1_6
                WorkingSet = Environment.WorkingSet,
#endif
                ConnectedUserCount = _sessionManager.GetUsers().Count()
            };

            var memoryInfo = _systemInfo.GetMemoryInformation();
            a.TotalVirtualMemory = memoryInfo.TotalVirtualMemory;
            a.FreeVirtualMemory = memoryInfo.FreeVirtualMemory;
            a.TotalPhysicalMemory = memoryInfo.TotalPhysicalMemory;
            a.FreePhysicalMemory = memoryInfo.FreePhysicalMemory;

            a.VideoCards = _systemInfo.GetVideoControllerInformation().Select(ci => 
                new VideoCardInfo() {
                    VideoCardName = ci.VideoCardName,
                    VideoRAM = ci.VideoRAM,
                    VideoProcessor = ci.VideoProcessor
                }).ToArray();

            a.Interpreters = _interpManager.Interpreters.Select(x => Invariant($"[{x.Id}] {x.Name}")).ToArray();
            if(a.Interpreters.Length > 0) {
                a.Interpreters[0] = Invariant($"{a.Interpreters[0]} ({Resources.Default})");
            }
            return a;
        }
    }
}
