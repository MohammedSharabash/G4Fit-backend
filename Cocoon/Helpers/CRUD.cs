using G4Fit.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Helpers
{
    public static class CRUD<T> where T: BaseModel
    {
        public static void Update(T obj)
        {
            obj.IsModified = true;
            obj.ModifiedOn = DateTime.Now.ToUniversalTime();
        }

        public static void  Delete(T obj)
        {
            obj.IsDeleted = true;
            obj.DeletedOn = DateTime.Now.ToUniversalTime();
        }

        public static void Restore(T obj)
        {
            obj.IsDeleted = false;
            obj.DeletedOn = null;
            obj.RestoredOn = DateTime.Now.ToUniversalTime();
        }
    }
}