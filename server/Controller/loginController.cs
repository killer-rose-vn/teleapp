using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using System.Xml;
using System.Net.Http;
using System.Collections.Specialized;
using System.Threading;
using System.CodeDom;
using server.Models;
using System.Security.Cryptography;
using System.Text;
using server.Cls;

namespace server.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class loginController : ApiController
    {
        [HttpGet]
        public AjaxResult getU(int tid)
        {
            try
            {
                var db = new db.dcDataContext(dbProvider.connStr);
                var u = db.tbUsers.SingleOrDefault(i => i.tele_id == tid);
                if (u == null)
                
                    return new AjaxResult(0, "Không tìm thấy user");
                
                if (u.status != E.user_status.active.ToString())
                    return new AjaxResult(0, "User bị khoá");

                return new AjaxResult(1, "OK", new
                {
                    tid = u.tele_id,
                    u.id,
                    shortId = $"{u.id.Substring(0, 7)}...{u.id.Substring(u.id.Length - 6)}",
                    createDate = string.Format("{0:yyyy-MM-dd}", u.createDate),
                    u.caption,
                    u.desciption,
                    u.money,
                    u.status
                });
            }
            catch (Exception ex)
            {
                return new AjaxResult(-1, ex.Message);
            }
        }
    }
}

