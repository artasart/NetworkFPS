// Designed by KINEMATION, 2023

using System;
using UnityEngine;

namespace Demo.Scripts.Runtime
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TabAttribute : PropertyAttribute
    {
        public readonly string tabName;

        public TabAttribute(string tabName)
        {
            this.tabName = tabName;
        }
    }
}