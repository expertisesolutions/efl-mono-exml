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
        public class GeneratedEntity: System.Attribute
        {
        }
    }
}

namespace Dummy
{

    [Efl.Eo.GeneratedEntity]
    public class CustomArgs : EventArgs
    {
    }

    [Efl.Eo.GeneratedEntity]
    public class Parent
    {
        public event EventHandler Clicked;

        public event EventHandler<CustomArgs> CustomEvent;

        public Parent()
        {
        }

        public Parent(int x, double y)
        {
        }

        public int PropGetSet { get; set; }
        public string PropGetOnly { get; }
        public double PropSetOnly
        {
            set
            {
            }
        }
    }

    [Efl.Eo.GeneratedEntity]
    public class EmptyClass
    {
    }
}
