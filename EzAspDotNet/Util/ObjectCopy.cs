using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace WebShared.Util
{
    public static class ObjectCopy
    {
        // Copyright 헝그리개발자(http://bemeal2.tistory.com)

        public static T DeepClone<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Object cannot be null.");

            return (T)Process(obj, new Dictionary<object, object>() { });
        }

        private static object Process(object obj, Dictionary<object, object> circular)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }

            if (type.IsArray)
            {
                if (circular.ContainsKey(obj))
                    return circular[obj];
                string typeNoArray = type.FullName.Replace("[]", string.Empty);
                Type elementType = Type.GetType(typeNoArray + ", " + type.Assembly.FullName);
                var array = obj as Array;
                Array arrCopied = Array.CreateInstance(elementType, array.Length);

                circular[obj] = arrCopied;

                for (int i = 0; i < array.Length; i++)
                {
                    object element = array.GetValue(i);
                    object objCopy = null;

                    if (element != null && circular.ContainsKey(element))
                        objCopy = circular[element];
                    else
                        objCopy = Process(element, circular);
                    arrCopied.SetValue(objCopy, i);
                }
                return Convert.ChangeType(arrCopied, obj.GetType());
            }

            if (type.IsClass)
            {
                if (circular.ContainsKey(obj))
                    return circular[obj];

                object objValue = Activator.CreateInstance(obj.GetType());
                circular[obj] = objValue;
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);

                    if (fieldValue == null)
                        continue;

                    object objCopy = circular.ContainsKey(fieldValue) ? circular[fieldValue] : Process(fieldValue, circular);
                    field.SetValue(objValue, objCopy);
                }
                return objValue;
            }
            else
                throw new ArgumentException("Unknown type");
        }
    }
}
