using System;

namespace Discussion.Core.Utilities
{
    public class ArgGuard
    {
        public static void NotNull(object obj, string name)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(name);
            }
        }
        
        public static void MakeSure(bool requirement, string name)
        {
            if (!requirement)
            {
                throw new ArgumentException(name + " 参数不符合要求");
            }
        }
    }
}