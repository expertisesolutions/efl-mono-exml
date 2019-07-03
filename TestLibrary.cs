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
    public class Parent
    {
        public delegate void ClickedHandler(object sender, EventArgs e);

        event ClickedHandler Clicked;

        public Parent()
        {
        }

        public Parent(int x, double y)
        {
        }
    }

    [Efl.Eo.GeneratedEntity]
    public class EmptyClass
    {
    }
}
