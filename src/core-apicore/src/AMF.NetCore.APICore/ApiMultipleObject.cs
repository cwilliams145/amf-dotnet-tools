﻿using System.Collections.Generic;
using System.Net;

namespace AMF.Api.Core
{
    public class ApiMultipleObject
    {
        protected IDictionary<HttpStatusCode, string> names = new Dictionary<HttpStatusCode, string>();
        protected IDictionary<HttpStatusCode, System.Type> types = new Dictionary<HttpStatusCode, System.Type>();

        public System.Type GetTypeByStatusCode(HttpStatusCode statusCode)
        {
            if (types.ContainsKey(statusCode))
                return types[statusCode];

            return null;
        }
 
    }
}