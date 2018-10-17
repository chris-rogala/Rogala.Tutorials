using FilterExample.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using FilterExample.Extensions;
using Microsoft.Extensions.Primitives;

namespace FilterExample.Concurrency
{
    public abstract class EnableConcurrenyAttributeBase : ActionFilterAttribute
    {
        public static Func<object, string> CalculateConcurrencyValue = (result) =>
        {
            byte[] hashBytes = new byte[0];
            if (result != null)
            {
                using (var md5Check = System.Security.Cryptography.MD5.Create())
                {
                    var objectBytes = ObjectToByteArray(result);
                    md5Check.TransformBlock(objectBytes, 0, objectBytes.Length, null, 0);
                    md5Check.TransformFinalBlock(new byte[0], 0, 0);

                    hashBytes = md5Check.Hash;
                }
            }

            return $"\"{Convert.ToBase64String(hashBytes)}\"";
        };

        private bool _preconditionRequired;
        public bool IsCreateAction { get; set; }

        protected EnableConcurrenyAttributeBase(bool preconditionRequired = false)
        {
            _preconditionRequired = preconditionRequired;
            IsCreateAction = false;
        }

        protected Type ResourceType { get; set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ObjectResult result = null;

            if (!IsCreateAction)
            {
                var applicationContext = context.HttpContext.RequestServices.GetService<ApplicationContext>();
                
                StringValues ifMatchValues;
                if (context.HttpContext.Request.Headers.TryGetValue("if-match", out ifMatchValues))
                {
                    if (ifMatchValues.Count == 1)
                    {
                        applicationContext.RequestCheckSum = ifMatchValues[0];
                    }
                }

                if (_preconditionRequired && string.IsNullOrWhiteSpace(applicationContext.RequestCheckSum))
                {
                    result = new ObjectResult((context.Result as ObjectResult)?.Value) { StatusCode = 428 };
                }

                var existing = await FindExistingAsync(context);

                if (existing == null)
                {
                    result = new ObjectResult(null) { StatusCode = 404 };
                }
                else
                {
                    applicationContext.AddObject(ResourceType, existing);
                    var checkSum = CalculateConcurrencyValue(existing);
                    applicationContext.CurrentCheckSum = checkSum;
                    applicationContext.ResponseCheckSum = checkSum;
                }

                if (!string.IsNullOrWhiteSpace(applicationContext.RequestCheckSum))
                {
                    if (_preconditionRequired && applicationContext.CurrentCheckSum != applicationContext.RequestCheckSum)
                    {
                        result = new ObjectResult(new { ValidationResults = new[] { new { Message = "Replace with errors", Severity = "Error" } } }) { StatusCode = 412 };
                    }
                    else if (!_preconditionRequired && applicationContext.RequestCheckSum == applicationContext.CurrentCheckSum)
                    {
                        result = new ObjectResult(new { ValidationResults = new[] { new { Message = "Object was not modified", Severity = "Information" } } }) { StatusCode = 304 };
                    }
                }
            }

            if (result != null)
            {
                context.Result = result;
            }
            else
            {
                await next();
            }
        }

        public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var objectResult = context.Result as ObjectResult;
            if (objectResult != null && objectResult.StatusCode >= 200 & objectResult.StatusCode < 400)
            {
                var applicationContext = context.HttpContext.RequestServices.GetService<ApplicationContext>();

                context.HttpContext.Response.Headers.Add(
                        "ETag", new string[] { $"{applicationContext.ResponseCheckSum}" });
            }

            return base.OnResultExecutionAsync(context, next);
        }

        protected abstract Task<object> FindExistingAsync(ActionExecutingContext context);
        private static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
