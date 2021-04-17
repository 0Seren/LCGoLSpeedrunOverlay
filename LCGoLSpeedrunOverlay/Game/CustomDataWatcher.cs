using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using LiveSplit.ComponentUtil;

namespace LCGoLOverlayProcess.Game
{
    public class CustomDataWatcher<TR> : MemoryWatcher<TR> where TR : struct
    {
        private readonly MemoryWatcherList _memoryWatcherList = new MemoryWatcherList();
        private readonly Func<TR> _customExpression;

        public CustomDataWatcher(Func<TR> customExpression, params MemoryWatcher[] dependencies)
            : this(customExpression, dependencies.ToList()){}

        private CustomDataWatcher(Func<TR> customExpression, IEnumerable<MemoryWatcher> dependencies) : base(IntPtr.Zero)
        {
            _customExpression = customExpression;

            var pointerDependencies = new List<DeepPointer>();
            foreach (var memoryWatcher in dependencies)
            {
                _memoryWatcherList.Add(CopyMemoryWatcher(memoryWatcher));
            }
        }

        private static MemoryWatcher CopyMemoryWatcher(MemoryWatcher memoryWatcher)
        {
            MemoryWatcher mw = ShallowCopyObject(memoryWatcher);
            


        }

        private static T ShallowCopyObject<T>(T obj)
        {
            var cloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
            
            return (T) cloneMethod?.Invoke(obj, new object[0]);
        }
    }
}
