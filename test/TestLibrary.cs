using System;

namespace Efl
{
    namespace Eo
    {
        [System.AttributeUsage(System.AttributeTargets.Class |
                               System.AttributeTargets.Interface |
                               System.AttributeTargets.Enum |
                               System.AttributeTargets.Delegate |
                               System.AttributeTargets.Struct,
                               AllowMultiple = false,
                               Inherited = false)
        ]
        public class BindingEntity: System.Attribute
        {
        }
    }
}

namespace Dummy
{

    [Efl.Eo.BindingEntity]
    public class CustomArgs : EventArgs
    {
    }

    [Efl.Eo.BindingEntity]
    public class Parent
    {
#pragma warning disable 0067
        public event EventHandler Clicked;

        protected event EventHandler<CustomArgs> CustomEvent;
#pragma warning restore 0067

        protected Parent()
        {
        }

        public Parent(int x, double y)
        {
        }

        public int PropGetSet { get; set; }
        public string PropGetOnly { get; }
        protected double PropSetOnly
        {
            set
            {
            }
        }

        public int PropPrivateSet { get; private set; }

        public void PublicMeth()
        {
        }

        protected void ProtectedMeth()
        {
        }
    }

    [Efl.Eo.BindingEntity]
    public class EmptyClass
    {
    }
}
