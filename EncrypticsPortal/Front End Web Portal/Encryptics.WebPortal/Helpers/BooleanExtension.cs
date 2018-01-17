using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using Encryptics.WebPortal.Models;

namespace Encryptics.WebPortal.Helpers
{
    public static class BooleanExtension
    {
        /// <summary>
        ///     Return options represnting the True and False titles of a
        ///     boolean field.
        /// </summary>
        /// <returns>
        ///     A list with the false title at position 0,
        ///     and true title at position 1.
        /// </returns>
        public static IList<SelectListItem> OptionsForBoolean<TModel,
                                                              TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression,
                                                                        htmlHelper.ViewData);
            object trueTitle;
            metaData.AdditionalValues.TryGetValue(
                BooleanDisplayValuesAttribute.TRUE_TITLE_ADDITIONAL_VALUE_NAME,
                out trueTitle);
            trueTitle = trueTitle ?? "Yes";

            object falseTitle;
            metaData.AdditionalValues.TryGetValue(
                BooleanDisplayValuesAttribute.FALSE_TITLE_ADDITIONAL_VALUE_NAME,
                out falseTitle);
            falseTitle = falseTitle ?? "No";

            var options = new[]
                {
                    new SelectListItem
                        {
                            Text = (string) falseTitle,
                            Value = Boolean.FalseString
                        },
                    new SelectListItem
                        {
                            Text = (string) trueTitle,
                            Value = Boolean.TrueString
                        }
                };
            return options;
        }

        public static string BooleanDisplayFor<TModel,
                                        TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression,
                                                                        htmlHelper.ViewData);

            object trueTitle;
            metaData.AdditionalValues.TryGetValue(
                BooleanDisplayValuesAttribute.TRUE_TITLE_ADDITIONAL_VALUE_NAME,
                out trueTitle);
            trueTitle = trueTitle ?? "Yes";

            object falseTitle;
            metaData.AdditionalValues.TryGetValue(
                BooleanDisplayValuesAttribute.FALSE_TITLE_ADDITIONAL_VALUE_NAME,
                out falseTitle);
            falseTitle = falseTitle ?? "No";

            if (metaData.Model is Boolean)
            {
                return (((bool)metaData.Model) ? trueTitle : falseTitle).ToString();
            }

            return metaData.Model == null ? falseTitle.ToString() : trueTitle.ToString();
        }
    }
}