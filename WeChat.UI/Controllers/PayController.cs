using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Text;
using System.Web;
using WeChat.UI.Models;

namespace WeChat.UI.Controllers
{
    /// <summary>
    /// 支付管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PayController : ControllerBase
    {
        private ILog _logger;
        /// <summary>
        /// ctor
        /// </summary>
        public PayController()
        {
            this._logger = LogManager.GetLogger(Startup.respository.Name, typeof(PayController));
        }

        public class Data
        {
            public string openid { get; set; }
        }
        /// <summary>
        /// 准备获取Code
        /// </summary>
        [Route("pre_code"), HttpGet]
        public void GetPreCode()
        {
            string url = string.Empty;
            try
            {
                string host = "hexiaodong.top/";
                string path = "api/pay/code";
                string redirect_uri = HttpUtility.UrlEncode("http://" + host + path);
                WeChatPayData data = new WeChatPayData();
                data.SetValue("appid", WeChatConfig.APP_ID);
                data.SetValue("redirect_uri", redirect_uri);
                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_userinfo");
                //data.SetValue("connect_redirect", "1");
                data.SetValue("state", "1" + "#wechat_redirect");
                url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                _logger.Info("url地址为：" + url);
            }
            catch (Exception ex)
            {
                _logger.Info("错误信息为：" + ex);
            }
            ControllerContext.HttpContext.Response.Redirect(url);
            //var client = new RestClient(url)
            //{
            //    UserAgent = "Mozilla/5.0 (Linux; Android 6.0; 1503-M02 Build/MRA58K) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/37.0.0.0 Mobile MQQBrowser/6.2 TBS/036558 Safari/537.36 MicroMessenger/6.3.25.861 NetType/WIFI Language/zh_CN",
            //    Encoding = Encoding.UTF8
            //};
        }
        /// <summary>
        /// 获取Code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        [Route("code"), HttpGet]
        public void GetCode(string code, string state)
        {
            ControllerContext.HttpContext.Response.Redirect("http://hexiaodong.top/product/item?code=" + code);
        }
        /// <summary>
        /// 获取access_token
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Route("access_token"), HttpGet]
        public string GetAccessToken(string code)
        {
            //通过code获取access_token
            string url = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid=wxd001485d83e3628b&secret={WeChatConfig.APP_SECRET}&code={code}&grant_type=authorization_code";
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.Timeout = 10000;
            IRestResponse response = client.Execute(request);
            return response.Content;
        }
        /// <summary>
        /// 支付
        /// </summary>
        /// <returns></returns>
        [Route("unifiedorder"), HttpGet]
        public string Post()
        {
            //通过code获取access_token
            string code = Request.Query["code"];
            string url = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid=wxd001485d83e3628b&secret={WeChatConfig.APP_SECRET}&code={code}&grant_type=authorization_code";
            _logger.Info("unifiedorder->请求access_token地址为：" + url);
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.Timeout = 10000;
            IRestResponse response = client.Execute(request);
            _logger.Info("unifiedorder->请求access_token返回内容为：" + url);
            string openid = JsonConvert.DeserializeObject<Data>(response.Content)?.openid;
            _logger.Info("获取到openid为：" + openid);
            _logger.Info("开始支付：" + url);
            url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            client = new RestClient(url);
            request = new RestRequest(Method.POST);
            request.Timeout = 10000;
            WeChatPayData payData = new WeChatPayData();
            payData.SetValue("appid", WeChatConfig.APP_ID);
            payData.SetValue("mch_id", WeChatConfig.MCH_ID);
            payData.SetValue("nonce_str", payData.GenerateNonceStr());
            payData.SetValue("body", "test");
            payData.SetValue("out_trade_no", DateTime.Now.ToString("yyyyMMddHHmmss"));
            payData.SetValue("total_fee", 1);
            payData.SetValue("spbill_create_ip", "127.0.0.1");
            payData.SetValue("notify_url", "http://hexiaodong.top/api/pay/notify_url");
            payData.SetValue("trade_type", "JSAPI");
            payData.SetValue("openid", openid);//"oTSBW5wX09Qwiidfk1sarDeXq-hY"
            payData.SetValue("sign_type", WeChatPayData.SIGN_TYPE_HMAC_SHA256);//签名类型
            payData.SetValue("sign", payData.MakeSign());
            string xml = payData.ToXml();
            request.AddJsonBody(xml);
            response = client.Execute(request);
            payData.FromXml(response.Content);
            WeChatPayData jsApiParam = new WeChatPayData();
            jsApiParam.SetValue("appId", WeChatConfig.APP_ID);
            jsApiParam.SetValue("timeStamp", jsApiParam.GenerateTimeStamp());
            jsApiParam.SetValue("nonceStr", jsApiParam.GenerateNonceStr());
            jsApiParam.SetValue("package", "prepay_id=" + payData.GetValue("prepay_id"));
            jsApiParam.SetValue("signType", WeChatPayData.SIGN_TYPE_HMAC_SHA256);
            jsApiParam.SetValue("paySign", jsApiParam.MakeSign());
            return jsApiParam.ToJson();
        }
        /// <summary>
        /// 支付回调
        /// </summary>
        [Route("notify_url"), HttpGet]
        public void GetNotifyUrl()
        {
            //接收从微信后台POST过来的数据
            StringBuilder builder = new StringBuilder();
            using (Stream s = Request.Body)
            {
                using (StreamReader sr = new StreamReader(s))
                    builder.Append(sr.ReadToEnd());
            }
            WeChatPayData data = new WeChatPayData();
            data.FromXml(builder.ToString());
            _logger.Info("支付成功回调参数：" + data.ToJson());
            ControllerContext.HttpContext.Response.Redirect("http://hexiaodong.top/product/callback?data=" + data.ToJson());
        }
    }
}