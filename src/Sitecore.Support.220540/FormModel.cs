using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Forms.Core.Data;
using Sitecore.Forms.Mvc.Interfaces;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.WFFM.Abstractions.Actions;
using Sitecore.WFFM.Abstractions.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Sitecore.Support.Forms.Mvc.Models
{
  public class FormModel : IFormModel, IModelEntity, ICloneable
  {
    public IFormItem Item { get; private set; }

    public List<ExecuteResult.Failure> Failures { get; set; }

    public string SuccessRedirectUrl { get; set; }

    public bool RedirectOnSuccess { get; set; }

    public DateTime RenderedTime { get; set; }

    public bool ReadQueryString { get; set; }

    public NameValueCollection QueryParameters { get; set; }

    public int EventCounter { get; set; }

    public List<ControlResult> Results { get; set; }

    public bool IsValid { get; set; }

    public Guid UniqueId { get; private set; }
    public FormModel(Guid uniqueId)
    {
      Assert.ArgumentCondition(uniqueId != Guid.Empty, "uniqueId", "uniqueId is empty");
      this.UniqueId = uniqueId;
      this.Results = new List<ControlResult>();
      this.Failures = new List<ExecuteResult.Failure>();
    }

    public FormModel(Guid uniqueId, Sitecore.Data.Items.Item item) : this(uniqueId)
    {
      Assert.ArgumentNotNull(item, "item");
      this.Item = new FormItem(item);
      this.RedirectOnSuccess = this.Item.SuccessRedirect;
      this.IsValid = true;
      if (this.RedirectOnSuccess)
      {
        LinkField successPage = this.Item.SuccessPage;
        if (successPage != null)
        {
          UrlString str = null;
          if (successPage.LinkType == "external")
          {
            str = new UrlString(successPage.Url);
          }
          else
          {
            if (successPage.TargetItem == null)
            {
              Log.Error("[WFFM] [Sitecore.Support.220540] Redirect item is null", new NullReferenceException(), this);
              //throw new NullReferenceException("Redirect item is null");
            }
            string linkType = successPage.LinkType;
            if (linkType != null)
            {
              if (linkType != "internal" && linkType == "media")
              {
                str = new UrlString(MediaManager.GetMediaUrl(new MediaItem(successPage.TargetItem)));
              }
              else if (!successPage.TargetID.IsNull)
              {
                UrlOptions defaultUrlOptions = LinkManager.GetDefaultUrlOptions();
                defaultUrlOptions.Language = item.Language;
                defaultUrlOptions.SiteResolving = Settings.Rendering.SiteResolving;
                Item item2 = default(Item);
                using (new SecurityDisabler())
                {
                  item2 = item.Database.Items[successPage.TargetID];
                }
                str = new UrlString(LinkManager.GetItemUrl(item2, defaultUrlOptions));
              }
              /*if (linkType == "internal")
              {
                UrlOptions defaultUrlOptions = LinkManager.GetDefaultUrlOptions();
                defaultUrlOptions.SiteResolving = Settings.Rendering.SiteResolving;
                str = new UrlString(LinkManager.GetItemUrl(successPage.TargetItem, defaultUrlOptions));
              }
              else if (linkType == "media")
              {
                str = new UrlString(MediaManager.GetMediaUrl(new MediaItem(successPage.TargetItem)));
              }*/
            }
          }
          if (str != null)
          {
            string queryString = this.Item.SuccessPage.QueryString;
            if (!string.IsNullOrEmpty(queryString))
            {
              str.Parameters.Add(WebUtil.ParseUrlParameters(queryString));
            }
            this.SuccessRedirectUrl = str.ToString();
          }
        }
      }
    }

    public object Clone()
    {
      FormModel model = (FormModel)base.MemberwiseClone();
      model.Results = new List<ControlResult>();
      model.Failures = new List<ExecuteResult.Failure>();
      return model;
    }
  }
}