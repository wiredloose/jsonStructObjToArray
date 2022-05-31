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
            string json = req.Query["json"];
            string jpath = req.Query["jpath"]; // JPath expressions, see https://www.newtonsoft.com/json/help/html/SelectToken.htm 
            string keyToProp = req.Query["keyToProp"];
            string valToProp = req.Query["valToProp"];

            string jsonStr = string.IsNullOrEmpty(json)
                ? "{\"Category\":\"Azure Functions\",\"Action\":\"Run\",\"Label\":\"Test\"}"
                : json;
            string jpathStr = string.IsNullOrEmpty(jpath)
                ? "result.watts"
                : jpath;
            string keyToPropStr = string.IsNullOrEmpty(keyToProp)
                ? "propName"
                : keyToProp;
            string valToPropStr = string.IsNullOrEmpty(valToProp)
                ? "propValue"
                : valToProp;

            JObject jObj = null;

            dynamic jObject = JsonConvert.DeserializeObject(jsonStr);
            dynamic data = JsonConvert.DeserializeObject(await new StreamReader(req.Body).ReadToEndAsync());

            jObj = data ?? jObject;

            JToken jpathObj = null;

            // split jpath string - it supports multiple entries to transform multiple Objects into Arrays
            string[] jpathStrArr = jpathStr.Split(",");
            
            foreach(string pathStr in jpathStrArr)
            {
                // Locate JSON object to be modified
                jpathObj = jObj.SelectToken(pathStr);

                if (jpathObj != null)
                {
                    JObject parsedJson = JObject.Parse(jpathObj.ToString());

                    // Move all properties to array of objects 
                    JArray jToArr = new JArray();
                    foreach (var prop in parsedJson.Properties())
                    {
                        jToArr.Add(
                            new JObject(
                                new JProperty(keyToPropStr, prop.Name),
                                new JProperty(valToPropStr, prop.Value)
                                )
                            );
                    }

                    // Replace property object with version that is array of objects
                    JObject jForReplaceObj = (JObject)jpathObj;
                    JProperty jForReplaceProp = (JProperty)jForReplaceObj.Parent;
                    jForReplaceObj.Parent.Replace(new JProperty(jForReplaceProp.Name, jToArr));
                }
            }


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
