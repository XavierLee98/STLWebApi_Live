using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.Dynamic;
using System.Globalization;

namespace StarLaiPortal.WebApi.Helper
{
    public static class ExpandoParser
    {
        public static void ParseExObjectXPO<T>(IDictionary<string, object> exobject, object obj, IObjectSpace objectSpace) where T : XPObject
            => ParseExObjectXPO(typeof(T), exobject, obj, objectSpace);
        public static void ParseExObjectXPO(Type type, IDictionary<string, object> exobject, object obj, IObjectSpace objectSpace)
        {
            ITypeInfo typeInfo = objectSpace.TypesInfo.FindTypeInfo(type);
            List<string> memberNameList = exobject.Select(pp => pp.Key).ToList();
            foreach (string memberName in memberNameList)
            {
                IMemberInfo memberInfo = typeInfo.FindMember(memberName);
                if (memberInfo != null)
                {
                    if (memberInfo.IsAssociation)
                    {
                        if (memberInfo.IsList)
                        {
                            object detailexobj = null;
                            if (!exobject.TryGetValue(memberName, out detailexobj)) return;
                            if (detailexobj == null) return;
                            ParseExObjectXPOList((detailexobj as List<object>).OfType<ExpandoObject>().ToList(), obj, memberName, objectSpace);
                        }
                        else if (memberInfo.MemberType.IsAssignableTo(typeof(XPLiteObject)) || memberInfo.MemberType.IsAssignableTo(typeof(BaseObject)) || memberInfo.MemberType.IsAssignableTo(typeof(XPObject)))
                        {
                            ParseAssociationProperty(exobject, obj, memberInfo, objectSpace);
                        }
                    }
                    else if (memberInfo.MemberType.IsAssignableTo(typeof(XPLiteObject)) || memberInfo.MemberType.IsAssignableTo(typeof(BaseObject)) || memberInfo.MemberType.IsAssignableTo(typeof(XPObject)))
                    {
                        ParseAssociationProperty(exobject, obj, memberInfo, objectSpace);
                    }
                    else
                    {
                        ParseSimpleProperty(exobject, obj, memberInfo);
                    }
                }
            }
        }
        public static void ParseExObjectXPOList(List<ExpandoObject> exobject, object parent, string memberName, IObjectSpace objectSpace)
        {
            var prop = parent.GetType().GetProperty(memberName);
            var ele_type = prop.PropertyType.GetGenericArguments()[0];
            var add_method = prop.PropertyType.GetMethod("Add");

            var prop_obj = prop.GetValue(parent); // PackListDetails

            foreach (ExpandoObject exobj in exobject)
            {
                object obj = objectSpace.CreateObject(ele_type);
                ParseExObjectXPO(ele_type, exobj, obj, objectSpace);
                add_method.Invoke(prop_obj, new[] { obj });   // PackListDetails.Add
            }
        }
        public static void Parseexobject<T>(IDictionary<string, object> exobject, object obj, IObjectSpace objectSpace) where T : BaseObject
            => Parseexobject(typeof(T), exobject, obj, objectSpace);
        public static void Parseexobject(Type type, IDictionary<string, object> exobject, object obj, IObjectSpace objectSpace)
        {
            ITypeInfo typeInfo = objectSpace.TypesInfo.FindTypeInfo(type);
            List<string> memberNameList = exobject.Select(pp => pp.Key).ToList();
            foreach (string memberName in memberNameList)
            {
                IMemberInfo memberInfo = typeInfo.FindMember(memberName);
                if (memberInfo.MemberType.IsAssignableTo(typeof(XPLiteObject)) || memberInfo.MemberType.IsAssignableTo(typeof(BaseObject)) || memberInfo.MemberType.IsAssignableTo(typeof(XPObject)))
                {
                    ParseAssociationProperty(exobject, obj, memberInfo, objectSpace);
                }
                else
                {
                    ParseSimpleProperty(exobject, obj, memberInfo);
                }
            }
        }
        private static void ParseSimpleProperty(IDictionary<string,object> exobject, object obj, IMemberInfo memberInfo)
        {
            object jValue = exobject.Where(pp => pp.Key == memberInfo.Name).Select(pp => pp.Value).FirstOrDefault();
            object value = ConvertType(jValue, memberInfo);
            memberInfo.SetValue(obj, value);
        }
        private static void ParseAssociationProperty(IDictionary<string, object> exobject, object obj, IMemberInfo memberInfo, IObjectSpace objectSpace)
        {
            string keyPropertyName = memberInfo.MemberTypeInfo.KeyMember.Name;
            object keyValue = exobject.Where(pp => pp.Key == memberInfo.Name).Select(pp => pp.Value).FirstOrDefault();
            if (keyValue == null) return;
            object value = null;
            if (memberInfo.MemberType.IsAssignableTo(typeof(XPObject)))
                if (keyValue.GetType() == typeof(Int64))
                    value = objectSpace.GetObjectByKey(memberInfo.MemberType, int.Parse(keyValue.ToString()));
                else
                    value = objectSpace.GetObjectByKey(memberInfo.MemberType, keyValue);
            else if (memberInfo.MemberType.IsAssignableTo(typeof(XPLiteObject)))
                if (keyValue.GetType() == typeof(Int64))
                    value = objectSpace.GetObjectByKey(memberInfo.MemberType, int.Parse(keyValue.ToString()));
                else
                    value = objectSpace.GetObjectByKey(memberInfo.MemberType, keyValue);
            memberInfo.SetValue(obj, value);
        }
        private static object ConvertType(object value, IMemberInfo memberInfo)
        {
            if (value != null)
            {
                if (value.GetType() != memberInfo.MemberType)
                {
                    if (value is string && memberInfo.MemberType == typeof(Guid))
                    {
                        value = Guid.Parse((string)value);
                    }
                    else if (memberInfo.MemberType.IsEnum)
                    {
                        if (value is string stringValue)
                        {
                            return Enum.Parse(memberInfo.MemberType, stringValue);
                        }
                        else if (value is int intValue)
                        {
                            //value = Enum.ToObject(memberInfo.MemberType, intValue);
                        }
                        else if (value is long longValue)
                        {
                            value = Convert.ToInt32(longValue);
                            //int intConvertValue = Convert.ToInt32(longValue);
                            //value = Enum.ToObject(memberInfo.MemberType, intConvertValue);
                        }
                    }
                    else
                    {
                        value = Convert.ChangeType(value, memberInfo.MemberType, CultureInfo.InvariantCulture);
                    }
                }
            }
            return value;
        }
    }
}
