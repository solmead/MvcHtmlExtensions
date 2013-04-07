﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace MvcHtmlExtensions
{
    public static class Extensions
    {

        public static Dictionary<string, string> ToDictionary(this FormCollection collection)
        {
            var Dic = new Dictionary<String, String>();
            foreach (string C in collection.Keys)
            {
                Dic.Add(C, collection[C]);
            }
            return Dic;
        }

        public static MvcHtmlString CheckBoxList<T>(this HtmlHelper helper,
                                               String name,
                                               IEnumerable<T> items,
                                               String textField,
                                               String valueField,
                                               IEnumerable<T> selectedItems = null)
        {
            Type itemstype = typeof(T);
            PropertyInfo textfieldInfo = itemstype.GetProperty(textField, typeof(String));
            PropertyInfo valuefieldInfo = itemstype.GetProperty(valueField);

            TagBuilder tag;
            StringBuilder checklist = new StringBuilder();
            foreach (var item in items)
            {
                tag = new TagBuilder("input");
                tag.Attributes["type"] = "checkbox";
                tag.Attributes["value"] = valuefieldInfo.GetValue(item, null).ToString();
                tag.Attributes["name"] = name;
                if (selectedItems != null && selectedItems.Contains(item))
                {
                    tag.Attributes["checked"] = "checked";
                }
                tag.InnerHtml = textfieldInfo.GetValue(item, null).ToString();
                checklist.Append(tag.ToString());
                checklist.Append("<br />");
            }
            return MvcHtmlString.Create(checklist.ToString());
        }

        public static IDisposable BeginHtmlFieldPrefixScope(this HtmlHelper html, string htmlFieldPrefix)
        {
            return new HtmlFieldPrefixScope(html.ViewData.TemplateInfo, htmlFieldPrefix);
        }

        public static IHtmlString HtmlLabelFor<TModel, TProperty>(
        this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TProperty>> expression
    )
        {
            var metadata = ModelMetadata.FromLambdaExpression<TModel, TProperty>(expression, htmlHelper.ViewData);
            var htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            var labelText = (metadata.DisplayName ?? (metadata.PropertyName ?? htmlFieldName.Split(new char[] { '.' }).Last<string>()));
            if (string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }
            var label = new TagBuilder("label");
            label.Attributes.Add("for", TagBuilder.CreateSanitizedId(htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
            label.InnerHtml = labelText;
            return new HtmlString(label.ToString());

        }
        private static Type GetNonNullableModelType(ModelMetadata modelMetadata)
        {
            Type realModelType = modelMetadata.ModelType;

            Type underlyingType = Nullable.GetUnderlyingType(realModelType);
            if (underlyingType != null)
            {
                realModelType = underlyingType;
            }
            return realModelType;
        }

        private static readonly SelectListItem[] SingleEmptyItem = new[] { new SelectListItem { Text = "", Value = "" } };

        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if ((attributes != null) && (attributes.Length > 0))
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string GetEnumDescription<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, TEnum value)
        {
            return GetEnumDescription(value);
        }
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
        {
            return EnumDropDownListFor(htmlHelper, expression, null);
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            Type enumType = GetNonNullableModelType(metadata);
            IEnumerable<TEnum> values = Enum.GetValues(enumType).Cast<TEnum>();

            IEnumerable<SelectListItem> items = from value in values
                                                select new SelectListItem
                                                {
                                                    Text = GetEnumDescription(value),
                                                    Value = value.ToString(),
                                                    Selected = value.Equals(metadata.Model)
                                                };

            // If the enum is nullable, add an 'empty' item to the collection
            if (metadata.IsNullableValueType)
                items = SingleEmptyItem.Concat(items);

            return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        }
        public static MvcHtmlString EnumRadioButtonListFor<TModel, TProperty>(
        this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TProperty>> expression
    )
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            Type enumType = GetNonNullableModelType(metadata);
            IEnumerable<TProperty> values = Enum.GetValues(enumType).Cast<TProperty>();
            var sb = new StringBuilder();
            sb.Append("<ol class='RadioList'>");
            foreach (var name in values)
            {
                var id = string.Format(
                    "{0}_{1}_{2}",
                    htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix,
                    metadata.PropertyName,
                    name
                );

                var radio = htmlHelper.RadioButtonFor(expression, name, new { id = id }).ToHtmlString();
                sb.Append("<li>");
                sb.AppendFormat(
                    "{2} <label for=\"{0}\">{1}</label>",
                    id,
                    HttpUtility.HtmlEncode(GetEnumDescription(name)),
                    radio
                );
                sb.Append("</li>");
            }
            sb.Append("</ol>");
            return MvcHtmlString.Create(sb.ToString());
        }
        public static MvcHtmlString RadioButtonListFor<TModel, TProperty>(
        this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TProperty>> expression, List<SelectListItem> selectList
    )
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var sb = new StringBuilder();
            sb.Append("<ol class='RadioList'>");
            foreach (var select in selectList)
            {
                var id = string.Format(
                    "{0}_{1}_{2}",
                    htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix,
                    metadata.PropertyName,
                    select.Value
                );

                var radio = htmlHelper.RadioButtonFor(expression, select.Value, new { id = id }).ToHtmlString();
                sb.Append("<li>");
                sb.AppendFormat(
                    "{2} <label for=\"{0}\">{1}</label>",
                    id,
                    HttpUtility.HtmlEncode(select.Text),
                    radio
                );
                sb.Append("</li>");
            }
            sb.Append("</ol>");
            return MvcHtmlString.Create(sb.ToString());
        }
        public static MvcHtmlString DisplayColumnNameFor<TModel, TClass, TProperty> (this HtmlHelper<TModel> helper, IEnumerable<TClass> model, Expression<Func<TClass, TProperty>> expression)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            name = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(
                () => Activator.CreateInstance<TClass>(), typeof(TClass), name);

            return new MvcHtmlString(metadata.DisplayName);
        }

    public static MvcHtmlString DropDownYesNoForCheck<TModel, TProperty> (this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            
            var selectList = new List<SelectListItem>();
        selectList.Add(new SelectListItem()
            {
                Text="No",
                Value="False"
            });
        selectList.Add(new SelectListItem()
        {
            Text = "Yes",
            Value = "True"
        });
            var dropdown = htmlHelper.DropDownListFor(expression,selectList).ToHtmlString();


            return MvcHtmlString.Create(dropdown);
        }
    public static MvcHtmlString RadioYesNoForCheck<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
    {
        ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

        var selectList = new List<SelectListItem>();
        selectList.Add(new SelectListItem()
        {
            Text = "No",
            Value = "False"
        });
        selectList.Add(new SelectListItem()
        {
            Text = "Yes",
            Value = "True"
        });
        var dropdown = htmlHelper.RadioButtonListFor(expression, selectList).ToHtmlString();


        return MvcHtmlString.Create(dropdown);
    }
    }
}