using System;

namespace Generic.Utils
{
    public interface IPrototype<out T>
    {
        T Copy();
    }

    public interface ILockable
    {
        void Lock();
        void Unlock();
        bool IsLocked { get; }
    }
}
