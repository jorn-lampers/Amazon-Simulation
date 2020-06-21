using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Controllers
{
    public abstract class Command
    {
        protected Guid id;
        public string type { get { return this.GetType().Name; } }
        protected Object parameters;

        public Command()
        {
            this.id = Guid.NewGuid();
        }

        public Command(Object parameters) : this() {
            this.parameters = parameters;
        }

        public string ToJson() {
            return JsonConvert.SerializeObject(new {
                id = id,
                command = type,
                parameters = parameters
            });
        }
    }
}