using Easycaching.Hybrid.Sentinel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Easycaching.Hybrid.Sentinel.infra
{
    public class CachedAttribute : ActionFilterAttribute
    {
        private readonly int _ttlPercisionValue;
        private readonly CacheTTLPercision _ttlPercision;

        /// <summary>
        /// default ttl set to 10 second
        /// </summary>
        public CachedAttribute()
        {
            _ttlPercisionValue = 10;
            _ttlPercision = CacheTTLPercision.Second;
        }

        /// <param name="percision"></param>
        /// <param name="percisionValue"></param>
        public CachedAttribute(CacheTTLPercision percision, int percisionValue)
        {
            _ttlPercisionValue = percisionValue;
            _ttlPercision = percision;
        }


        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();

            // check if request has Cache
            var cacheKey = GenerateCacheKeyFromRequest(context);
            var cachedResponse = await cacheService.GetCachedResponseAsync(cacheKey);


            // If Yes => return Value
            if (!string.IsNullOrEmpty(cachedResponse))
            {
                var contentResult = new ContentResult
                {
                    Content = cachedResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result = contentResult;
                return;
            }


            // If No => Go to method => Cache Value
            var actionExecutedContext = await next();
            if (actionExecutedContext.Result != null)
            {
                if (actionExecutedContext.Result?.GetType() == typeof(ObjectResult))
                {
                    dynamic result = ((ObjectResult)actionExecutedContext.Result).Value;
                    if (result.Data != null)
                    {
                        var res = JsonConvert.SerializeObject(result.Data);
                        await cacheService.CacheResponseAsync(cacheKey, res, TTLToCache);
                    }
                }
            }
        }


        private TimeSpan TTLToCache
        {
            get
            {
                return _ttlPercision switch
                {
                    CacheTTLPercision.Day => TimeSpan.FromDays(_ttlPercisionValue),
                    CacheTTLPercision.Hour => TimeSpan.FromHours(_ttlPercisionValue),
                    CacheTTLPercision.Minute => TimeSpan.FromMinutes(_ttlPercisionValue),
                    CacheTTLPercision.Second => TimeSpan.FromSeconds(_ttlPercisionValue),
                    _ => TimeSpan.FromSeconds(_ttlPercisionValue),
                };
            }
        }





        // Generate Cache Key
        private static string GenerateCacheKeyFromRequest(ActionExecutingContext context)
        {
            try
            {
                var httpRequest = context.HttpContext.Request;
                var keyBuilder = new StringBuilder();
                keyBuilder.Append($"{httpRequest.Path.ToString().Replace("/api/", "")}");
                var bodyKey = ReadBodyAsKeyAccomplisher(context);
                keyBuilder.Append($"{bodyKey}");
                return keyBuilder.ToString();
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }




        private static string ReadBodyAsKeyAccomplisher(ActionExecutingContext context)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (var arg in context.ActionArguments)
                {
                    if (arg.Value.GetType().IsValueType)
                    {
                        // param is value type
                        sb.Append($"|{arg.Key.Substring(0, 3)}-{arg.Value}");
                    }
                    else if (arg.Value.GetType().IsClass)
                    {
                        // param is reference type
                        var temp = JsonConvert.SerializeObject(arg.Value);
                        var objStr = temp.Replace("{", "").Replace("[", "").Replace("\"", "").Replace(",", "|").Replace("}", "").Replace("]", "");
                        sb.Append($"|{objStr}");
                    }
                }
            }
            catch (Exception)
            {
                return sb.ToString();
            }

            return sb.ToString();
        }
    }




    public enum CacheTTLPercision
    {
        Day,
        Hour,
        Minute,
        Second
    }
}
