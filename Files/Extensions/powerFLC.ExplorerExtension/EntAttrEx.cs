using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Connectivity.WebServices;

namespace powerFLC.ExplorerExtension
{
    public class EntAttrEx : EntAttr
    {
        public string Namespace { get; set; }

        private string _tenant;
        public string Tenant
        {
            get
            {
                if (_tenant == null) ExtractUrnFields();
                return _tenant;
            }
        }

        private string _workspace;
        public string Workspace
        {
            get
            {
                if (_workspace == null) ExtractUrnFields();
                return _workspace;
            }
        }

        private string _item;
        public string Item
        {
            get
            {
                if (_item == null) ExtractUrnFields();
                return _item;
            }
        }

        public EntAttrEx(EntAttr attribute, string ns)
        {
            Namespace = ns;
            Attr = attribute.Attr;
            Cloaked = attribute.Cloaked;
            EntityId = attribute.EntityId;
            Val = attribute.Val;
        }

        private void ExtractUrnFields()
        {
            var urn = Val;
            if (urn == null)
                return;

            var contents = urn.Split(':').Reverse().ToArray();
            var values = contents[0].Split('.').ToArray();
            var names = contents[1].Split('.').ToArray();
            _tenant = values[Array.IndexOf(names, "tenant")];
            _workspace = values[Array.IndexOf(names, "workspace")];
            _item = values[Array.IndexOf(names, "item")];
        }
    }

    public static class ExtensionMethods
    {
        public static EntAttrEx[] ToExtended(this EntAttr[] attributes, string ns)
        {
            var result = new List<EntAttrEx>();
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                    result.Add(new EntAttrEx(attribute, ns));
            }
            return result.ToArray();
        }
    }
}