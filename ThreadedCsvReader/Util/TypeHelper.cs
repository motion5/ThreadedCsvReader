using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreadedCsvReader.Util
{
    public static class TypeHelper
    {
        public static IEnumerable<Type> GetTypes(Func<Type, bool> expression)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(expression).ToList();
        }
    }
}