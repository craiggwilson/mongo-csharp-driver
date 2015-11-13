using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet;

namespace Benchmarking
{
    class Program
    {
        static void Main(string[] args)
        {
            var benchmarks = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                             .Any(m => m.GetCustomAttributes(typeof(BenchmarkAttribute), false).Any()))
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name)
                .ToArray();
            var switcher = new BenchmarkCompetitionSwitch(benchmarks);
            switcher.Run(args);
        }
    }
}
