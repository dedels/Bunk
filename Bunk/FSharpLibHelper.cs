using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk
{
    public static class FSharpLibHelper
    {
        public static bool IsSome<T>(this FSharpOption<T> option)
        {
            return FSharpOption<T>.get_IsSome(option);
        }

        public static bool IsNone<T>(this FSharpOption<T> option)
        {
            return FSharpOption<T>.get_IsNone(option);
        }
    }
}
