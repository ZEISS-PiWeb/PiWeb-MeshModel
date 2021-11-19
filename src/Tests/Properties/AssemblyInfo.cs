#region usings

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;

#endregion

[assembly: AssemblyConfiguration( "" )]

[assembly: ComVisible( false )]
[assembly: CLSCompliant( false )]
[assembly: Parallelizable( ParallelScope.Self | ParallelScope.Children | ParallelScope.Fixtures )]