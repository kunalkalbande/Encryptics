using System;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Helpers
{
    public static class HtmlHelperExtension
    {
        public static MvcHtmlString ImageLink(this HtmlHelper htmlHelper, string imgSrc, string alt, string actionName, string controllerName, object routeValues, object htmlAttributes, object imgHtmlAttributes)
        {
            UrlHelper urlHelper = ((Controller)htmlHelper.ViewContext.Controller).Url;
            var imgtag = Image(htmlHelper, imgSrc, alt, imgHtmlAttributes);
            string url = urlHelper.Action(actionName, controllerName, routeValues);

            var imglink = new TagBuilder("a");
            imglink.MergeAttribute("href", url);
            imglink.InnerHtml = imgtag.ToHtmlString();
            imglink.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes), true);

            return MvcHtmlString.Create(imglink.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString Image(this HtmlHelper helper, string src, string altText, object imgHtmlAttributes)
        {
            var builder = new TagBuilder("img");
            builder.MergeAttribute("src", src);
            builder.MergeAttribute("alt", altText);
            builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(imgHtmlAttributes), true);
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString ToggleFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string textClass = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);

            var divTag = new TagBuilder("div");
            divTag.MergeAttribute("class", "onoffswitch");

            var checkBoxTag = new TagBuilder("input");
            checkBoxTag.MergeAttribute("id", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            checkBoxTag.MergeAttribute("name", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName));
            checkBoxTag.MergeAttribute("value", "true");
            checkBoxTag.MergeAttribute("type", "checkbox");

            if (metadata.Model != null && (bool)metadata.Model)
            {
                checkBoxTag.Attributes.Add("checked", null);
            }

            checkBoxTag.AddCssClass("onoffswitch-checkbox");

            var labelTag = new TagBuilder("label");
            labelTag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            labelTag.AddCssClass("onoffswitch-label");

            var innerDivTag = new TagBuilder("div");
            innerDivTag.MergeAttribute("class", "onoffswitch-inner");
            if (!string.IsNullOrEmpty(textClass))
            {
                innerDivTag.AddCssClass(textClass);
            }

            var switchDivTag = new TagBuilder("div");
            switchDivTag.MergeAttribute("class", "onoffswitch-switch");

            labelTag.InnerHtml = innerDivTag.ToString(TagRenderMode.Normal) +
                                 switchDivTag.ToString(TagRenderMode.Normal);

            divTag.InnerHtml = checkBoxTag.ToString(TagRenderMode.SelfClosing) +
                               labelTag.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(divTag.ToString(TagRenderMode.Normal));
        }

        public static IHtmlString HelpTextFor<TModel, TValue>(this HtmlHelper<TModel> html,
                                                               Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            if (metadata.AdditionalValues.ContainsKey("HelpText"))
            {
                var helpText = metadata.AdditionalValues["HelpText"] as string;

                return new HtmlString(html.Encode(helpText));
            }

            return MvcHtmlString.Empty;
        }
    }
}