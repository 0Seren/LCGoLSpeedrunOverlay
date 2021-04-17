using LiveSplit.ComponentUtil;

namespace LCGoLOverlayProcess.Game
{
    public class InformationHolder<T>
    {
        public T Current { get; private set; }

        public T Old { get; private set; }

        public InformationHolder(){}

        public InformationHolder(T currentValue, T oldValue)
        {
            Current = currentValue;
            Old = oldValue;
        }

        public void Update(T value)
        {
            Old = Current;
            Current = value;
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
            return new InformationHolder<string>(memoryWatcher.Current, memoryWatcher.Old);
        }

        public static InformationHolder<TMemoryWatcher> FromMemoryWatcher<TMemoryWatcher>(MemoryWatcher<TMemoryWatcher> memoryWatcher) where TMemoryWatcher : struct
        {
            return new InformationHolder<TMemoryWatcher>(memoryWatcher.Current, memoryWatcher.Old);
        }
    }
}
