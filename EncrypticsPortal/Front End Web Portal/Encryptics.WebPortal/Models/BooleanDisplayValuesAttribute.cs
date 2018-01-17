using System;
using System.Resources;
using System.Web.Mvc;
//using Resources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Models
{
    [AttributeUsageAttribute(AttributeTargets.Method|AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Parameter, AllowMultiple = false)]
    public class BooleanDisplayValuesAttribute : Attribute, IMetadataAware
    {
        public const string TRUE_TITLE_ADDITIONAL_VALUE_NAME = "BooleanTrueValueTitle";
        public const string FALSE_TITLE_ADDITIONAL_VALUE_NAME = "BooleanFalseValueTitle";

        public Type ResourceType { get; set; }
        public string TrueValue { get; set; }
        public string FalseValue { get; set; }
        public string TrueValueName { get; set; }
        public string FalseValueName { get; set; }

        public BooleanDisplayValuesAttribute(Type resourceType = null, string trueValue = "",
                                             string falseValue = "", string trueValueName = "",
                                             string falseValueName = "")
        {
            FalseValueName = falseValueName;
            TrueValueName = trueValueName;
            FalseValue = falseValue;
            TrueValue = trueValue;
            ResourceType = resourceType;
        }

        public BooleanDisplayValuesAttribute(string trueValue, string falseValue)
        {
            TrueValue = trueValue;
            FalseValue = falseValue;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            if (ResourceType != null)
            {
                var resourceManager = new ResourceManager(ResourceType);

                metadata.AdditionalValues[TRUE_TITLE_ADDITIONAL_VALUE_NAME] = resourceManager.GetObject(TrueValueName);
                metadata.AdditionalValues[FALSE_TITLE_ADDITIONAL_VALUE_NAME] = resourceManager.GetObject(FalseValueName);
            }
            else
            {
                metadata.AdditionalValues[TRUE_TITLE_ADDITIONAL_VALUE_NAME] = TrueValue;
                metadata.AdditionalValues[FALSE_TITLE_ADDITIONAL_VALUE_NAME] = FalseValue;
            }
        }
    }

    //public class YesNoDisplayValuesAttribute : BooleanDisplayValuesAttribute
    //{
    //    public YesNoDisplayValuesAttribute() : base(Resources.YesDisplay, Resources.NoDisplay)
    //    {
            
    //    }
    //}

    //public class ActiveInactiveDisplayValuesAttribute : BooleanDisplayValuesAttribute
    //{
    //    public ActiveInactiveDisplayValuesAttribute(): base(Resources.ActiveDisplay, Resources.InactiveDisplay)
    //    {
            
    //    }
    //}
}