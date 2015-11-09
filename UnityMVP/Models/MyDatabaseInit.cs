using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using WebMatrix.WebData;

namespace UnityMVP.Models
{
    public class MyDatabaseInit : DropCreateDatabaseAlways<UsersContext>
    {
        //
        // GET: /MyDatabaseInit/
        protected override void Seed(UsersContext context)
        {
        }

       


}
}
