using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using PocoPropertyData;

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

        public static IHtmlString AntiForgeryTokenValue(this HtmlHelper htmlHelper)
        {
            var field = htmlHelper.AntiForgeryToken().ToHtmlString();
            var beginIndex = field.IndexOf("value=\"") + 7;
            var endIndex = field.IndexOf("\"", beginIndex);
            return new HtmlString(field.Substring(beginIndex, endIndex - beginIndex));
        }
        public static MvcHtmlString LabelEx(this HtmlHelper html, string expression, string labelText, object htmlAttributes)
        {
            if (String.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }
            //var sb = new StringBuilder();
            //sb.Append(labelText);
            //sb.Append(":");

            var tag = new TagBuilder("label");
            //if (!string.IsNullOrWhiteSpace(id))
            //{
            //    tag.Attributes.Add("id", id);
            //}
            //else if (generatedId)
            //{
            //    tag.Attributes.Add("id", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName) + "_Label");
            //}
            if (htmlAttributes == null)
            {
                htmlAttributes = new
                {
                    @class = ""
                };
            }
            var dic = htmlAttributes.PropertiesAsDictionary();

            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(expression));
            tag.MergeAttributes(dic, true);
            tag.InnerHtml = labelText;


            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }
        public static MvcHtmlString LabelEx(this HtmlHelper html, string expression, string id = "", bool generatedId = false)
        {
            return LabelHelper(html, ModelMetadata.FromStringExpression(expression, html.ViewData), expression, id, generatedId);
        }

        public static MvcHtmlString LabelForEx<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string id = "", bool generatedId = false)
        {
            return LabelHelper(html, ModelMetadata.FromLambdaExpression(expression, html.ViewData), ExpressionHelper.GetExpressionText(expression), id, generatedId);
        }

        internal static MvcHtmlString LabelHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string id, bool generatedId)
        {
            string labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (String.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }
            var sb = new StringBuilder();
            sb.Append(labelText);
            sb.Append(":");

            var tag = new TagBuilder("label");
            if (!string.IsNullOrWhiteSpace(id))
            {
                tag.Attributes.Add("id", id);
            }
            else if (generatedId)
            {
                tag.Attributes.Add("id", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName) + "_Label");
            }

            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            //tag.SetInnerText();

            tag.InnerHtml = sb.ToString();

            var span = new TagBuilder("span");
            span.AddCssClass("requiredStar");
            span.SetInnerText("*");
            if (metadata.IsRequired)
                tag.InnerHtml += span.ToString();

            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
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
        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, 
                                               String name,
                                               IEnumerable<SelectListItem> items,
                                               object htmlAttributes)
        {

            //TagBuilder tag;
            //StringBuilder checklist = new StringBuilder();
            //foreach (var item in items)
            //{
            //    tag = new TagBuilder("input");
            //    tag.Attributes["type"] = "checkbox";
            //    tag.Attributes["value"] = item.Value.ToString();
            //    tag.Attributes["name"] =name;
            //    if (item.Selected)
            //    {
            //        tag.Attributes["checked"] = "checked";
            //    }
            //    tag.InnerHtml = item.Text;
            //    checklist.Append(tag.ToString());
            //    checklist.Append("<br />");
            //}

            if (htmlAttributes == null)
            {
                htmlAttributes = new
                {
                    @class = name
                };
            }
            var dic = htmlAttributes.PropertiesAsDictionary();
            if (!dic.ContainsKey("class"))
            {
                dic.Add("class", name);
            }

            dic["class"] = dic["class"] + " CheckBoxList";
            
            var ol = new TagBuilder("ol");
            foreach (var k in dic.Keys)
            {
                ol.Attributes.Add(new KeyValuePair<string, string>(k, dic[k]));
            }


            var sb = new StringBuilder();
            foreach (var item in items)
            {
                var id = name;
                var baseId = name + "_" + item.Value;
                if (!string.IsNullOrWhiteSpace(htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix))
                {
                    id = string.Format(
                        "{0}_{1}",
                        htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix,
                        id
                        );
                    baseId = string.Format(
                        "{0}_{1}",
                        htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix,
                        baseId
                        );
                }

                var radio = htmlHelper.CheckBox(id, item.Selected, new { id = baseId, value = item.Value }).ToHtmlString();
                sb.Append("<li>");
                sb.AppendFormat(
                    "<label for=\"{0}\">{2} {1}</label>",
                    baseId,
                    HttpUtility.HtmlEncode(item.Text),
                    radio
                );
                sb.Append("</li>");
            }
            ol.InnerHtml = sb.ToString();

            return MvcHtmlString.Create(ol.ToString());

            //return MvcHtmlString.Create(checklist.ToString());
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
        public static MvcHtmlString EnumDropDownListForEx<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
        {
            return EnumDropDownListForEx(htmlHelper, expression, null);
        }

        public static MvcHtmlString EnumDropDownListForEx<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
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
        Expression<Func<TModel, TProperty>> expression, List<SelectListItem> selectList, object htmlAttributes = null
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
                if (htmlAttributes == null)
                {
                    htmlAttributes = new
                    {
                        id = id
                    };
                }
                var dic = new Dictionary<string, string>();
                if (htmlAttributes != null)
                {
                    dic = htmlAttributes.PropertiesAsDictionary();
                }
                if (!dic.ContainsKey("id"))
                {
                    dic.Add("id", id);
                }
                else
                {
                    dic["id"] = id;
                }
                var dic2 = new Dictionary<string, object>();
                foreach (var k in dic.Keys)
                {
                    dic2.Add(k, dic[k]);
                }
                var radio = htmlHelper.RadioButtonFor(expression, select.Value, dic2).ToHtmlString();
                sb.Append("<li>");
                sb.AppendFormat(
                    "<label for=\"{0}\">{2} {1}</label>",
                    id,
                    HttpUtility.HtmlEncode(select.Text),
                    radio
                );
                sb.Append("</li>");
            }
            sb.Append("</ol>");
            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString RadioButtonList<TModel>(
        this HtmlHelper<TModel> htmlHelper,
        string name, List<SelectListItem> selectList, object htmlAttributes = null
    )
        {
           // ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var sb = new StringBuilder();
            sb.Append("<ol class='RadioList'>");
            foreach (var select in selectList)
            {
                var id = string.Format(
                    "{0}_{1}_{2}",
                    htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix,
                    name,
                    select.Value
                );
                if (htmlAttributes == null)
                {
                    htmlAttributes = new
                    {
                        id = id
                    };
                }
                var dic = htmlAttributes.PropertiesAsDictionary();
                if (!dic.ContainsKey("id"))
                {
                    dic.Add("id", id);
                }
                else
                {
                    dic["id"] = id;
                }
                var dic2 = new Dictionary<string, object>();
                foreach (var k in dic.Keys)
                {
                    dic2.Add(k, dic[k]);
                }
                var radio = htmlHelper.RadioButton(name, select.Value, dic2).ToHtmlString();
                sb.Append("<li>");
                sb.AppendFormat(
                    "<label for=\"{0}\">{2} {1}</label>",
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

        public static MvcHtmlString DropDownYesNoForCheck<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            return DropDownYesNoBooleanFor(htmlHelper, expression);

        }

        public static MvcHtmlString DropDownYesNoBooleanFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, 
            object htmlAttributes = null)
        {
            
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
            var dropdown = htmlHelper.DropDownListFor(expression, selectList, htmlAttributes).ToHtmlString();


            return MvcHtmlString.Create(dropdown);
        }
        public static MvcHtmlString DropDownYesNoBoolean<TModel>(this HtmlHelper<TModel> htmlHelper,
            string id,
            Boolean value,
            object htmlAttributes = null)
        {
            var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem()
            {
                Text = "No",
                Value = "False",
                Selected=!value
            });
            selectList.Add(new SelectListItem()
            {
                Text = "Yes",
                Value = "True",
                Selected = value
            });
            var dropdown = htmlHelper.DropDownList(id, selectList, htmlAttributes).ToHtmlString();


            return MvcHtmlString.Create(dropdown);
        }

        
        public static MvcHtmlString RadioYesNoForCheck<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            return RadioYesNoBooleanFor(htmlHelper, expression);

        }

        public static MvcHtmlString RadioYesNoBooleanFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            object htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            if (htmlAttributes == null)
            {
                htmlAttributes = new
                {
                    @class = ""
                };
            }
            //var dic = htmlAttributes.PropertiesAsDictionary();
            //var dic2 = new Dictionary<string, object>();
            //foreach (var k in dic.Keys)
            //{
            //    dic2.Add(k, dic[k]);
            //}

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
            var dropdown = htmlHelper.RadioButtonListFor(expression, selectList, htmlAttributes).ToHtmlString();


            return MvcHtmlString.Create(dropdown);
        }

        public static MvcHtmlString RadioYesNoBoolean<TModel>(this HtmlHelper<TModel> htmlHelper,
            string name,
            object htmlAttributes = null)
        {
           // ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            if (htmlAttributes == null)
            {
                htmlAttributes = new
                {
                    @class = ""
                };
            }
            var dic = htmlAttributes.PropertiesAsDictionary();
            var dic2 = new Dictionary<string, object>();
            foreach (var k in dic.Keys)
            {
                dic2.Add(k, dic[k]);
            }

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
            return htmlHelper.RadioButtonList(name, selectList, dic2);
        }
    }
}