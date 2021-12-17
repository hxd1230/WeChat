using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using StackExchange.Redis;
using WeChat.UI.Models;

namespace WeChat.UI.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private ILog _logger;
        private IMemoryCache _memoryCache;
        private IDistributedCache _cache;
        private readonly IDatabase _redis;
        public UserController(IMemoryCache memoryCache
            , IDistributedCache cache
            , RedisHelper client)
        {
            this._logger = LogManager.GetLogger(Startup.respository.Name, typeof(UserController));
            _memoryCache = memoryCache;
            _cache = cache;
            _redis = client.GetDatabase();
        }
        /// <summary>
        /// 获取用户列表（OpenID）
        /// </summary>
        /// <returns></returns>
        [Route("all"), HttpGet]
        public string Get()
        {
            //string access_token = _memoryCache.Get<string>("access_token");
            //string access_token = _cache.GetString("access_token");
            string access_token = _redis.StringGet("access_token");
            if (string.IsNullOrEmpty(access_token))
            {
                JObject jObject = (JObject)JsonConvert.DeserializeObject(GetAccessToken());
                access_token = jObject["access_token"].ToString();
                //_memoryCache.Set<string>("access_token", access_token);
                _redis.StringSet("access_token", access_token);
            }
            _logger.Info("access_token=" + access_token);
            string url = $"https://api.weixin.qq.com/cgi-bin/user/get?access_token={access_token}";
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.Timeout = 10000;
            var response = client.Execute(request);
            _logger.Info("Content=" + response.Content);
            return response.Content;
        }
        private string GetAccessToken()
        {
            //通过code获取access_token
            string url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={WeChatConfig.APP_ID}&secret={WeChatConfig.APP_SECRET}";
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.Timeout = 10000;
            IRestResponse response = client.Execute(request);
            return response.Content;
        }
        /// <summary>
        /// 获取用户基本信息（OpenID）
        /// </summary>
        /// <returns></returns>
        [Route("info"), HttpGet]
        public string GetInfo(string openId)
        {
            //string access_token = _memoryCache.Get<string>("access_token");
            //string access_token = _cache.GetString("access_token");
            string access_token = _redis.StringGet("access_token");
            if (string.IsNullOrEmpty(access_token))
            {
                JObject jObject = (JObject)JsonConvert.DeserializeObject(GetAccessToken());
                access_token = jObject["access_token"].ToString();
                //_memoryCache.Set<string>("access_token", access_token);
                //_cache.SetString("access_token", access_token);
                _redis.StringSet("access_token", access_token);
            }
            _logger.Info("access_token=" + access_token);
            string url = $"https://api.weixin.qq.com/cgi-bin/user/info?access_token={access_token}&openid={openId}&lang=zh_CN";
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.Timeout = 10000;
            var response = client.Execute(request);
            return response.Content;
        }
    }
}