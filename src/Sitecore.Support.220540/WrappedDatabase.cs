using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Forms.Mvc.Data.Wrappers;

namespace Sitecore.Support.Forms.Mvc.Data.Wrappers
{
  public class WrappedDatabase : IDatabase
  {
    private readonly Database sitecoreDatabase;

    public WrappedDatabase(Database database)
    {
      sitecoreDatabase = database;
    }

    public Item GetItem(string id)
    {
      return sitecoreDatabase.GetItem(id);
    }

    public Item GetItem(ID id)
    {
      return sitecoreDatabase.GetItem(id);
    }
  }
}