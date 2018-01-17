using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<SelectListItem> SelectListFor<T>(T? selected)
            where T : struct
        {
            return selected == null
                       ? SelectListFor<T>()
                       : SelectListFor(selected.Value);
        }

        public static IEnumerable<SelectListItem> SelectListFor<T>() where T : struct
        {
            Type t = typeof(T);
            
            if (t.IsEnum)
            {
                var values = Enum.GetValues(typeof(T)).Cast<Enum>()
                                 .Select(e => new { Id = Convert.ToInt32(e), Name = e.GetDisplay() });

                return new SelectList(values, "Id", "Name");
            }
            
            return null;
        }

        public static IEnumerable<SelectListItem> SelectListFor<T>(T selected) where T : struct
        {
            Type t = typeof(T);
            
            if (t.IsEnum)
            {
                return Enum.GetValues(t).Cast<Enum>()
                                 .Select(e => new SelectListItem
                                 {
                                     Text = e.GetDisplay(),
                                     Value = e.ToString(),
                                     Selected =  selected.ToString().Equals(e.ToString())
                                 });
            }

            return null;
        }

        // Get the value of the display attribute if the 
        // enum has one, otherwise use the value.
        public static string GetDisplay<TEnum>(this TEnum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            if (fi != null)
            {
                var attributes =
                    (DisplayAttribute[]) fi.GetCustomAttributes(
                        typeof (DisplayAttribute),
                        false);

                if (attributes.Length > 0)
                {
                    string displayName;
                    if (attributes[0].ResourceType != null)
                    {
                        displayName =
                            new ResourceManager(attributes[0].ResourceType).GetObject(attributes[0].Name) as string;
                    }
                    else
                    {
                        displayName = attributes[0].Name;
                    }
                    return displayName;
                }
            }

            return value.ToString();
        }

        // Get the value of the description attribute if the 
        // enum has one, otherwise use the value.
        public static string GetDescription<TEnum>(this TEnum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            if (fi != null)
            {
                var attributes =
                    (DescriptionAttribute[])fi.GetCustomAttributes(
                        typeof(DescriptionAttribute),
                        false);

                if (attributes.Length > 0)
                {
                    return attributes[0].Description;
                }
            }

            return value.ToString();
        }
    }
}