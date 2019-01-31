﻿using System;
using System.Collections.Generic;
using System.Linq;
using DR.Common.Monitoring.Contract;
using DR.Common.Monitoring.Models;

namespace DR.Common.Monitoring
{
    /// <summary>
    /// The global collection of every registred IHealthCheck implementations.
    /// Use structure map to bootstrap (already added in Global.asax.cs for the web-project):
    ///  a.For&lt;IEnumerable&lt;DR.Common.Monitoring.Contract.IHealthCheck&gt;&gt;().Use(x => x.GetAllInstances&lt;DR.Common.Monitoring.Contract.IHealthCheck&gt;());
    /// </summary>
    public class SystemStatus : ISystemStatus
    {
        private readonly bool _isPrivileged;
        private readonly IHealthCheck[] _checks;
        private readonly string[] _names;
        
        /// <summary>
        /// Construtor for System status
        /// </summary>
        /// <param name="checks">List of checks to register.</param>
        public SystemStatus(IEnumerable<IHealthCheck> checks, bool isPrivileged = true)
        {
            _isPrivileged = isPrivileged;
            _checks = checks.ToArray();
            _names = _checks.Select(c => c.Name).ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<Status> RunAllChecks()
        {
            return _checks.Select(c => c.GetStatus(_isPrivileged));
        }

        /// <inheritdoc />
        public Status RunCheck(string name)
        {
            var check = _checks.FirstOrDefault(c => c.Name == name) ?? 
                        throw new KeyNotFoundException($"No check named: {name}");
            return check.GetStatus(_isPrivileged);
        }

        /// <inheritdoc />
        public Status RunProbeCheck(string name, string node)
        {
            var check = _checks.FirstOrDefault(c => c.Name == name) ??
                        throw new KeyNotFoundException($"No check named: {name}");

            var probe = (check as IClusterProbe) ?? 
                        throw new InvalidCastException($"Check: {name} is not a cluster probe");

            return probe.GetStatus(node, _isPrivileged);
        }

        /// <inheritdoc />
        public IEnumerable<string> Names => _names;
    }
}
