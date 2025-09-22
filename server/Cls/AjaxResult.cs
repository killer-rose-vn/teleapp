using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace server.Models
{
    public class AjaxResult
    {
        public int stt { get; set; }
        public string msg { get; set; }
        public object data { get; set; }

        public AjaxResult() { }
        public AjaxResult(int stt, string msg, object data = null)
        {
            this.stt = stt;
            this.msg = msg.Replace("\r", "").Replace("\r", "\\");
            this.data = data;
        }

        public override string ToString()
        {
            return $"stt: {stt}, msg: {msg}, data: {data}";
        }
    }
}
