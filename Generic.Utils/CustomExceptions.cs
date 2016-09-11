using System;

namespace Generic.Utils
{
    [Serializable]
    public class ObjectIsLockedException : Exception
    {
        public ObjectIsLockedException()
            : base("Object is Locked")
        {

        }
        public ObjectIsLockedException(string message, params object[] args)
            : base(String.Format(message, args))
        {

        }
    }

    [Serializable]
    public class ItemAlreadyAddedException : Exception
    {
        public ItemAlreadyAddedException()
            : base("Item has already been added")
        {

        }
        public ItemAlreadyAddedException(string message, params object[] args)
            : base(String.Format(message, args))
        {

        }
    }

    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException(string message, params object[] args)
            : base(String.Format(message, args))
        {

        }
    }

    [Serializable]
    public class ReadOnlyException : Exception
    {
        public ReadOnlyException()
            : base("Field is Readonly")
        {

        }
        public ReadOnlyException(string message, params object[] args)
            : base(String.Format(message, args))
        {

        }
    }
}
