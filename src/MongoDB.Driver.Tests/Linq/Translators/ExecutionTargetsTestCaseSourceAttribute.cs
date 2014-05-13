using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver.Linq;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Linq.Translators
{
    public class ExecutionTargetsTestCaseSourceAttribute : TestCaseSourceAttribute
    {
        private readonly IEnumerable<ExecutionTarget> _targets;

        public ExecutionTargetsTestCaseSourceAttribute()
            : base(typeof(ExecutionTargetsTestCaseSourceAttribute), "Targets")
        {
            _targets = new List<ExecutionTarget>
            {
                ExecutionTarget.Query,
                ExecutionTarget.Pipeline
            };
        }

        public IEnumerable<ExecutionTarget> Targets
        {
            get { return _targets; }
        }
    }
}
