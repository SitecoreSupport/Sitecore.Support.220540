using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Forms.Mvc.Data.Wrappers;
using Sitecore.Forms.Mvc.Interfaces;
using Sitecore.Forms.Mvc.Models;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Web;

namespace Sitecore.Support.Forms.Mvc.Services
{
  public class FormRepository : IRepository<FormModel>
  {
    private readonly Dictionary<Guid, FormModel> models = new Dictionary<Guid, FormModel>();

    public IRenderingContext RenderingContext
    {
      get;
      private set;
    }

    public FormRepository(IRenderingContext renderingContext)
    {
      Assert.ArgumentNotNull(renderingContext, "renderingContext");
      RenderingContext = renderingContext;
    }

    public FormModel GetModel(Guid uniqueId)
    {
      if (uniqueId != Guid.Empty && models.ContainsKey(uniqueId))
      {
        return (FormModel)models[uniqueId].Clone();
      }
      string dataSource = RenderingContext.Rendering.DataSource;
      string text = (string.IsNullOrEmpty(dataSource) || !ID.IsID(dataSource)) ? RenderingContext.Rendering.Parameters[Sitecore.Forms.Mvc.Constants.FormId] : dataSource;
      if (!ID.IsID(text))
      {
        return null;
      }
      ID id = ID.Parse(text);
      //Item item = RenderingContext.Database.GetItem(id);
      Item item = null;
      string contextLanguage = HttpContext.Current.Request.QueryString["wffm.FormLanguage"];
      if (!string.IsNullOrEmpty(contextLanguage))
      {
        item = (RenderingContext.Database as Sitecore.Support.Forms.Mvc.Data.Wrappers.WrappedDatabase).GetItem(id, Sitecore.Globalization.Language.Parse(contextLanguage));
      }
      else
      {
        item = RenderingContext.Database.GetItem(id);
      }
      Assert.IsNotNull(item, "Form item is absent");
      /*FormModel formModel = new FormModel(uniqueId, item);
      formModel.ReadQueryString = MainUtil.GetBool(RenderingContext.Rendering.Parameters[Sitecore.Forms.Mvc.Constants.ReadQueryString], false);
      formModel.QueryParameters = HttpUtility.ParseQueryString(WebUtil.GetQueryString());*/
      Sitecore.Forms.Mvc.Models.FormModel formModel = Convert(new Sitecore.Support.Forms.Mvc.Models.FormModel(uniqueId, item)
      {
        ReadQueryString = MainUtil.GetBool(RenderingContext.Rendering.Parameters[Sitecore.Forms.Mvc.Constants.ReadQueryString], false),
        QueryParameters = HttpUtility.ParseQueryString(WebUtil.GetQueryString())
      });
      FormModel formModel2 = formModel;
      models.Add(uniqueId, formModel2);
      return formModel2;
    }

    public FormModel GetModel()
    {
      return GetModel(RenderingContext.Rendering.UniqueId);
    }

    private static Sitecore.Forms.Mvc.Models.FormModel Convert(Sitecore.Support.Forms.Mvc.Models.FormModel formModel)
    {
      Sitecore.Forms.Mvc.Models.FormModel formModel2 = new Sitecore.Forms.Mvc.Models.FormModel(formModel.UniqueId)
      {
        IsValid = formModel.IsValid,
        EventCounter = formModel.EventCounter,
        QueryParameters = formModel.QueryParameters,
        ReadQueryString = formModel.ReadQueryString,
        RedirectOnSuccess = formModel.RedirectOnSuccess,
        RenderedTime = formModel.RenderedTime,
        Results = formModel.Results,
        SuccessRedirectUrl = formModel.SuccessRedirectUrl
      };
      formModel2.GetType().GetProperty("Item").SetValue(formModel2, formModel.Item, null);
      return formModel2;
    }
  }
}