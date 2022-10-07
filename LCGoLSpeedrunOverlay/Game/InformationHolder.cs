using System;
using LiveSplit.ComponentUtil;

namespace LCGoLOverlayProcess.Game
{
    public class InformationHolder<T> : IInformationHolder
    {
        private readonly Func<T> _getNewValue;

        public T Current { get; private set; }

        public T Old { get; private set; }

        public bool Changed => !Equals(Current, Old);

        public bool IsUpdateable => !(_getNewValue is null);

        public InformationHolder(Func<T> getNewValue, T currentValue = default, T oldValue = default)
        {
            _getNewValue = getNewValue;
            Current = currentValue;
            Old = oldValue;
        }

        public void Update()
        {
            if (_getNewValue is null)
                return;

            Old = Current;
            Current = _getNewValue.Invoke();
        }

        public InformationHolder<T> Clone()
        {
            return (InformationHolder<T>)MemberwiseClone();
        }
    }

    public static class InformationHolder
    {
        public static InformationHolder<string> FromMemoryWatcher(StringWatcher memoryWatcher)
        {
            return new InformationHolder<string>(null, memoryWatcher.Current, memoryWatcher.Old);
        }

        public static InformationHolder<TMemoryWatcher> FromMemoryWatcher<TMemoryWatcher>(MemoryWatcher<TMemoryWatcher> memoryWatcher) where TMemoryWatcher : struct
        {
            return new InformationHolder<TMemoryWatcher>(null, memoryWatcher.Current, memoryWatcher.Old);
        }
    }
}
