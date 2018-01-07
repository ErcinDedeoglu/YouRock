using System;

namespace YouRock
{
    public class ReflectionHelper
    {
        public static dynamic ObjectInvoke(dynamic className, string objectName, object parameter1 = null, object parameter2 = null, object parameter3 = null)
        {
            if (parameter3 != null)
            {
                return className.GetType().GetMethod(objectName).Invoke(className, new Object[] { parameter1, parameter2, parameter3 });
            }
            else if (parameter2 != null)
            {
                return className.GetType().GetMethod(objectName).Invoke(className, new Object[] { parameter1, parameter2 });
            }
            else if (parameter1 != null)
            {
                return className.GetType().GetMethod(objectName).Invoke(className, new Object[] { parameter1 });
            }
            else
            {
                return className.GetType().GetMethod(objectName).Invoke(className, null);
            }
        }
    }
}