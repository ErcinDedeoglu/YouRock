using System;

namespace YouRock
{
    public class ReflectionHelper
    {
        public static dynamic ObjectInvoke(dynamic className, string objectName, object parameters = null)
        {
            if (parameters != null)
            {
                return className.GetType().GetMethod(objectName).Invoke(className, new Object[] { parameters });
            }
            else
            {
                return className.GetType().GetMethod(objectName).Invoke(className, null);
            }
        }
    }
}