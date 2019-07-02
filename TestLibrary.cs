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
    public class Parent
    {
        public Parent()
        {
        }

        public Parent(int x)
        {
        }
    }

    [Efl.Eo.GeneratedEntity]
    public class EmptyClass
    {
    }
}
