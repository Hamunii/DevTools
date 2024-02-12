using System;
using System.Reflection;
using static TestingLib.Attributes;

namespace DevTools {
    public class MethodListing {
        public Type type;
        public MethodBase methodBase;
        public string value;
        public Type valueType;
        public Visibility visibility;
        public bool state;

        public MethodListing(Type _type, MethodBase _methodBase, string _value, Visibility _visiblity) {
            type = _type;
            methodBase = _methodBase;
            value = _value;
            valueType = _value.GetType();
            state = false;
            visibility = _visiblity;
        }

        public MethodListing(Type _type, MethodBase _methodBase, int _value, Visibility _visiblity) {
            type = _type;
            methodBase = _methodBase;
            value = _value.ToString();
            valueType = _value.GetType();
            state = false;
            visibility = _visiblity;
        }

        public MethodListing(Type _type, MethodBase _methodBase, bool _value, Visibility _visiblity) {
            type = _type;
            methodBase = _methodBase;
            value = _value.ToString();
            valueType = _value.GetType();
            state = false;
            visibility = _visiblity;
        }
    }
}