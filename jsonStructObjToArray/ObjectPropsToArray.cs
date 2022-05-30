using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace jsonStructObjToArray
{
    public static class ObjectPropsToArray
    {
        [FunctionName("ObjectPropsToArray")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string json = req.Query["json"];
            string jpath = req.Query["jpath"]; // JPath expressions, see https://www.newtonsoft.com/json/help/html/SelectToken.htm 
            
            string jsonStr = string.IsNullOrEmpty(json)
                ? "{\"Category\":\"Azure Functions\",\"Action\":\"Run\",\"Label\":\"Test\"}"
                : json;
            string jpathStr = string.IsNullOrEmpty(jpath)
                ? "result.watts"
                : jpath;
            
            dynamic jObject = JsonConvert.DeserializeObject(jsonStr);

            //Modify JSON object
            JObject jObj = jObject;
            JToken jpathObj = jObj.SelectToken(jpathStr);
            JObject parsedJson = JObject.Parse(jpathObj.ToString());
            
            foreach (var prop in parsedJson.Properties())
            {
                
                log.LogInformation(jpathStr + " property name: " + prop.Name +" with value: " + prop.Value);
            }

            //jpathObj.Remove();
            JObject jResultObj = (JObject)jObj.SelectToken("result");
            log.LogInformation("removing watts successfully?: " + jResultObj.Remove("watts"));
            log.LogInformation("jObj after removal: " + jObj.ToString());
            log.LogInformation("parsedJson after removal: " + parsedJson.ToString());

            //return new OkObjectResult(jObject);
            return new OkObjectResult(jObj);
        }
    }

    public class sampleDataObj
    {
        public string propName { get; set; }
        public string propValue { get; set; }
    }


}
