using OpenTS2.Diagnostic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Houses some common TS2 boolean properties
/// </summary>

namespace Assets.Scripts.OpenTS2.Diagnostic
{
    public class SimpleProperty<T> : IConsoleProperty
    {
        string serializedValue
        {
            get => Convert.ToString(myValue);
            set => myValue = (T)Convert.ChangeType(value, typeof(T));
        }

        T myValue;

        public SimpleProperty()
        {
        }
        public SimpleProperty(T DefaultValue) : this()
        {
            myValue = DefaultValue;
        }

        public string GetStringValue() => serializedValue;
        public Type GetValueType() => typeof(T);
        public void SetStringValue(string value) => serializedValue = value;

        public static SimpleProperty<T> Create() => new SimpleProperty<T>();
        public static SimpleProperty<T> Create(T DefaultValue) => new SimpleProperty<T>(DefaultValue);
    }
}
