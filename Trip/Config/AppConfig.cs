using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trip.Interfaces;
using Trip.Models;

namespace Trip.Config
{
    public class AppConfig : IAppConfig
    {
        public ConfigJSONModel Config {  get; }
        public string FilePath { get; }

        public AppConfig(ConfigJSONModel model, string path)
        {
            Config = model;
            FilePath = path;
        }
    }
}
