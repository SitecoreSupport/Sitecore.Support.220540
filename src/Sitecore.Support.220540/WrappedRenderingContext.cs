using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Form.Core.Configuration;
using Sitecore.Forms.Mvc.Data.Wrappers;
using Sitecore.Mvc.Presentation;
using System;
using System.Linq;
using System.Web;

namespace Sitecore.Support.Forms.Mvc.Data.Wrappers
{
  public class WrappedRenderingContext : IRenderingContext
  {
    public IRendering Rendering
    {
      get
      {
        if (RenderingContext.CurrentOrNull != null)
        {
          return new WrappedRendering(RenderingContext.CurrentOrNull.Rendering);
        }
        return new WrappedRendering(TryToGetRendering());
      }
    }

    public IDatabase Database
    {
      get
      {
        if (RenderingContext.CurrentOrNull != null)
        {
          return new WrappedDatabase(RenderingContext.CurrentOrNull.PageContext.Database);
        }
        return new WrappedDatabase(Context.Database);
      }
    }

    private Rendering TryToGetRendering()
    {
      string text = HttpContext.Current.Request.QueryString.AllKeys.FirstOrDefault(delegate (string x)
      {
        if (x != null && x.EndsWith("." + Sitecore.Forms.Mvc.Constants.FormItemId))
        {
          return x.StartsWith("wffm");
        }
        return false;
      });
      if (text != null)
      {
        string input = HttpContext.Current.Request.QueryString[text];
        Guid guid = Guid.Parse(input);
        if (guid != Guid.Empty)
        {
          Item item = Database.GetItem(new ID(guid));
          if (item != null)
          {
            Rendering rendering = new Rendering();
            rendering.RenderingItem = item;
            Rendering rendering2 = rendering;
            string text2 = HttpContext.Current.Request.QueryString.AllKeys.FirstOrDefault(delegate (string x)
            {
              if (x != null && x.EndsWith("." + Sitecore.Forms.Mvc.Constants.Id))
              {
                return x.StartsWith("wffm");
              }
              return false;
            });
            string input2 = string.IsNullOrEmpty(text2) ? text.Replace("wffm", "").Replace("." + Sitecore.Forms.Mvc.Constants.FormItemId, "") : HttpContext.Current.Request.QueryString[text2];
            Guid guid3 = rendering2.UniqueId = Guid.Parse(input2);
            rendering2.Parameters[Sitecore.Forms.Mvc.Constants.FormId] = item.ID.ToString();
            rendering2.RenderingItem = Database.GetItem(IDs.FormMvcInterpreterID);
            return rendering2;
          }
        }
      }
      return null;
    }
  }
}